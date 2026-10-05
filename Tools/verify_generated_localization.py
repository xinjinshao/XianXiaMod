"""Check the active localization generator without rewriting the working tree."""
from __future__ import annotations

import generate_tmod_content as generator


def normalized_lines(text: str) -> list[str]:
    # Historical Windows output has CRCRLF line breaks. Empty formatting lines
    # are insignificant in these generated, single-line-value HJSON files.
    return [line for line in text.splitlines() if line.strip()]


def main() -> int:
    outputs = {}
    generator.write = lambda path, text: outputs.update({path: text})
    generator.generate_localization()
    expected = {
        "generated/zh-Hans.hjson", "generated/en-US.hjson",
        "generated_bestiary/zh-Hans.hjson", "generated_bestiary/en-US.hjson",
    }
    if {path.relative_to(generator.ROOT / "Localization").as_posix() for path in outputs} != expected:
        print("Unexpected localization output set; update the verifier's contract.")
        return 1
    stale = [path for path, text in outputs.items()
             if not path.exists() or normalized_lines(path.read_text(encoding="utf-8")) != normalized_lines(text)]
    if stale:
        print("Generated localization content is stale:")
        for path in stale:
            print(path.relative_to(generator.ROOT))
        return 1
    print("Generated localization verified fresh (blank-line formatting ignored; no files modified).")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
