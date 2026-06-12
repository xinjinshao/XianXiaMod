from __future__ import annotations

import re
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]


def read_all(pattern: str) -> list[tuple[Path, str]]:
    return [(path, path.read_text(encoding="utf-8")) for path in ROOT.rglob(pattern)]


def localization_keys_from_code() -> set[str]:
    keys: set[str] = set()
    bestiary_pattern = re.compile(r'FlavorTextBestiaryInfoElement\("Mods\.XianXia\.Bestiary\.([^"]+)"\)')
    language_pattern = re.compile(r'Language\.GetTextValue\("Mods\.XianXia\.([^"]+)"')
    config_class_pattern = re.compile(r"class\s+([A-Za-z0-9_]+)\s*:\s*ModConfig\b")
    config_member_pattern = re.compile(r"public\s+(?:[A-Za-z0-9_<>,.?]+)\s+([A-Za-z0-9_]+)\s*\{\s*get;\s*set;\s*\}")
    for path, text in read_all("*.cs"):
        if "FlavorTextBestiaryInfoElement" in text:
            keys.update(f"Bestiary.{key}" for key in bestiary_pattern.findall(text))
        if "Language.GetTextValue" in text:
            keys.update(language_pattern.findall(text))
        if ": ModConfig" in text:
            for config_name in config_class_pattern.findall(text):
                keys.add(f"Configs.{config_name}.DisplayName")
                for member_name in config_member_pattern.findall(text):
                    keys.add(f"Configs.{config_name}.{member_name}.Label")
                    keys.add(f"Configs.{config_name}.{member_name}.Tooltip")
    return keys


def localization_keys() -> set[str]:
    keys: set[str] = set()
    key_pattern = re.compile(r"^\s*([A-Za-z0-9_.-]+)\s*:\s*(.*)$")

    for _, text in read_all("*.hjson"):
        stack: list[str] = []
        for raw_line in text.splitlines():
            line = raw_line.strip()
            if not line or line.startswith("#"):
                continue
            if line.startswith("}"):
                if stack:
                    stack.pop()
                continue

            match = key_pattern.match(raw_line)
            if match is None:
                continue

            key, value = match.groups()
            if value.strip().startswith("{"):
                stack.append(key)
                continue

            full_key = ".".join(stack + [key])
            if full_key.startswith("Mods.XianXia."):
                keys.add(full_key.removeprefix("Mods.XianXia."))

    return keys


def main() -> int:
    available = localization_keys()
    missing = sorted(key for key in localization_keys_from_code() if key not in available)
    if missing:
        print("Missing localization keys:")
        for key in missing:
            print(f"  Mods.XianXia.{key}")
        return 1

    print("Localization keys verified.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
