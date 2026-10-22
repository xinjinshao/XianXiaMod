"""Run registered integration checks in an isolated generated world or copied save."""
import argparse
import re
import hashlib
import json
import os
from pathlib import Path
import shutil
import socket
import subprocess
import time
import uuid


FATAL_MARKERS = ("Load failed!  No backup found.", "Main engine crash")


def log_tail(path, limit=65536):
    """Read bounded current-run output without loading an ever-growing log."""
    if not path.exists():
        return ""
    with path.open("rb") as stream:
        stream.seek(0, 2)
        stream.seek(max(0, stream.tell() - limit))
        return stream.read(limit).decode("utf-8", errors="replace")


def validate_report(result, report):
    if not isinstance(result, dict) or type(result.get("schema")) is not int or result["schema"] != 1:
        raise RuntimeError(f"Invalid summon report schema; inspect {report}")
    if result.get("passed") is not True:
        raise RuntimeError(f"Summon audit failed: {result.get('error')}; report: {report}")
    checks = result.get("checks")
    if not isinstance(checks, list) or not checks or any(not isinstance(check, str) or not check.strip() for check in checks):
        raise RuntimeError(f"Missing or invalid summon check evidence; inspect {report}")
    if not isinstance(result.get("limitations"), str) or not result["limitations"].strip():
        raise RuntimeError(f"Missing summon evidence limitations; inspect {report}")
    return result


def wait_for_report(process, report, output, timeout):
    deadline = time.monotonic() + timeout
    while True:
        exit_code = process.poll()
        tail = log_tail(output)
        marker = next((marker for marker in FATAL_MARKERS if marker in tail), None)
        if marker:
            raise RuntimeError(f"Server failed before audit acceptance ({marker}); inspect {output}")
        incomplete = False
        if report.exists():
            try:
                result = json.loads(report.read_text(encoding="utf-8"))
            except (json.JSONDecodeError, UnicodeDecodeError):
                incomplete = True
            else:
                return validate_report(result, report)
        if exit_code is not None:
            raise RuntimeError(f"Server exited with code {exit_code} without a complete summon report; inspect {output}")
        if time.monotonic() >= deadline:
            state = "incomplete JSON" if incomplete else "no report"
            raise RuntimeError(f"Timed out waiting for summon audit ({state}); inspect {output}")
        time.sleep(0.25)


def extra_mods(entries):
    result = {}
    seen = {"xianxia"}
    for entry in entries:
        name, separator, filename = entry.partition("=")
        if not separator or not re.fullmatch(r"[A-Za-z][A-Za-z0-9_]*", name):
            raise ValueError("Extra mod must be NAME=PATH with an internal alphanumeric/underscore mod name")
        if name.casefold() in seen or re.fullmatch(r"CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9]", name, re.I):
            raise ValueError("Duplicate, primary or reserved extra mod name: " + name)
        package = Path(filename).resolve(strict=True)
        if not package.is_file() or package.suffix.lower() != ".tmod":
            raise ValueError("Extra mod package must be a .tmod file")
        seen.add(name.casefold())
        result[name] = package
    return result


def validate_scope(result, scope):
    if result.get("scope") != scope:
        raise RuntimeError(f"Audit scope mismatch: requested {scope}, received {result.get('scope')}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--dotnet", required=True)
    parser.add_argument("--tml", required=True, type=Path)
    parser.add_argument("--package", required=True, type=Path)
    world_choice = parser.add_mutually_exclusive_group(required=True)
    world_choice.add_argument("--world", type=Path)
    world_choice.add_argument("--generate-world", action="store_true", help="Generate an isolated small world instead of copying an existing save")
    parser.add_argument("--seed", default="20261020")
    parser.add_argument("--timeout", type=int, default=180)
    parser.add_argument("--extra-mod", action="append", default=[], metavar="NAME=PATH", help="Copy and enable an explicit optional dependency in the disposable mod directory; repeat for dependencies")
    parser.add_argument("--scope", choices=("full", "calamity-recipes"), default="full")
    args = parser.parse_args()
    dependencies = extra_mods(args.extra_mod)
    tml = args.tml.resolve(strict=True)
    package = args.package.resolve(strict=True)
    if args.timeout <= 0:
        parser.error("timeout must be positive")
    if not args.seed or any(c in args.seed for c in "\r\n"):
        parser.error("seed must be a nonempty single line")
    original_hashes = {path: hashlib.sha256(path.read_bytes()).hexdigest() for path in dependencies.values()}
    source = args.world.resolve(strict=True) if args.world else None
    if source:
        companion = source.with_suffix(".twld").resolve(strict=True)
        if source.suffix.lower() != ".wld":
            parser.error("Expected a .wld world with its .twld companion")
        original_hashes.update({path: hashlib.sha256(path.read_bytes()).hexdigest() for path in (source, companion)})
    if package.suffix.lower() != ".tmod":
        parser.error("Expected a .tmod package")
    root = Path(__file__).resolve().parents[1] / ".tml-test" / ("summons-" + uuid.uuid4().hex)
    save = root / "save"
    mods = save / "Mods"
    worlds = save / "Worlds"
    mods.mkdir(parents=True)
    worlds.mkdir()
    world = worlds / "SummonProbe.wld"
    if source:
        shutil.copy2(source, world)
        shutil.copy2(companion, world.with_suffix(".twld"))
    shutil.copy2(package, mods / "XianXia.tmod")
    for name, dependency in dependencies.items():
        shutil.copy2(dependency, mods / (name + ".tmod"))
    (mods / "enabled.json").write_text(json.dumps(["XianXia", *dependencies]), encoding="utf-8")
    with socket.socket() as listener:
        listener.bind(("127.0.0.1", 0))
        port = listener.getsockname()[1]
    config = root / "serverconfig.txt"
    config.write_text("\n".join([
        f"world={world}", *([] if source else ["autocreate=1", f"seed={args.seed}", "worldname=SummonProbe", "difficulty=0"]), "maxplayers=1", "ip=127.0.0.1", f"port={port}",
        f"password={uuid.uuid4().hex}", "upnp=0", "language=en-US",
        f"banlist={root / 'banlist.txt'}", "priority=3",
    ]), encoding="utf-8")
    environment = os.environ.copy()
    environment["XIANXIA_AUDIT_SUMMONS"] = "1"
    environment["XIANXIA_AUDIT_SCOPE"] = args.scope
    report = save / "XianXia" / "summon-audit.json"
    print(f"Isolated summon run: {root}", flush=True)
    with (root / "server.log").open("w", encoding="utf-8") as output, \
            (root / "server.err.log").open("w", encoding="utf-8") as errors:
        process = subprocess.Popen([
            args.dotnet, "tModLoader.dll", "-server", "-nosteam", "-noupnp",
            "-config", str(config), "-tmlsavedirectory", str(save), "-modpath", str(mods),
        ], cwd=tml, env=environment, stdin=subprocess.PIPE, stdout=output,
            stderr=errors, text=True)
        try:
            result = wait_for_report(process, report, root / "server.log", args.timeout)
            validate_scope(result, args.scope)
            # Redirected Windows servers cannot reliably consume console shutdown.
            # This disposable world is deliberately never saved back to its source.
            if process.poll() is None:
                process.kill()
            process.communicate(timeout=30)
        finally:
            if process.poll() is None:
                process.kill()
                process.communicate()
    engine_log = tml / "tModLoader-Logs" / "server.log"
    if engine_log.exists():
        shutil.copy2(engine_log, root / "runtime.log")
    if (root / "server.err.log").read_text(encoding="utf-8").strip():
        raise RuntimeError(f"Server wrote stderr; inspect {root}")
    if any(hashlib.sha256(path.read_bytes()).hexdigest() != digest for path, digest in original_hashes.items()):
        raise RuntimeError("Source world or dependency package changed during the isolated audit")
    print(f"Registered {args.scope} integration passed: {len(result['checks'])} checks. Report: {report}")
    print(result["limitations"])


if __name__ == "__main__":
    main()
