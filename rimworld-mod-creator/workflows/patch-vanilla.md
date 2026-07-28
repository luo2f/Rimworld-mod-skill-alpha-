# 修改原版

> **适用版本**: RimWorld 1.6 | **最后更新**: 2026-07-28

## 概述

使用 PatchOperations 修改原版 Def，**不直接修改原版文件**。原版文件应保持不变，所有修改通过 Mod 中的 Patch 文件实现，这样修改可随 Mod 启用/禁用，且不会破坏原版完整性。

---

## 何时用 XML Patch vs Harmony

根据修改类型选择方案：

| 修改类型 | 使用方案 | 说明 |
|---------|---------|------|
| 修改数值（伤害、耐久、成本等） | XML Patch | 数据层修改 |
| 添加/删除字段 | XML Patch | 数据层修改 |
| 添加配方到工作台 | XML Patch | 数据层修改 |
| 修改贴图路径 | XML Patch | 数据层修改 |
| 修改游戏逻辑/行为 | Harmony | 需要拦截 C# 方法 |
| 修改 UI 显示逻辑 | Harmony | 需要修改渲染代码 |
| 修改物品生成逻辑 | Harmony | 需要修改生成方法 |

> XML Patch 参考文档：`references/04-xml-patching.md`
> Harmony 参考文档：`references/06-harmony.md`

---

## 步骤 1：确定修改目标

明确以下信息：
- 要修改哪个 Def（通过 defName 定位）
- 要修改哪个字段
- 修改为什么值
- 是添加、删除、还是替换

示例：
- 修改目标：原版武器 `Knife` 的伤害值
- 字段：`statBases` 下的 `MeleeWeapon_DamageAmount`
- 操作：替换为新的数值
- 修改目标：给原版工作台 `ElectricSmithy` 添加新配方
- 字段：`recipes` 节点
- 操作：添加新配方 defName

---

## 步骤 2：查原版 Def 结构

**铁律：编写 Patch 前必须先查原版对应文件，确认字段名、节点层级、XML 结构。**

用 grep 搜索原版 Defs 目录：

```bash
# 搜索目标 Def 定义
grep -r 'defName="Knife"' "<RimWorld安装路径>/Data/Core/Defs/"
# 搜索目标字段的所有用法
grep -r "MeleeWeapon_DamageAmount" "<RimWorld安装路径>/Data/Core/Defs/"
# 搜索 ParentName 定义
grep -r 'Name="BaseMeleeWeapon"' "<RimWorld安装路径>/Data/Core/Defs/"
```

查看目标 Def 的完整结构，确认：
- Def 的完整 XML 结构
- 字段所在的节点层级
- 是否有 ParentName 继承（继承的字段可能不在当前 Def 中直接定义）
- 该字段是否可能存在于多个 Def 中

---

## 步骤 3：选择 Patch 操作

根据修改需求选择操作类型：

| 操作类型 | 类名 | 用途 |
|---------|------|------|
| Add | `PatchOperationAdd` | 在指定节点下添加子节点 |
| Remove | `PatchOperationRemove` | 删除指定节点 |
| Replace | `PatchOperationReplace` | 替换指定节点的值 |
| Insert | `PatchOperationInsert` | 在指定位置插入节点 |
| AddModExtension | `PatchOperationAddModExtension` | 添加 ModExtension |
| Attribute | `PatchOperationAttribute` | 修改 XML 属性值 |

### 各操作示例

**PatchOperationAdd**（添加子节点）：

```xml
<Operation Class="PatchOperationAdd">
  <xpath>/Defs/ThingDef[defName="ElectricSmithy"]/recipes</xpath>
  <value>
    <li>你的配方defName</li>
  </value>
</Operation>
```

**PatchOperationRemove**（删除节点）：

```xml
<Operation Class="PatchOperationRemove">
  <xpath>/Defs/ThingDef[defName="目标defName"]/要删除的字段名</xpath>
</Operation>
```

**PatchOperationReplace**（替换节点值）：

```xml
<Operation Class="PatchOperationReplace">
  <xpath>/Defs/ThingDef[defName="Knife"]/statBases/MeleeWeapon_DamageAmount</xpath>
  <value>
    <MeleeWeapon_DamageAmount>15</MeleeWeapon_DamageAmount>
  </value>
</Operation>
```

**PatchOperationInsert**（在指定位置插入节点）：

```xml
<Operation Class="PatchOperationInsert">
  <xpath>/Defs/ThingDef[defName="目标defName"]/comps</xpath>
  <value>
    <li Class="CompProperties_Flickable"/>
  </value>
  <order>Append</order>
</Operation>
```

> order 可选值：`Append`（末尾追加）、`Prepend`（开头插入）。

---

## 步骤 4：编写 XPath

XPath 用于精确定位要修改的节点：

### 基本语法

```xpath
/Defs/ThingDef[defName="目标defName"]/字段路径
```

### 常用 XPath 模式

| 场景 | XPath 示例 | 说明 |
|------|-----------|------|
| 精确匹配 defName | `ThingDef[defName="Knife"]` | 只匹配指定 defName |
| 匹配多个 defName | `ThingDef[defName="Knife" or defName="Sword"]` | 匹配多个 |
| 匹配 ParentName | `ThingDef[ParentName="BaseMeleeWeapon"]` | 匹配继承自某基类的 |
| 匹配某标签 | `ThingDef[statBases/MeleeWeapon_DamageAmount]` | 匹配包含某字段的 |
| 匹配某属性 | `ThingDef[@Name="BaseMeleeWeapon"]` | 匹配有 Name 属性的 |
| 通配子节点 | `ThingDef[defName="Knife"]/*` | 匹配所有子节点 |
| 按类别匹配 | `ThingDef[@ParentName="BaseMeleeWeapon"]` | 匹配继承自某父类的 |

### XPath 注意事项

- 使用 `defName` 精确匹配，避免大范围匹配
- 注意继承的 Def：字段可能在 ParentName 中定义，不在当前 Def 中直接出现
- 如果目标节点不存在（如工作台没有 `recipes` 节点），Add 操作可能失败，需先检查

---

## 步骤 5：条件补丁

如果需要兼容 DLC 或其他 Mod，使用条件判断：

### FindMod（判断是否安装了某 Mod）

```xml
<Operation Class="PatchOperationFindMod">
  <modId><!-- 目标Mod的packageId --></modId>
  <!-- Mod存在时执行的操作 -->
  <match>
    <Operation Class="PatchOperationAdd">
      <xpath>...</xpath>
      <value>...</value>
    </Operation>
  </match>
</Operation>
```

### Conditional + Sequence（多条件组合）

```xml
<Operation Class="PatchOperationConditional">
  <!-- 条件1：DLC存在 -->
  <modsConfigData>
    <li>
      <key><!-- DLC的packageId --></key>
      <value Class="ParseInstruction">true</value>
    </li>
  </modsConfigData>
  <!-- 条件满足时执行 -->
  <match Class="PatchOperationSequence">
    <operations>
      <li Class="PatchOperationAdd">
        <xpath>...</xpath>
        <value>...</value>
      </li>
      <li Class="PatchOperationReplace">
        <xpath>...</xpath>
        <value>...</value>
      </li>
    </operations>
  </match>
</Operation>
```

### 常用 DLC packageId

| DLC | packageId |
|-----|----------|
| RimWorld - Royalty | `ludeon.rimworld.royalty` |
| RimWorld - Ideology | `ludeon.rimworld.ideology` |
| RimWorld - Biotech | `ludeon.rimworld.biotech` |
| RimWorld - Anomaly | `ludeon.rimworld.anomaly` |

> 完整 DLC packageId 列表见 `references/frameworks.md`。

---

## 步骤 6：创建 Patch 文件

### 文件命名规范

```
Patches/Patch_<目标>_<修改内容>.xml
```

示例：
- `Patches/Patch_VanillaWeapons_Damage.xml` — 修改原版武器伤害
- `Patches/Patch_Smithy_AddRecipes.xml` — 给锻造台添加配方
- `Patches/Patch_Buildings_Texture.xml` — 修改建筑贴图

### 文件结构

```xml
<?xml version="1.0" encoding="utf-8"?>
<Patch>

  <!-- 修改说明注释 -->
  <Operation Class="PatchOperationReplace">
    <xpath>/Defs/ThingDef[defName="目标defName"]/字段路径</xpath>
    <value>
      <字段名>新值</字段名>
    </value>
  </Operation>

  <!-- 可包含多个 Operation -->
  <Operation Class="PatchOperationAdd">
    <xpath>...</xpath>
    <value>...</value>
  </Operation>

</Patch>
```

### 文件位置

```
<Mod名>/
├── About/
├── Defs/
├── Patches/               ← Patch 文件放这里
│   ├── Patch_目标1_修改.xml
│   └── Patch_目标2_修改.xml
└── Textures/
```

---

## 实战示例

### 示例 1：修改原版武器伤害

将原版 `Knife` 的近战伤害从默认值修改为新值：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Patch>
  <!-- 修改原版Knife的伤害值 -->
  <Operation Class="PatchOperationReplace">
    <xpath>/Defs/ThingDef[defName="Knife"]/statBases/MeleeWeapon_DamageAmount</xpath>
    <value>
      <MeleeWeapon_DamageAmount>12</MeleeWeapon_DamageAmount>
    </value>
  </Operation>
</Patch>
```

### 示例 2：给工作台添加配方

给原版 `ElectricSmithy` 添加新配方：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Patch>
  <!-- 如果工作台没有recipes节点，先创建 -->
  <Operation Class="PatchOperationSequence">
    <operations>
      <li Class="PatchOperationAdd">
        <xpath>/Defs/ThingDef[defName="ElectricSmithy"]</xpath>
        <value>
          <recipes />
        </value>
      </li>
    </operations>
  </Operation>

  <!-- 添加配方 -->
  <Operation Class="PatchOperationAdd">
    <xpath>/Defs/ThingDef[defName="ElectricSmithy"]/recipes</xpath>
    <value>
      <li>你的配方defName</li>
    </value>
  </Operation>
</Patch>
```

> 注意：如果工作台已有 `recipes` 节点，Add 操作直接添加即可，不需要先创建。
> 如果不确定节点是否存在，可使用 `PatchOperationConditional` 判断。

### 示例 3：修改建筑贴图

将原版建筑的贴图替换为本 Mod 的贴图：

```xml
<?xml version="1.0" encoding="utf-8"?>
<Patch>
  <!-- 替换原版建筑的贴图路径 -->
  <Operation Class="PatchOperationReplace">
    <xpath>/Defs/ThingDef[defName="目标defName"]/graphicData/texPath</xpath>
    <value>
      <texPath>Things/Building/你的贴图名</texPath>
    </value>
  </Operation>
</Patch>
```

---

## 最佳实践

1. **一文件一修改**：每个 Patch 文件只处理一种修改类型，便于管理和调试
2. **精确 XPath**：使用 `defName` 精确匹配，避免大范围匹配导致意外修改
3. **加注释**：在每个 Operation 上方添加注释，说明修改目的
4. **避免大范围匹配**：不要用通配符匹配过多 Def，可能导致意外行为
5. **检查继承**：被修改的字段可能在 ParentName 中定义，需确认实际位置
6. **测试验证**：修改后进游戏检查是否生效，查看 Player.log 是否有 Patch 报错
7. **顺序注意**：如果一个 Patch 文件中有多个操作，确保操作顺序正确（如先创建节点再添加内容）

---

## 相关文档

- XML PatchOperations 详解：`references/04-xml-patching.md`
- Harmony 补丁：`references/06-harmony.md`
- XML Def 系统：`references/03-xml-defs.md`
- 崩溃排查：`workflows/debug-crash.md`
