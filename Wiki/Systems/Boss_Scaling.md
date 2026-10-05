# Boss 难度与人数缩放

12 个 Boss 的普通模式基础生命和接触伤害集中在 Common/Systems/BossStatRules.cs；SetDefaults 不再检查专家/大师并额外叠加倍率。原生引擎先结算难度和特殊种子，随后 ApplyDifficultyAndPlayerScaling 仅以传入 balance × bossAdjustment 调整生命。伤害保留引擎结果，不随人数额外增加。蠕虫节段由现有 FollowPreviousSegment 继承头部数值，不自行再次应用头部倍率。

官方顺序：[ModNPC hook 文档](https://docs.tmodloader.net/docs/stable/class_mod_n_p_c.html)。这是首轮消除重复缩放与接入多人参数，不表示完整战斗平衡已完成。专家/大师值相对旧实现降低，影响接触伤害和引用 NPC.damage 的招式；这项行为变化需要按阶段重新验证。

| Boss 内部名 | 普通基础生命 | 普通基础接触伤害 |
| --- | ---: | ---: |
| SpiritVeinWyrm | 1200 | 22 |
| GardenWarden | 2800 | 28 |
| BlackFurnaceIronGolem | 3200 | 34 |
| TribulationCloudAvatar | 4200 | 30 |
| ThunderMarshJiao | 18000 | 58 |
| AbyssalStarWomb | 21000 | 54 |
| FormlessSwordSoul | 48000 | 72 |
| GreenwoodMedicineKingEcho | 52000 | 66 |
| HeavenTabletGuardian | 86000 | 82 |
| BrokenHeavenInspector | 96000 | 92 |
| MoonboneImmortal | 420000 | 180 |
| OldHeavenDaoCore | 650000 | 220 |

## 验证与剩余工作

BossScaling 回归134条断言覆盖12组基础值、已有难度结果不再额外乘算、人数参数、bossAdjustment、取整、非法参数和溢出。输入是模拟引擎提供的结果，不能作为真实普通/专家/大师 × 1/2/4 人矩阵的证据。源契约检查12个Boss已接入；编译、打包与专用服务器加载通过。

- [ ] 记录三个难度、1/2/4人实际出生时 lifeMax、damage、defDamage、人数参数。
- [ ] 特殊种子与其他Mod的GlobalNPC缩放兼容。
- [ ] 蠕虫首次节段同步和新玩家加入/退出后的原生行为。
- [ ] 同阶段四职业实际战斗时长、弹幕伤害与复活/脱战验收。

普通模式的多人行为沿用原生引擎，不额外强行加入专家生命倍率；实际引擎是否调用hook及给出的参数按上述矩阵记录。
