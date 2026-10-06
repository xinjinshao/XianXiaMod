"""Audit actual engine registrations exported by verify_build.ps1 -ExportEconomyAudit.

All registered shop/recipe conditions are assumed unlocked. This is a potential
coin conversion audit, not proof of progression, harvesting or in-game economics.
"""
from __future__ import annotations
import argparse
from fractions import Fraction
import json
from pathlib import Path


def analyze(snapshot: dict) -> dict:
    if snapshot.get("schema") != 1:
        raise ValueError("Unsupported economy snapshot schema")
    items = {item["id"]: item for item in snapshot["items"]}
    if len(items) != len(snapshot["items"]) or not items:
        raise ValueError("Missing or duplicate registered item IDs")
    recipes = snapshot["recipes"]
    for recipe in recipes:
        if recipe["output"] not in items or recipe["quantity"] <= 0 or not recipe["ingredients"]:
            raise ValueError("Invalid or ingredient-free recipe; needs explicit review")
        if any(i["id"] not in items or i["quantity"] <= 0 for i in recipe["ingredients"]):
            raise ValueError("Invalid recipe ingredient")
        if any(i not in items for group in recipe["groups"] for i in group["items"]):
            raise ValueError("Unknown recipe-group item")
    mod_items = {key: item for key, item in items.items() if item["name"].startswith("XianXia/")}
    if not mod_items or not snapshot.get("prices"):
        raise ValueError("Missing mod items or native price scenarios")
    mod_recipes = [r for r in recipes if r["output"] in mod_items]
    mod_shops = [s for s in snapshot["shops"] if s["name"].startswith("XianXia/")]
    if {s["name"] for s in snapshot["prices"]} != {"neutral", "happy", "happy_discount"} or len(snapshot["prices"]) != 3:
        raise ValueError("Missing or duplicate native price scenarios")
    scenarios = []
    for scenario in snapshot["prices"]:
        prices = {item["id"]: item for item in scenario["items"]}
        if prices.keys() != items.keys() or len(prices) != len(scenario["items"]):
            raise ValueError("Incomplete native price scenario")
        costs: dict[int, Fraction] = {}
        routes: dict[int, str] = {}
        excluded = 0
        for shop in snapshot["shops"]:
            for entry in shop["entries"]:
                if entry["id"] not in items:
                    raise ValueError("Unknown shop item")
                # Special currency and customized prices are not copper-value paths.
                # Callbacks may further restrict stock; never treat them as a source
                # with a guessed lower price.
                if entry["currency"] != -1 or entry["price"] != items[entry["id"]]["value"]:
                    excluded += 1
                    continue
                price = prices[entry["id"]]["buying"]
                if price <= 0:
                    excluded += 1
                    continue
                if entry["id"] not in costs or price < costs[entry["id"]]:
                    costs[entry["id"]] = Fraction(price)
                    routes[entry["id"]] = shop["name"]
        # Exact fractions preserve output batches. Unseeded cycles cannot acquire a
        # finite cost. A seeded cost-reducing cycle is reported, never silently cut.
        stable = False
        for _ in range(len(items) + 1):
            changed = False
            for recipe in recipes:
                total = Fraction(0)
                for ingredient in recipe["ingredients"]:
                    alternatives = {ingredient["id"]}
                    for group in recipe["groups"]:
                        if ingredient["id"] in group["items"]:
                            alternatives.update(group["items"])
                    known = [costs[key] for key in alternatives if key in costs]
                    if not known:
                        break
                    total += min(known) * ingredient["quantity"]
                else:
                    unit = total / recipe["quantity"]
                    output = recipe["output"]
                    if output not in costs or unit < costs[output]:
                        costs[output] = unit
                        routes[output] = f"recipe {recipe['id']}"
                        changed = True
            if not changed:
                stable = True
                break
        if not stable:
            raise ValueError("Shop-seeded cost-reducing recipe cycle; manual review required")
        candidates = []
        for key, item in mod_items.items():
            if key in costs and prices[key]["selling"] > costs[key]:
                candidates.append({"name": item["name"], "cost": costs[key],
                    "sell": prices[key]["selling"], "route": routes[key]})
        scenarios.append({"name": scenario["name"], "costs": costs, "prices": prices,
            "candidates": candidates, "excluded": excluded})
    usage = {}
    for key, item in mod_items.items():
        consumers = {items[r["output"]]["name"] for r in recipes if any(
            i["id"] == key or any(key in g["items"] and i["id"] in g["items"] for g in r["groups"])
            for i in r["ingredients"])}
        producers = [r for r in recipes if r["output"] == key]
        stocked = sorted({s["name"] for s in mod_shops if any(e["id"] == key for e in s["entries"])})
        usage[key] = {"consumers": sorted(consumers), "recipes": producers, "shops": stocked}
    return {"items": items, "mod_items": mod_items, "mod_recipes": mod_recipes,
        "mod_shops": mod_shops, "scenarios": scenarios, "usage": usage}


def report(snapshot: dict, result: dict) -> str:
    lines = ["# 制作与金币经济注册审计", "", f"引擎：{snapshot['engineVersion']}；模组：{snapshot['modVersion']}。",
        "", f"原生注册：{len(result['mod_items'])}件本模组物品、{len(result['mod_recipes'])}条产物配方、{len(result['mod_shops'])}个商店。",
        "", "数据来自隔离专服实际 PostSetupRecipes 和 GetItemExpectedPrice；没有打开商店或改动玩家库存。",
        "所有条件视为已解锁，制作站只作前置记录；价格为无前缀物品。成本由可购材料及多步配方传播，包含批量产出和配方组最低成本选择。",
        "不模拟 ModifyActiveShop/开店回调、卖回原购品的退款、随机制作奖励/前缀、掉落/种植/钓鱼/微光、旅途复制和其它模组价格钩子。特殊货币及自定义价格条目排除。没有可购路径不等于无法获得，零候选不等于完整经济验收。",
        "", "## 金币购买—制作—卖回候选", "", "| 原生价格场景 | 可购或可由购料制作的本模组物品 | 正收益候选 | 排除商店条目 |", "| --- | --- | --- | --- |"]
    for scenario in result["scenarios"]:
        reachable = sum(key in scenario["costs"] for key in result["mod_items"])
        lines.append(f"| {scenario['name']} | {reachable} | {len(scenario['candidates'])} | {scenario['excluded']} |")
        for candidate in scenario["candidates"]:
            lines.append(f"\n候选 {candidate['name']}：最低单位材料成本{float(candidate['cost']):.2f}铜，单位卖价{candidate['sell']}铜，路径{candidate['route']}。")
    lines += ["", "## 指南物品价格复核", "", "| 物品 | 场景 | 配方材料成本（铜） | 卖价（铜） |", "| --- | --- | --- | --- |"]
    for key, item in result["mod_items"].items():
        if item["name"] not in ("XianXia/SectLedger", "XianXia/TribulationGauge"):
            continue
        recipe = result["usage"][key]["recipes"][0]
        for scenario in result["scenarios"]:
            cost = sum(scenario["costs"][i["id"]] * i["quantity"] for i in recipe["ingredients"])
            lines.append(f"| {item['name']} | {scenario['name']} | {float(cost):.2f} | {scenario['prices'][key]['selling']} |")
    lines += ["", "## 原生标为材料的用途", "", "下表仅记录配方用途；突破/事务消耗另有运行逻辑。", "", "| 材料 | 产物用途 | 注册商店 |", "| --- | --- | --- |"]
    for key, item in sorted(result["mod_items"].items(), key=lambda pair: pair[1]["name"]):
        if item["material"]:
            usage = result["usage"][key]
            lines.append(f"| {item['name']} | {', '.join(usage['consumers']) or '无（须人工核对）'} | {', '.join(usage['shops']) or '无；另查掉落/制作来源'} |")
    lines += ["", "## 实际配方及制作站", "", "| 产物 | 数量 | 材料 | 制作站ID | 条件键 |", "| --- | --- | --- | --- | --- |"]
    for recipe in sorted(result["mod_recipes"], key=lambda r: (result["items"][r["output"]]["name"], r["id"])):
        ingredients = ', '.join(f"{result['items'][i['id']]['name']} ×{i['quantity']}" for i in recipe['ingredients'])
        lines.append(f"| {result['items'][recipe['output']]['name']} | {recipe['quantity']} | {ingredients} | {', '.join(map(str, recipe['stations'])) or '无'} | {', '.join(recipe['conditions']) or '无'} |")
    return '\n'.join(lines) + '\n'


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("snapshot", type=Path)
    parser.add_argument("--report", type=Path)
    parser.add_argument("--check", action="store_true", help="Fail on potential positive conversion paths")
    args = parser.parse_args()
    snapshot = json.loads(args.snapshot.read_text(encoding="utf-8-sig"))
    result = analyze(snapshot)
    if args.report:
        args.report.write_text(report(snapshot, result), encoding="utf-8")
    for scenario in result["scenarios"]:
        print(f"{scenario['name']}: {len(scenario['candidates'])} potential positive coin conversions")
        for candidate in scenario["candidates"]:
            print(f"  {candidate['name']}: cost {float(candidate['cost']):.2f}, sell {candidate['sell']}, {candidate['route']}")
    return int(args.check and any(s['candidates'] for s in result['scenarios']))


if __name__ == "__main__":
    raise SystemExit(main())
