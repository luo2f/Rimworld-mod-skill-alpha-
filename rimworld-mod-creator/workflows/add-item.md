# 添加物品

> **适用版本**: RimWorld 1.6 | **最后更新**: 2026-07-28

## 概述

本工作流用于在已有 Mod 中添加物品，包括近战武器、远程武器、服装、材料等。所有物品类型均有对应模板，可直接使用，无需查原版。

**适用场景**：
- Mod 已创建（含基本目录结构）
- 需要新增武器、服装、材料等物品
- 需要为物品添加制作配方并挂载到工作台

> 如果还没有创建 Mod 项目，请先按 `workflows/new-mod.md` 创建。

---

## 步骤 1：确定物品类型

根据需求选择对应的模板和贴图目录：

| 物品类型 | 模板文件 | 贴图目录 | 说明 |
|----------|---------|---------|------|
| 近战武器 | `templates/weapon-melee.xml` | `Textures/Things/Item/` | 剑、刀、锤等 |
| 远程武器 | `templates/weapon-ranged.xml` | `Textures/Things/Item/` | 枪械、弓等 |
| 服装/护甲/头饰 | `templates/apparel.xml` | `Textures/Things/Pawn/Humanlike/Apparel/` | 衣服、头盔等 |
| 材料/建筑材料 | `templates/resource-stuff.xml` | `Textures/Things/Item/Resources/` | 可堆叠的原材料 |
| 制作配方 | `templates/recipe.xml` | （配方无独立贴图） | 配合物品使用 |

确认物品类型后，记录以下关键信息：
- defName（需加用户选择的前缀）
- 显示名称（label）
- 描述（description）
- 核心数值参数

---

## 步骤 2：加载对应模板

读取对应模板文件，模板已验证原版结构，包含完整的字段和注释。

模板中通常包含以下占位符（以 `<Your...>` 格式标注）：
- `<YourPrefix_WeaponName>` → 替换为带前缀的 defName
- `<YourWeaponLabel>` → 替换为中文显示名
- `<YourWeaponDescription>` → 替换为中文描述
- 数值占位符 → 替换为实际数值

> 模板中的 ParentName 继承链、Comps 配置均已在编写时对照原版验证，直接替换占位符即可。

---

## 步骤 3：替换占位符

逐一替换模板中的占位符：

### 必填字段

| 占位符 | 替换为 | 注意事项 |
|--------|-------|---------|
| defName | `前缀_名称`（如 `XX_Longsword`） | 必须加前缀，确保唯一 |
| label | 中文显示名 | 游戏内显示名称 |
| description | 中文描述 | 鼠标悬停时显示 |
| texPath | 贴图路径 | 指向 Textures/ 下的路径（不含 Textures/ 前缀） |

### 数值参数（根据物品类型选择）

**近战武器**（weapon-melee.xml）：
- `MeleeWeapon_DamageAmount`：近战伤害
- `MeleeWeapon_Cooldown`：攻击冷却（秒）
- `Mass`：重量（kg）
- verbs 中的 meleeDamageDef：伤害类型

**远程武器**（weapon-ranged.xml）：
- ranged verbs 中的 warmupTime：瞄准时间（秒）
- range：射程
- burstShotCount：连发数
- ticksBetweenBurstShots：连发间隔
- projectile：弹药 defName
- `RangedWeapon_Cooldown`：射击冷却

**服装**（apparel.xml）：
- `ArmorRating_Blunt` / `ArmorRating_Sharp` / `ArmorRating_Heat`：护甲值
- `Insulation_Cold` / `Insulation_Heat`：隔热值
- `Mass`：重量
- bodyPartGroups：覆盖的身体部位
- layers：穿戴层级

**材料**（resource-stuff.xml）：
- `StuffPower_Armor_Blunt` / `StuffPower_Armor_Sharp`：材料护甲系数
- `StuffPower_Insulation_Cold` / `StuffPower_Insulation_Heat`：隔热系数
- `SharpDamageMultiplier` / `BluntDamageMultiplier`：伤害系数
- stackLimit：堆叠上限
- `Mass`：单件重量

> 数值参考范围：建议查看原版同类物品的数值进行平衡调整。

---

## 步骤 4：创建贴图

为物品创建 PNG 贴图文件：

| 物品类型 | 建议尺寸 | 贴图路径 |
|----------|---------|---------|
| 近战武器 | 128x128 或 256x256 | `Textures/Things/Item/<defName>.png` |
| 远程武器 | 128x128 或 256x256 | `Textures/Things/Item/<defName>.png` |
| 服装 | 128x128 或 256x256 | `Textures/Things/Pawn/Humanlike/Apparel/<defName>.png` |
| 材料 | 128x128 或 256x256 | `Textures/Things/Item/Resources/<defName>.png` |

贴图要求：
- 格式：PNG
- 背景：透明
- 主体居中，留有适当边距

确保 texPath 与实际文件路径一致：

```xml
<!-- texPath 不含 Textures/ 前缀 -->
<texPath>Things/Item/<defName></texPath>
```

**测试阶段**：可暂时引用原版贴图路径，后续替换为原创贴图。

> 贴图制作详细说明见 `references/07-assets.md`。

---

## 步骤 5：添加制作配方（如需要）

如果物品需要通过制作获得，使用 `templates/recipe.xml` 创建配方：

### 必填字段

| 字段 | 说明 |
|------|------|
| defName | `前缀_Make_物品名`（如 `XX_Make_Longsword`） |
| label | 制作名称（如"制作长剑"） |
| description | 制作描述 |
| jobString | 工作描述 |
| workAmount | 工作量 |
| ingredients | 所需材料及数量 |
| products | 产出物品及数量 |
| recipeUsers | 关联的工作台（见步骤6）或用 Patch 添加 |

### 配方示例结构

```xml
<RecipeDef>
  <defName>前缀_Make_物品名</defName>
  <label>制作物品名</label>
  <description>制作物品名的描述</description>
  <jobString>正在制作物品名。</jobString>
  <workAmount><!-- 工作量数值 --></workAmount>
  <workSpeedStat>SmithingSpeed</workSpeedStat>
  <soundWorking>Recipe_Smith</soundWorking>
  <ingredients>
    <li>
      <filter>
        <thingDefs>
          <li>SteelBar</li>
        </thingDefs>
      </filter>
      <count><!-- 材料数量 --></count>
    </li>
  </ingredients>
  <fixedIngredientFilter>
    <thingDefs>
      <li>SteelBar</li>
    </thingDefs>
  </fixedIngredientFilter>
  <products>
    <前缀_物品defName>1</前缀_物品defName>
  </products>
</RecipeDef>
```

> 配方中的材料 defName 需使用原版或已存在的材料 defName。

---

## 步骤 6：添加到工作台

配方创建后，需要关联到工作台才能在游戏中制作。有两种方式：

### 方式 A：recipeUsers 字段（简单直接）

在配方 Def 中直接指定工作台：

```xml
<recipeUsers>
  <li>ElectricSmithy</li>       <!-- 电动锻造台 -->
  <li>FueledSmithy</li>        <!-- 燃料锻造台 -->
</recipeUsers>
```

常用工作台 defName：
- `ElectricSmithy` / `FueledSmithy`：锻造台
- `TableMachining`：机械加工台
- `TableButcher`：屠宰台
- `ElectricTailoringBench` / `HandTailoringBench`：裁缝台
- `ElectricStove` / `FueledStove`：炉灶
- `CraftingSpot`：制作点

> 完整工作台列表可通过 grep 搜索原版 `Building_Production.xml`。

### 方式 B：PatchOperationAdd（更灵活）

使用 Patch 给原版工作台添加配方，避免直接修改原版文件：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Patch>
  <Operation Class="PatchOperationAdd">
    <xpath>/Defs/ThingDef[defName="ElectricSmithy"]/recipes</xpath>
    <value>
      <li>前缀_Make_物品名</li>
    </value>
  </Operation>
</Patch>
```

> 如果工作台没有 `<recipes>` 节点，需要先使用 `PatchOperationInsert` 创建该节点。详见 `workflows/patch-vanilla.md`。

---

## 步骤 7：测试

生成完成后，进行游戏内测试：

1. 启动 RimWorld，确保你的 Mod 已勾选
2. 开启 **DevMode**
3. 使用 **Debug 动作菜单** → **Spawn Thing** → 搜索物品 defName
4. 生成物品后验证：
   - 物品属性是否正确（鼠标悬停查看）
   - 贴图是否正常显示
   - 是否有红字报错
5. 如果有制作配方：
   - 检查工作台是否能制作该物品
   - 制作流程是否正常完成
   - 产出是否正确

---

## 常见问题

### 物品无法在 Spawn 菜单中找到

- **原因**：defName 拼写错误或 ParentName 不存在
- **解决**：检查 defName 拼写，确认 ParentName 在原版或本 Mod 中已定义

### 贴图显示为紫色方块

- **原因**：texPath 路径错误或文件缺失
- **解决**：
  - 检查 texPath 是否正确（不含 `Textures/` 前缀）
  - 检查文件是否存在且文件名大小写一致
  - 检查 PNG 文件是否为有效格式

### 物品属性显示异常

- **原因**：statBases 中的字段名拼写错误或数值格式不对
- **解决**：对照模板或原版同类物品，检查字段名和数值

### 配方在工作台中不显示

- **原因**：recipeUsers 未设置或 Patch 未生效
- **解决**：
  - 检查 recipeUsers 中的工作台 defName 是否正确
  - 如果用 Patch，检查 XPath 路径是否正确
  - 检查 Player.log 中是否有 Patch 相关错误

### 远程武器无法射击

- **原因**：projectile 字段引用的弹药 defName 不存在
- **解决**：确认 projectile 引用的弹药 defName 在原版或本 Mod 中已定义

### 服装无法穿戴

- **原因**：bodyPartGroups 或 layers 设置不正确
- **解决**：对照原版同类服装，确认 bodyPartGroups 和 layers 值正确

---

## 相关文档

- 新建 Mod：`workflows/new-mod.md`
- XML Def 系统：`references/03-xml-defs.md`
- 资源制作：`references/07-assets.md`
- 修改原版（Patch）：`workflows/patch-vanilla.md`
- 崩溃排查：`workflows/debug-crash.md`
