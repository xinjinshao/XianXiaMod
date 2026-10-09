"""Run opt-in NewNPC integration checks in a disposable copy of a saved world."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import socket
import subprocess
import time
import uuid


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--dotnet", required=True)
    parser.add_argument("--tml", required=True, type=Path)
    parser.add_argument("--package", required=True, type=Path)
    parser.add_argument("--world", required=True, type=Path)
    parser.add_argument("--timeout", type=int, default=180)
    args = parser.parse_args()
    tml = args.tml.resolve(strict=True)
    package = args.package.resolve(strict=True)
    source = args.world.resolve(strict=True)
    companion = source.with_suffix(".twld").resolve(strict=True)
    original_hashes = {path: hashlib.sha256(path.read_bytes()).hexdigest() for path in (source, companion)}
    if source.suffix.lower() != ".wld" or package.suffix.lower() != ".tmod":
        parser.error("Expected a .wld world with its .twld companion and a .tmod package")
    if args.timeout <= 0:
        parser.error("timeout must be positive")
    root = Path(__file__).resolve().parents[1] / ".tml-test" / ("summons-" + uuid.uuid4().hex)
    save = root / "save"
    mods = save / "Mods"
    worlds = save / "Worlds"
    mods.mkdir(parents=True)
    worlds.mkdir()
    world = worlds / "SummonProbe.wld"
    shutil.copy2(source, world)
    shutil.copy2(companion, world.with_suffix(".twld"))
    shutil.copy2(package, mods / "XianXia.tmod")
    (mods / "enabled.json").write_text('["XianXia"]', encoding="utf-8")
    with socket.socket() as listener:
        listener.bind(("127.0.0.1", 0))
        port = listener.getsockname()[1]
    config = root / "serverconfig.txt"
    config.write_text("\n".join([
        f"world={world}", "maxplayers=1", "ip=127.0.0.1", f"port={port}",
        f"password={uuid.uuid4().hex}", "upnp=0", "language=en-US",
        f"banlist={root / 'banlist.txt'}", "priority=3",
    ]), encoding="utf-8")
    environment = os.environ.copy()
    environment["XIANXIA_AUDIT_SUMMONS"] = "1"
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
            deadline = time.monotonic() + args.timeout
            while not report.exists() and process.poll() is None and time.monotonic() < deadline:
                time.sleep(0.25)
            if not report.exists():
                raise RuntimeError(f"No summon report; inspect {root}")
            # The report is written after entity/player cleanup. Wait for complete JSON.
            while True:
                try:
                    result = json.loads(report.read_text(encoding="utf-8"))
                    break
                except json.JSONDecodeError:
                    if time.monotonic() >= deadline:
                        raise
                    time.sleep(0.25)
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
    if result.get("schema") != 1 or result.get("passed") is not True:
        raise RuntimeError(f"Summon audit failed: {result.get('error')}; report: {report}")
    if (root / "server.err.log").read_text(encoding="utf-8").strip():
        raise RuntimeError(f"Server wrote stderr; inspect {root}")
    if any(hashlib.sha256(path.read_bytes()).hexdigest() != digest for path, digest in original_hashes.items()):
        raise RuntimeError("Source world changed during the isolated audit")
    print(f"Registered summon integration passed: {len(result['checks'])} checks. Report: {report}")
    print(result["limitations"])


if __name__ == "__main__":
    main()
