import contextlib
import io
import sys
import tempfile
import unittest
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import verify_png_assets as verifier

class NativeTextureTests(unittest.TestCase):
    def check(self, override):
        with tempfile.TemporaryDirectory() as temp:
            root=Path(temp); folder=root/"Content/Items";folder.mkdir(parents=True)
            (folder/"Arrow.cs").write_text('public class Arrow : ModItem { '+override+' }',encoding="utf-8")
            old=verifier.ROOT;verifier.ROOT=root
            try:
                with contextlib.redirect_stdout(io.StringIO()):return verifier.main()
            finally:verifier.ROOT=old
    def test_native_item_texture(self):
        self.assertEqual(self.check('public override string Texture => $"Terraria/Images/Item_{ItemID.FlamingArrow}";'),0)
    def test_native_projectile_texture(self):
        self.assertEqual(self.check('public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.FireArrow}";'),0)
    def test_missing_custom_texture_still_fails(self):
        self.assertEqual(self.check('public override string Texture => "XianXia/Content/Items/Missing";'),1)
    def test_unrecognized_override_is_not_skipped(self):
        self.assertEqual(self.check('public override string Texture => "Missing/Texture";'),1)

if __name__ == "__main__":unittest.main()
