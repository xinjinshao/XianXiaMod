import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import verify_localization_keys as verifier


class LocalizationScopeTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name)
        self.previous = verifier.ROOT
        verifier.ROOT = self.root

    def tearDown(self):
        verifier.ROOT = self.previous
        self.temp.cleanup()

    def write(self, name, content):
        path = self.root / name
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(content, encoding="utf-8")

    def test_only_runtime_sources_are_scanned(self):
        for name in ("XianXia.cs", "Common/Deep/A.cs", "Content/Items/B.cs"):
            self.write(name, 'Language.GetTextValue("Mods.XianXia.Runtime");')
        for folder in ("Tools/Tests", "bin", "obj", ".tml-test/run/save", ".tml-test-worldgen"):
            self.write(folder + "/Ignored.cs", 'Language.GetTextValue("Mods.XianXia.Fixture");')
        self.assertEqual(verifier.localization_keys_from_code(), {"Runtime"})
        self.assertEqual(len(verifier.read_all("*.cs")), 3)

    def test_copied_keys_cannot_hide_missing_runtime_localization(self):
        self.write("Content/A.cs", 'Language.GetTextValue("Mods.XianXia.Missing");')
        text = "Mods: {\n XianXia: {\n Missing: copied\n }\n}\n"
        for folder in ("Tools/Tests", "bin", ".tml-test/run/save"):
            self.write(folder + "/en-US.hjson", text)
        self.assertEqual(verifier.localization_keys(), set())
        self.assertEqual(verifier.localization_keys_from_code() - verifier.localization_keys(), {"Missing"})

    def test_nested_packaged_localization_satisfies_key(self):
        self.write("Common/A.cs", 'Language.GetTextValue("Mods.XianXia.Feature.Message");')
        self.write("Localization/feature/en-US.hjson", 'Mods: {\n XianXia: {\n Feature: {\n Message: actual\n }\n }\n}\n')
        self.assertEqual(verifier.localization_keys_from_code() - verifier.localization_keys(), set())

    def test_artifacts_are_not_read_and_unknown_patterns_rejected(self):
        path = self.root / ".tml-test" / "Bad.cs"
        path.parent.mkdir(parents=True)
        path.write_bytes(b"\xff")
        self.assertEqual(verifier.read_all("*.cs"), [])
        with self.assertRaises(ValueError):
            verifier.read_all("*")


if __name__ == "__main__":
    unittest.main()
