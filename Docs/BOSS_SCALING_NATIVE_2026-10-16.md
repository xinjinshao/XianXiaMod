# Boss 原生难度与人数数值记录

日期：2026-10-16，第217轮。官方 tModLoader v2026.08.3.0，隔离普通小世界，无其它模组；实际注册 NPC.SetDefaults，NPCSpawnParams.playerCountForMultiplayerDifficultyOverride 指定人数，依次设置 Main.GameMode=0/1/2。每格生命按1/2/4人列出，伤害为接触基础伤害。

| Boss | 模式 | 生命（1 / 2 / 4人） | 接触伤害 | 防御 |
| --- | --- | --- | --- | --- |
| AbyssalStarWomb | 普通 | 21000 / 21000 / 21000 | 54 | 30 |
| AbyssalStarWomb | 专家 | 42000 / 56700 / 110366 | 108 | 30 |
| AbyssalStarWomb | 大师 | 53550 / 72292 / 140717 | 162 | 30 |
| BlackFurnaceIronGolem | 普通 | 3200 / 3200 / 3200 | 34 | 18 |
| BlackFurnaceIronGolem | 专家 | 6400 / 8640 / 16817 | 68 | 18 |
| BlackFurnaceIronGolem | 大师 | 8160 / 11016 / 21442 | 102 | 18 |
| BrokenHeavenInspector | 普通 | 96000 / 96000 / 96000 | 92 | 42 |
| BrokenHeavenInspector | 专家 | 192000 / 259200 / 504533 | 184 | 42 |
| BrokenHeavenInspector | 大师 | 244800 / 330480 / 643280 | 276 | 42 |
| FormlessSwordSoul | 普通 | 48000 / 48000 / 48000 | 72 | 38 |
| FormlessSwordSoul | 专家 | 96000 / 129600 / 252266 | 144 | 38 |
| FormlessSwordSoul | 大师 | 122400 / 165240 / 321640 | 216 | 38 |
| GardenWarden | 普通 | 2800 / 2800 / 2800 | 28 | 10 |
| GardenWarden | 专家 | 5600 / 7560 / 14715 | 56 | 10 |
| GardenWarden | 大师 | 7140 / 9639 / 18762 | 84 | 10 |
| GreenwoodMedicineKingEcho | 普通 | 52000 / 52000 / 52000 | 66 | 34 |
| GreenwoodMedicineKingEcho | 专家 | 104000 / 140400 / 273288 | 132 | 34 |
| GreenwoodMedicineKingEcho | 大师 | 132600 / 179010 / 348443 | 198 | 34 |
| HeavenTabletGuardian | 普通 | 86000 / 86000 / 86000 | 82 | 48 |
| HeavenTabletGuardian | 专家 | 172000 / 232200 / 451977 | 164 | 48 |
| HeavenTabletGuardian | 大师 | 219300 / 296055 / 576271 | 246 | 48 |
| MoonboneImmortal | 普通 | 420000 / 420000 / 420000 | 180 | 80 |
| MoonboneImmortal | 专家 | 840000 / 1134000 / 2207333 | 360 | 80 |
| MoonboneImmortal | 大师 | 1071000 / 1445850 / 2814350 | 540 | 80 |
| OldHeavenDaoCore | 普通 | 650000 / 650000 / 650000 | 220 | 100 |
| OldHeavenDaoCore | 专家 | 1300000 / 1755000 / 3416111 | 440 | 100 |
| OldHeavenDaoCore | 大师 | 1657500 / 2237625 / 4355541 | 660 | 100 |
| SpiritVeinWyrm | 普通 | 1200 / 1200 / 1200 | 22 | 6 |
| SpiritVeinWyrm | 专家 | 2400 / 3240 / 6306 | 44 | 6 |
| SpiritVeinWyrm | 大师 | 3060 / 4131 / 8041 | 66 | 6 |
| ThunderMarshJiao | 普通 | 18000 / 18000 / 18000 | 58 | 26 |
| ThunderMarshJiao | 专家 | 36000 / 48600 / 94600 | 116 | 26 |
| ThunderMarshJiao | 大师 | 45900 / 61965 / 120615 | 174 | 26 |
| TribulationCloudAvatar | 普通 | 4200 / 4200 / 4200 | 30 | 12 |
| TribulationCloudAvatar | 专家 | 8400 / 11340 / 22073 | 60 | 12 |
| TribulationCloudAvatar | 大师 | 10710 / 14458 / 28143 | 90 | 12 |

108组均通过：普通生命对应统一基础值且不随人数改变；专家/大师生命逐人数增加；单人生命随难度增加；伤害仅受一次1/2/3难度倍率，不随人数改变。没有为了得到整数倍率而覆盖引擎的生命计算。

构建/打包/专服加载：run-2161832157d84a3486755c13b0c0aef5。注册审计15901项（本轮新增433），报告summons-243a65d74c454e10802295e4a4675417/save/XianXia/summon-audit.json，同目录boss-scaling-audit.json为原始机器记录。134项缩放规则与12Boss钩子契约通过。

证据边界：人数通过原生参数覆盖，不是真实多客户端进入；不包含完整玩家/世界更新、战斗时长、弹幕实际命中伤害、特殊种子、其它模组的GlobalNPC或平衡评价。R03仍需1/2/4人战斗时长与实战记录，保持未完成。
