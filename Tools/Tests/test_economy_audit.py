"""Behavior checks for registered-price graph analysis, not gameplay mocks."""
import copy
import importlib.util
from pathlib import Path
import unittest

spec = importlib.util.spec_from_file_location("audit_economy", Path(__file__).resolve().parents[1] / "audit_economy.py")
audit = importlib.util.module_from_spec(spec)
spec.loader.exec_module(audit)


def fixture():
    items = [{"id": i, "name": name, "value": value, "material": i == 1}
        for i, name, value in [(1, "XianXia/Material", 100), (2, "XianXia/Product", 1000), (3, "Terraria/Substitute", 10)]]
    return {"schema": 1, "items": items,
        "recipes": [{"id": 1, "output": 2, "quantity": 1,
            "ingredients": [{"id": 1, "quantity": 1}], "groups": [], "stations": [], "conditions": []}],
        "shops": [{"name": "XianXia/Vendor/Shop", "entries": [{"id": 1, "price": 100, "currency": -1}]}],
        "prices": [{"name": name, "items": [{"id": i, "buying": value, "selling": value // 5}
            for i, value in [(1, 100), (2, 1000), (3, 10)]]} for name in ("neutral", "happy", "happy_discount")]}


class EconomyAuditTests(unittest.TestCase):
    def test_detects_purchase_craft_sell_profit(self):
        result = audit.analyze(fixture())
        for scenario in result["scenarios"]:
            self.assertEqual(scenario["candidates"][0]["name"], "XianXia/Product")
            self.assertEqual(scenario["candidates"][0]["cost"], 100)

    def test_repricing_eliminates_profit(self):
        data = fixture()
        for scenario in data["prices"]:
            scenario["items"][1]["selling"] = 99
        self.assertTrue(all(not s["candidates"] for s in audit.analyze(data)["scenarios"]))

    def test_batch_output_and_multistep_paths(self):
        data = fixture()
        data["recipes"][0]["quantity"] = 2
        data["recipes"].append({"id": 2, "output": 3, "quantity": 1,
            "ingredients": [{"id": 2, "quantity": 3}], "groups": []})
        scenario = audit.analyze(data)["scenarios"][0]
        self.assertEqual(scenario["costs"][2], 50)
        self.assertEqual(scenario["costs"][3], 150)

    def test_group_uses_cheapest_registered_substitute(self):
        data = fixture()
        data["shops"][0]["entries"].append({"id": 3, "price": 10, "currency": -1})
        data["recipes"][0]["groups"] = [{"id": 1, "items": [1, 3]}]
        self.assertEqual(audit.analyze(data)["scenarios"][0]["costs"][2], 10)

    def test_special_currency_and_custom_prices_are_not_guessed(self):
        for modify in (lambda e: e.update(currency=0), lambda e: e.update(price=5)):
            data = fixture()
            modify(data["shops"][0]["entries"][0])
            result = audit.analyze(data)["scenarios"][0]
            self.assertFalse(result["candidates"])
            self.assertEqual(result["excluded"], 1)

    def test_unseeded_cycle_is_not_free(self):
        data = fixture()
        data["shops"][0]["entries"] = []
        data["recipes"].append({"id": 2, "output": 1, "quantity": 1,
            "ingredients": [{"id": 2, "quantity": 1}], "groups": []})
        self.assertEqual(audit.analyze(data)["scenarios"][0]["costs"], {})

    def test_seeded_cost_reducing_cycle_requires_review(self):
        data = fixture()
        data["recipes"].append({"id": 2, "output": 1, "quantity": 2,
            "ingredients": [{"id": 2, "quantity": 1}], "groups": []})
        with self.assertRaisesRegex(ValueError, "cost-reducing"):
            audit.analyze(data)

    def test_conditioned_recipes_remain_potential_candidates(self):
        data = fixture()
        data["recipes"][0]["conditions"] = ["LateProgression"]
        self.assertEqual(len(audit.analyze(data)["scenarios"][0]["candidates"]), 1)

    def test_incomplete_or_ambiguous_snapshot_cannot_pass(self):
        variants = []
        for mutate in (lambda d: d.update(schema=99), lambda d: d["items"].append(copy.deepcopy(d["items"][0])),
            lambda d: d["prices"].pop(), lambda d: d["prices"][0]["items"].pop(),
            lambda d: d["prices"][0]["items"].append(copy.deepcopy(d["prices"][0]["items"][0])),
            lambda d: d["recipes"][0].update(quantity=0), lambda d: d["recipes"][0].update(ingredients=[]),
            lambda d: d["recipes"][0]["ingredients"][0].update(id=999)):
            data = fixture()
            mutate(data)
            variants.append(data)
        for data in variants:
            with self.subTest(data=data), self.assertRaises(ValueError):
                audit.analyze(data)

    def test_material_usage_and_shop_sources_are_recorded(self):
        usage = audit.analyze(fixture())["usage"][1]
        self.assertEqual(usage["consumers"], ["XianXia/Product"])
        self.assertEqual(usage["shops"], ["XianXia/Vendor/Shop"])


if __name__ == "__main__":
    unittest.main()
