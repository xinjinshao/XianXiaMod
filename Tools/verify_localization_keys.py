from __future__ import annotations

import re
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]


def read_all(pattern: str) -> list[tuple[Path, str]]:
    return [(path, path.read_text(encoding="utf-8")) for path in ROOT.rglob(pattern)]


def bestiary_keys_from_code() -> set[str]:
    keys: set[str] = set()
    pattern = re.compile(r'FlavorTextBestiaryInfoElement\("Mods\.XianXia\.Bestiary\.([^"]+)"\)')
    for path, text in read_all("*.cs"):
        if "FlavorTextBestiaryInfoElement" not in text:
            continue
        keys.update(pattern.findall(text))
    return keys


def hjson_text() -> str:
    return "\n".join(text for _, text in read_all("*.hjson"))


def has_hjson_key(text: str, key: str) -> bool:
    parts = key.split(".")
    if len(parts) == 1:
        return re.search(rf"(?m)^\s*{re.escape(parts[0])}\s*:", text) is not None

    parent = re.escape(parts[0])
    child = re.escape(parts[1])
    return re.search(rf"(?s)(?m)^\s*{parent}\s*:\s*\{{.*?^\s*{child}\s*:", text) is not None


def main() -> int:
    localization = hjson_text()
    missing = sorted(key for key in bestiary_keys_from_code() if not has_hjson_key(localization, key))
    if missing:
        print("Missing Bestiary localization keys:")
        for key in missing:
            print(f"  Mods.XianXia.Bestiary.{key}")
        return 1

    print("Bestiary localization keys verified.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
