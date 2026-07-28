# RimWorld Mod 批量需求文件格式模板

> 本文件用于批量描述模组制作需求。每行一个需求，用 `## 需求 N: <名称>` 分割，AI 将逐条处理。
> 使用时请复制本模板，替换示例内容为实际需求。

---

## 格式说明

每个需求块以 `## 需求 N: <名称>` 开头，包含以下信息：

- **类型**: 近战武器 / 远程武器 / 服装护甲 / 建筑 / 原材料 / 配方 / 其他
- **defName**: 唯一标识符（建议加模组前缀）
- **标签**: 游戏内显示名称
- **相关属性**: 根据类型提供对应的关键属性
- **科技等级**: Neolithic / Medieval / Industrial / Spacer / Ultra / Archotech
- **材料**: 制作所需材料及数量

AI 会根据每个需求的"类型"字段，自动选择对应的模板文件生成 Def。

---

## 需求 1: 等离子剑

- **类型**: 近战武器
- **defName**: <YourModPrefix>_PlasmaSword
- **标签**: 等离子剑
- **描述**: 一把以等离子能量驱动的近战武器，刀刃灼热可切割大多数材料。
- **攻击方式**:
  - 刀刃（Cut）: 伤害 22，冷却 1.5s，穿甲 0.5
  - 刀柄（Blunt）: 伤害 10，冷却 1.8s，穿甲 0.15
- **科技等级**: Spacer
- **材料**: 精炼可塑性钢 x60，太空零部件 x4
- **研究前置**: <YourResearchDefName>
- **制作技能**: Crafting 8
- **制作工作量**: 20000
- **weaponTags**: SpacerMelee
- **贴图路径**: Things/Item/Equipment/WeaponMelee/<YourTexPath>

---

## 需求 2: 精密组装台

- **类型**: 建筑
- **defName**: <YourModPrefix>_PrecisionAssemblyBench
- **标签**: 精密组装台
- **描述**: 一台高精度的组装工作台，配备自动化机械臂，可高效制作精密部件。
- **尺寸**: 3x1
- **工作量**: 12000
- **科技等级**: Spacer
- **材料**: 钢 x200，太空零部件 x8，精炼可塑性钢 x50
- **功耗**: 500W
- **研究前置**: <YourResearchDefName>
- **建筑分类**: Production
- **耐久**: 300
- **可旋转**: 是
- **贴图路径**: Things/Building/Production/<YourTexPath>

---

## 需求 3: 战术头盔

- **类型**: 服装护甲
- **defName**: <YourModPrefix>_TacticalHelmet
- **标签**: 战术头盔
- **描述**: 采用复合装甲材料制作的战术头盔，提供优秀的弹道防护和隔热性能。
- **护甲值**:
  - 锐器（ArmorRating_Sharp）: 0.75
  - 钝器（ArmorRating_Blunt）: 0.45
  - 热能（ArmorRating_Heat）: 0.35
- **隔热**:
  - 寒冷（Insulation_Cold）: 8
  - 炎热（Insulation_Heat）: 4
- **科技等级**: Industrial
- **材料**: 钢 x80，工业零部件 x3（stuffCategories: Metallic）
- **研究前置**: <YourResearchDefName>
- **制作技能**: Crafting 7
- **制作工作量**: 15000
- **穿戴部位**: UpperHead
- **穿戴层级**: Overhead
- **耐久**: 150
- **贴图路径**: Things/Pawn/Humanlike/Apparel/<YourTexPath>/<YourTexPath>_north

---

## 需求 4: 高爆手雷

- **类型**: 远程武器
- **defName**: <YourModPrefix>_GrenadeHE
- **标签**: 高爆手雷
- **描述**: 威力巨大的高爆手雷，投掷后在目标区域造成大范围爆炸伤害。
- **伤害**: 50（爆炸范围 3 格）
- **射程**: 12 格
- **射速**: warmupTime 1.5s，单发，无连发
- **科技等级**: Industrial
- **材料**: 钢 x30，化学燃料 x10，工业零部件 x2
- **研究前置**: <YourResearchDefName>
- **制作技能**: Crafting 5
- **制作工作量**: 8000
- **弹丸**: <YourModPrefix>_Bullet_GrenadeHE
- **weaponTags**: Grenade
- **贴图路径**: Things/Item/Equipment/WeaponRanged/<YourTexPath>

---

<!-- 在此下方继续添加更多需求，格式同上 -->
