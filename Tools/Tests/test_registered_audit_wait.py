"""Audit runner failure handling: real child handles plus deterministic timeout cases."""
import importlib.util
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import Mock, patch

spec = importlib.util.spec_from_file_location("audit_runner", Path(__file__).parents[1] / "verify_registered_summons.py")
runner = importlib.util.module_from_spec(spec)
spec.loader.exec_module(runner)


class AuditWaitTests(unittest.TestCase):
    def setUp(self):
        self.directory = tempfile.TemporaryDirectory()
        self.addCleanup(self.directory.cleanup)
        self.root = Path(self.directory.name)
        self.report = self.root / "report.json"
        self.output = self.root / "server.log"
        self.valid = {"schema": 1, "passed": True, "checks": ["actual assertion"], "limitations": "fixture only"}

    def write(self, result):
        self.report.write_text(json.dumps(result), encoding="utf-8")

    def worker(self, text, report=None):
        # Readiness is established by the real subprocess writing its own output file.
        script = "import pathlib,sys,time; pathlib.Path(sys.argv[1]).write_text(sys.argv[2]); "
        if report is not None:
            script += "pathlib.Path(sys.argv[3]).write_text(sys.argv[4]); "
        script += "time.sleep(60)"
        process = subprocess.Popen([sys.executable, "-c", script, str(self.output), text, str(self.report), json.dumps(report)])
        def stop():
            if process.poll() is None:
                process.kill()
            process.wait(timeout=5)
        self.addCleanup(stop)
        return process

    def test_live_fatal_server_fails_before_timeout(self):
        process = self.worker("Loading world data: 100%\nLoad failed!  No backup found.\n")
        with self.assertRaisesRegex(RuntimeError, "Server failed"):
            runner.wait_for_report(process, self.report, self.output, 5)
        self.assertIsNone(process.poll())  # Caller still owns cleanup of this confirmed live handle.

    def test_live_success_returns_without_stopping_server(self):
        process = self.worker("World loaded", self.valid)
        self.assertEqual(runner.wait_for_report(process, self.report, self.output, 5), self.valid)
        self.assertIsNone(process.poll())

    def test_fatal_log_cannot_be_hidden_by_pass_report(self):
        self.write(self.valid); self.output.write_text("Main engine crash", encoding="utf-8")
        with self.assertRaisesRegex(RuntimeError, "Server failed"):
            runner.wait_for_report(Mock(poll=lambda: None), self.report, self.output, 5)

    def test_exit_with_partial_report_fails_without_waiting(self):
        self.report.write_text('{"schema":', encoding="utf-8")
        with patch.object(runner.time, "sleep") as sleep, self.assertRaisesRegex(RuntimeError, "exited with code 7"):
            runner.wait_for_report(Mock(poll=lambda: 7), self.report, self.output, 5)
        sleep.assert_not_called()

    def test_live_partial_report_can_finish(self):
        self.report.write_text('{"schema":', encoding="utf-8")
        with patch.object(runner.time, "sleep", side_effect=lambda _: self.write(self.valid)):
            self.assertEqual(runner.wait_for_report(Mock(poll=lambda: None), self.report, self.output, 5), self.valid)

    def test_main_kills_its_fatal_disposable_server(self):
        engine = self.root / "engine"; engine.mkdir()
        (engine / "tModLoader.dll").write_text("import time\nprint('Main engine crash', flush=True)\ntime.sleep(60)\n", encoding="utf-8")
        world = self.root / "source.wld"; world.write_bytes(b"original world")
        companion = world.with_suffix(".twld"); companion.write_bytes(b"original mod world")
        package = self.root / "mod.tmod"; package.write_bytes(b"test package")
        dependency = self.root / "optional.tmod"; dependency.write_bytes(b"optional package")
        real_popen = subprocess.Popen; children = []
        def launch(*args, **kwargs):
            process = real_popen(*args, **kwargs); children.append(process)
            return process
        args = ["verify", "--dotnet", sys.executable, "--tml", str(engine), "--package", str(package), "--world", str(world), "--timeout", "5"]
        args += ["--extra-mod", "Helper=" + str(dependency)]
        with patch.object(sys, "argv", args), patch.object(runner, "__file__", str(self.root / "Tools" / "verify.py")), patch.object(runner.subprocess, "Popen", side_effect=launch), patch("builtins.print"):
            with self.assertRaisesRegex(RuntimeError, "Server failed"):
                runner.main()
        self.assertEqual(len(children), 1)
        self.assertIsNotNone(children[0].poll())
        self.assertEqual(world.read_bytes(), b"original world")
        self.assertEqual(companion.read_bytes(), b"original mod world")
        mods = next((self.root / ".tml-test").glob("summons-*/save/Mods"))
        self.assertEqual(json.loads((mods / "enabled.json").read_text()), ["XianXia", "Helper"])
        self.assertEqual((mods / "Helper.tmod").read_bytes(), b"optional package")
        self.assertEqual(dependency.read_bytes(), b"optional package")

    def test_extra_mod_validation(self):
        package = self.root / "optional.tmod"; package.write_bytes(b"optional")
        self.assertEqual(runner.extra_mods(["CalamityMod=" + str(package)]), {"CalamityMod": package.resolve()})
        for name in ("../escape", "XianXia", "CON", "lpt1", "bad/name", ""):
            with self.subTest(name=name), self.assertRaises(ValueError):
                runner.extra_mods([name + "=" + str(package)])
        with self.assertRaises(ValueError):
            runner.extra_mods(["Helper=" + str(package), "helper=" + str(package)])
        with self.assertRaises(ValueError):
            runner.extra_mods([str(package)])
        wrong = self.root / "wrong.txt"; wrong.write_text("test")
        with self.assertRaises(ValueError):
            runner.extra_mods(["Helper=" + str(wrong)])

    def test_partial_utf8_report_can_finish(self):
        self.report.write_bytes(b'{"checks":["' + bytes([0xe4]))
        with patch.object(runner.time, "sleep", side_effect=lambda _: self.write(self.valid)):
            self.assertEqual(runner.wait_for_report(Mock(poll=lambda: None), self.report, self.output, 5), self.valid)

    def test_live_timeout_is_distinct_from_process_exit(self):
        with patch.object(runner.time, "monotonic", side_effect=[10, 15]), self.assertRaisesRegex(RuntimeError, "Timed out.*no report"):
            runner.wait_for_report(Mock(poll=lambda: None), self.report, self.output, 5)

    def test_invalid_evidence_is_not_a_pass(self):
        for change in ({"schema": True}, {"passed": 1}, {"checks": []}, {"checks": [3]}, {"limitations": ""}):
            with self.subTest(change=change), self.assertRaises(RuntimeError):
                runner.validate_report(self.valid | change, self.report)

    def test_log_read_is_bounded_and_ordinary_error_text_is_not_fatal(self):
        self.output.write_text("x" * 70000 + "recoverable error example", encoding="utf-8")
        self.assertLessEqual(len(runner.log_tail(self.output)), 65536)
        self.write(self.valid)
        self.assertEqual(runner.wait_for_report(Mock(poll=lambda: None), self.report, self.output, 5), self.valid)


if __name__ == "__main__":
    unittest.main()
