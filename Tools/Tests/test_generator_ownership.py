"""Replay generation in isolation and verify hand-maintained files cannot be touched."""
import contextlib
import io
from pathlib import Path
import sys
import tempfile
import unittest
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import generate_tmod_content as generator
import verify_generated_localization as verifier


OWNED_OUTPUTS = frozenset({
    "generated/zh-Hans.hjson", "generated/en-US.hjson",
    "generated_bestiary/zh-Hans.hjson", "generated_bestiary/en-US.hjson",
})


class GeneratorOwnershipTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name) / "workspace"
        self.root.mkdir()
        self.root_patch = patch.object(generator, "ROOT", self.root)
        self.root_patch.start()
        self.manifest_patch = patch.object(generator, "manifest_rows", return_value=[
            {"asset_id": "greenwood_root", "output_type": "item_icon"}])
        self.manifest_patch.start()

    def tearDown(self):
        self.manifest_patch.stop()
        self.root_patch.stop()
        self.temp.cleanup()

    def sentinel(self, relative):
        path = self.root / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(b"hand-maintained sentinel")
        return path

    def test_real_entrypoint_only_writes_four_owned_outputs(self):
        sentinels = [self.sentinel(name) for name in (
            "Content/NPCs/Bosses/GardenWarden.Battle.cs", "Content/Items/Stations/GeneratedStations.cs",
            "Common/Items/RefinedArtifact.cs", "Content/NPCs/Bosses/GardenWarden.png",
            "Localization/garden-battle/zh-Hans.hjson", "Localization/zh-Hans.hjson")]
        before = {p.relative_to(self.root).as_posix() for p in self.root.rglob("*") if p.is_file()}
        with contextlib.redirect_stdout(io.StringIO()):
            generator.main()
        after = {p.relative_to(self.root).as_posix() for p in self.root.rglob("*") if p.is_file()}
        self.assertEqual(generator.GENERATED_LOCALIZATION_OUTPUTS, OWNED_OUTPUTS)
        self.assertEqual(after - before, {"Localization/" + n for n in OWNED_OUTPUTS})
        self.assertTrue(all(p.read_bytes() == b"hand-maintained sentinel" for p in sentinels))
        for name in generator.GENERATED_LOCALIZATION_OUTPUTS:
            raw = (self.root / "Localization" / name).read_bytes()
            self.assertNotIn(b"\r", raw)
            self.assertIn(b"Mods", raw)

    def test_all_retired_gameplay_entrypoints_fail_before_mutations(self):
        sentinel = self.sentinel("Content/Items/GeneratedItems.cs")
        with patch.object(generator, "write", side_effect=AssertionError("write attempted")), \
             patch.object(generator, "copy_asset", side_effect=AssertionError("asset copy attempted")):
            for name in ("generate_materials", "generate_projectiles", "generate_tiles", "generate_enemies",
                         "generate_bosses", "generate_summons", "generate_biomes"):
                with self.subTest(name=name), self.assertRaisesRegex(RuntimeError, "retired"):
                    getattr(generator, name)(*(() if name == "generate_biomes" else (set(),)))
        self.assertEqual(sentinel.read_bytes(), b"hand-maintained sentinel")

    def test_generic_writer_rejects_sources_assets_manual_localization_and_escape(self):
        for name in ("Content/A.cs", "Content/A.png", "Common/A.cs", "Localization/zh-Hans.hjson",
                     "Localization/garden-battle/en-US.hjson", "Localization/generated/extra.hjson"):
            target = self.sentinel(name)
            with self.subTest(name=name), self.assertRaisesRegex(RuntimeError, "only write"):
                generator.write(target, "replacement")
            self.assertEqual(target.read_bytes(), b"hand-maintained sentinel")
        outside = self.root.parent / "outside.hjson"
        with self.assertRaises(RuntimeError):
            generator.write(outside, "replacement")
        self.assertFalse(outside.exists())
        with self.assertRaises(RuntimeError):
            generator.write(self.root / "Localization/generated/../../Content/new.cs", "replacement")
        self.assertFalse((self.root / "Content/new.cs").exists())

    def test_asset_copy_is_retired_even_with_a_real_source_asset(self):
        source = self.sentinel("Assets/Final/warden/warden__body__v01.png")
        target = self.sentinel("Content/Warden.png")
        with patch.object(generator, "FINAL", self.root / "Assets/Final"), self.assertRaisesRegex(RuntimeError, "retired"):
            generator.copy_asset("warden", "body", "Warden", target.parent)
        self.assertEqual(source.read_bytes(), b"hand-maintained sentinel")
        self.assertEqual(target.read_bytes(), b"hand-maintained sentinel")

    def test_owned_writer_is_utf8_lf_and_repeatable(self):
        path = self.root / "Localization/generated/zh-Hans.hjson"
        generator.write(path, "青木\r\n药园\n")
        self.assertEqual(path.read_bytes(), "青木\n药园\n".encode("utf-8"))
        generator.write(path, "青木\n药园\n")
        self.assertEqual(path.read_bytes(), "青木\n药园\n".encode("utf-8"))

    def test_readonly_freshness_verifier_restores_writer_and_reports_stale_values(self):
        generator.generate_localization()
        original = generator.write
        before = {p: p.read_bytes() for p in self.root.rglob("*.hjson")}
        with contextlib.redirect_stdout(io.StringIO()):
            self.assertEqual(verifier.main(), 0)
        self.assertIs(generator.write, original)
        self.assertEqual({p: p.read_bytes() for p in self.root.rglob("*.hjson")}, before)
        target = self.root / "Localization/generated/en-US.hjson"
        target.write_text("stale value", encoding="utf-8")
        with contextlib.redirect_stdout(io.StringIO()):
            self.assertEqual(verifier.main(), 1)
        self.assertEqual(target.read_text(encoding="utf-8"), "stale value")
        self.assertIs(generator.write, original)

    def test_unexpected_output_set_cannot_pass_readonly_verifier(self):
        original = generator.write
        def bad_generation():
            generator.write(self.root / "Content/A.cs", "bad")
        with patch.object(generator, "generate_localization", bad_generation), contextlib.redirect_stdout(io.StringIO()):
            self.assertEqual(verifier.main(), 1)
        self.assertIs(generator.write, original)
        self.assertFalse((self.root / "Content/A.cs").exists())

    def test_generation_failure_restores_writer(self):
        original = generator.write
        with patch.object(generator, "generate_localization", side_effect=RuntimeError("bad input")):
            with self.assertRaisesRegex(RuntimeError, "bad input"):
                verifier.main()
        self.assertIs(generator.write, original)


if __name__ == "__main__":
    unittest.main()
