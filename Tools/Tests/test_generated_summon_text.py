"""Verify generated player-facing summon realms against manual cultures and real item rules."""
import json
from pathlib import Path
import re
import sys
import tempfile
import unittest
from unittest.mock import patch
sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import generate_tmod_content as generator

ROOT = generator.ROOT

class SummonTextTests(unittest.TestCase):
    def capture(self):
        output = {}
        with patch.object(generator, "write", side_effect=lambda path, text: output.update({path.name + "/" + path.parent.name: text})):
            generator.generate_localization()
        return output

    def test_all_eleven_summons_use_canonical_readable_realms(self):
        output = self.capture()
        self.assertEqual(len(generator.BOSS_STAGE_REQUIREMENTS), 11)
        for culture in ("zh-Hans", "en-US"):
            names = json.loads((ROOT / "Localization/cultivation-status" / f"{culture}.hjson").read_text(encoding="utf-8"))["Mods"]["XianXia"]["CultivationStatus"]["Realms"]
            text = output[f"{culture}.hjson/generated"]
            for stage in set(generator.BOSS_STAGE_REQUIREMENTS.values()):
                self.assertIn(names[stage], text)
            tooltips = re.findall(r'Tooltip: ("(?:\\.|[^"\\])*")', text)
            self.assertGreaterEqual(len(tooltips), 11)
            for value in tooltips:
                displayed = json.loads(value)
                for stage in generator.BOSS_STAGE_REQUIREMENTS.values():
                    if names[stage] != stage:
                        self.assertNotIn(stage, displayed)

    def test_generated_requirements_match_actual_summon_sources(self):
        files = list((ROOT / "Content/Items/BossSummons").glob("*.cs"))
        for boss_id, required in generator.BOSS_STAGE_REQUIREMENTS.items():
            npc_name = generator.pascal(boss_id)
            matching = [path for path in files if f"NPCs.Bosses.{npc_name}>" in path.read_text(encoding="utf-8")]
            self.assertEqual(len(matching), 1, boss_id)
            source = matching[0].read_text(encoding="utf-8")
            stages = re.findall(r'CultivationStage\.(\w+)', source)
            self.assertEqual(stages, [required], matching[0].name)

    def test_heaven_tablet_name_is_consistent_in_generated_labels(self):
        output = self.capture()
        self.assertNotIn("天碑守尽", output["zh-Hans.hjson/generated"])
        self.assertNotIn("天碑守尽", output["zh-Hans.hjson/generated_bestiary"])
        self.assertIn("天碑守御", output["zh-Hans.hjson/generated"])

    def test_missing_manual_culture_fails_before_any_write(self):
        with tempfile.TemporaryDirectory() as temp, patch.object(generator, "ROOT", Path(temp)), patch.object(generator, "write") as writer:
            with self.assertRaises(FileNotFoundError): generator.generate_localization()
            writer.assert_not_called()

    def test_unknown_requirement_fails_before_any_write(self):
        with patch.dict(generator.BOSS_STAGE_REQUIREMENTS, {"garden_warden": "UnknownRealm"}), patch.object(generator, "write") as writer:
            with self.assertRaisesRegex(ValueError, "Missing readable"): generator.generate_localization()
            writer.assert_not_called()

    def test_manual_name_change_is_read_without_mutating_inputs(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            before = {}
            for culture in ("zh-Hans", "en-US"):
                path = root / "Localization/cultivation-status" / f"{culture}.hjson"
                path.parent.mkdir(parents=True, exist_ok=True)
                data = json.loads((ROOT / "Localization/cultivation-status" / f"{culture}.hjson").read_text(encoding="utf-8"))
                data["Mods"]["XianXia"]["CultivationStatus"]["Realms"]["QiAwakening"] = "Updated readable realm " + culture
                path.write_text(json.dumps(data), encoding="utf-8")
                before[path] = path.read_bytes()
            with patch.object(generator, "ROOT", root), patch.object(generator, "manifest_rows", return_value=[]):
                output = self.capture()
            for culture in ("zh-Hans", "en-US"):
                self.assertIn("Updated readable realm " + culture, output[f"{culture}.hjson/generated"])
            self.assertEqual({path: path.read_bytes() for path in before}, before)

if __name__ == "__main__": unittest.main()
