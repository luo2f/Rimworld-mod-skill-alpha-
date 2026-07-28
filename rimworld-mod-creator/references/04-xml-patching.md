# 04 - XML PatchOperations

> **适用版本**: RimWorld 1.6 | **最后更新**: 2026-07-28

本文档讲解如何用 XML PatchOperations 修改原版或其他模组的 Def，实现无需 C# 的运行时数据改写，涵盖 XPath 选择器、8 种操作、条件补丁与兼容范式。

---

## 一、概述

- Patch 文件放在模组的 `Patches/` 目录下，根元素为 `<Patch>`。
- 游戏加载所有 Def 后，按模组顺序依次应用 Patch，对已有 Def 进行增删改。
- 优势：无需 C# 编译，修改原版数据、与其他模组兼容的利器。

```xml
<?xml version="1.0" encoding="utf-8"?>
<Patch>
  <!-- 一系列 PatchOperation -->
</Patch>
```

---

## 二、XPath 选择器基础

每个 `PatchOperation` 通过 `xpath` 定位目标节点。常用选择器：

### 2.1 按 defName 选择

```xml
<!-- 选中 defName 为 Steel 的 ThingDef -->
xpath="Defs/ThingDef[defName="Steel"]"
```

### 2.2 按 Name 属性选择（抽象模板）

```xml
<!-- 选中 Name="BaseGun" 的模板 -->
xpath="Defs/ThingDef[@Name="BaseGun"]"
```

### 2.3 按 thingClass 选择

```xml
<!-- 选中所有 thingClass 为 Building 的 ThingDef -->
xpath="Defs/ThingDef[thingClass="Building"]"
```

### 2.4 按 Class 属性选择（comps 中的类）

```xml
<!-- 选中所有 CompProperties_Quality 节点 -->
xpath="Defs/ThingDef/comps/li[@Class="CompProperties_Quality"]"
```

### 2.5 多名 OR

```xml
<!-- 选中 defName 为 Steel 或 Plasteel 的 Def -->
xpath="Defs/ThingDef[defName="Steel" or defName="Plasteel"]"
```

### 2.6 按文本值选择

```xml
<!-- 选中 weaponTags 中文本为 "Melee" 的 li -->
xpath="Defs/ThingDef/weaponTags/li[text()="Melee"]"
```

> XPath 路径必须以 `Defs/` 开头（Patch 应用于解析后的 Def 树）。`@` 表示属性，`[...]` 表示谓词条件。

---

## 三、8 种 PatchOperation 操作详解

### 3.1 PatchOperationAdd（添加子节点）

向目标节点下追加子节点。

```xml
<Operation Class="PatchOperationAdd">
  <xpath>Defs/ThingDef[defName="Steel"]</xpath>
  <value>
    <testField>42</testField>
  </value>
</Operation>
```

可选 `order="Append/Prepend"` 控制插入到列表首尾：

```xml
<Operation Class="PatchOperationAdd">
  <xpath>Defs/ThingDef[defName="Steel"]/statBases</xpath>
  <order>Prepend</order>
  <value>
    <Beauty>1</Beauty>
  </value>
</Operation>
```

### 3.2 PatchOperationRemove（删除节点）

```xml
<Operation Class="PatchOperationRemove">
  <xpath>Defs/ThingDef[defName="Steel"]/testField</xpath>
</Operation>
```

### 3.3 PatchOperationReplace（替换节点）

```xml
<Operation Class="PatchOperationReplace">
  <xpath>Defs/ThingDef[defName="Steel"]/statBases/MaxHitPoints</xpath>
  <value>
    <MaxHitPoints>200</MaxHitPoints>
  </value>
</Operation>
```

### 3.4 PatchOperationInsert（按位置插入）

```xml
<Operation Class="PatchOperationInsert">
  <xpath>Defs/ThingDef[defName="Steel"]/comps</xpath>
  <order>Append</order>
  <value>
    <li Class="CompProperties_Forbiddable" />
  </value>
</Operation>
```

### 3.5 PatchOperationFindMod（条件：检测模组）

仅当指定模组存在时，才应用其包裹的操作。

```xml
<Operation Class="PatchOperationFindMod">
  <mods>
    <li>Ludeon.RimWorld.Royalty</li>
  </mods>
  <match>
    <!-- 此处的操作仅在 Royalty 存在时应用 -->
    <Operation Class="PatchOperationAdd">
      <xpath>Defs/ThingDef[defName="<YourPrefix>_RoyalItem"]</xpath>
      <value>
        <royaltyTag>true</royaltyTag>
      </value>
    </Operation>
  </match>
</Operation>
```

### 3.6 PatchOperationSequence（序列容器）

将多个操作打包为一个序列，可配合 `success` 属性。

```xml
<Operation Class="PatchOperationSequence">
  <success>Always</success>
  <operations>
    <li Class="PatchOperationAdd">
      <xpath>Defs/ThingDef[defName="Steel"]</xpath>
      <value><testField>1</testField></value>
    </li>
    <li Class="PatchOperationReplace">
      <xpath>Defs/ThingDef[defName="Steel"]/statBases/MaxHitPoints</xpath>
      <value><MaxHitPoints>200</MaxHitPoints></value>
    </li>
  </operations>
</Operation>
```

### 3.7 PatchOperationConditional（条件：检测节点存在性）

仅当 `xpath` 选中的节点存在时，才应用 `match` 内的操作；否则可选 `nomatch`。

```xml
<Operation Class="PatchOperationConditional">
  <xpath>Defs/ThingDef[defName="Steel"]/statBases</xpath>
  <match>
    <Operation Class="PatchOperationAdd">
      <xpath>Defs/ThingDef[defName="Steel"]/statBases</xpath>
      <value><Beauty>1</Beauty></value>
    </Operation>
  </match>
</Operation>
```

### 3.8 PatchOperationAttributeSet（设置属性）

为目标节点设置 XML 属性。

```xml
<Operation Class="PatchOperationAttributeSet">
  <xpath>Defs/ThingDef[defName="Steel"]/statBases/Beauty</xpath>
  <attribute>Inherit</attribute>
  <value>False</value>
</Operation>
```

### 操作速查表

| 操作 | 作用 | 关键子元素 |
|------|------|------------|
| `PatchOperationAdd` | 追加子节点 | `value`、`order` |
| `PatchOperationRemove` | 删除节点 | 无 |
| `PatchOperationReplace` | 替换节点 | `value` |
| `PatchOperationInsert` | 按位置插入 | `value`、`order` |
| `PatchOperationFindMod` | 检测模组存在 | `mods`、`match` |
| `PatchOperationSequence` | 操作序列 | `operations`、`success` |
| `PatchOperationConditional` | 检测节点存在 | `match`、`nomatch` |
| `PatchOperationAttributeSet` | 设置属性 | `attribute`、`value` |

---

## 四、条件补丁

### 4.1 PatchOperationFindMod 检测 DLC/模组

`<mods>` 中填 packageId，支持多个（任一存在即匹配）：

```xml
<Operation Class="PatchOperationFindMod">
  <mods>
    <li>Ludeon.RimWorld.Royalty</li>
    <li>Ludeon.RimWorld.Biotech</li>
  </mods>
  <match>
    <!-- 操作 -->
  </match>
</Operation>
```

### 4.2 PatchOperationConditional 处理节点存在性

常用于「目标 Def 可能被其他模组删除/改名」的兼容场景，避免 Patch 失败报红：

```xml
<Operation Class="PatchOperationConditional">
  <xpath>Defs/ThingDef[defName="<YourPrefix>_MaybeRemoved"]</xpath>
  <match>
    <!-- 仅在目标存在时操作 -->
  </match>
</Operation>
```

---

## 五、兼容补丁标准范式

为其他模组/DLC 写兼容补丁时，推荐组合 `FindMod + Conditional + Sequence + success="Always"`，确保：

1. 目标模组未启用时不报错。
2. 目标 Def 可能被改动时仍稳健。
3. 序列整体报成功，避免单个失败连累整个 Patch 文件。

```xml
<Operation Class="PatchOperationFindMod">
  <mods>
    <li>Ludeon.RimWorld.Royalty</li>
  </mods>
  <match>
    <Operation Class="PatchOperationSequence">
      <success>Always</success>
      <operations>
        <li Class="PatchOperationConditional">
          <xpath>Defs/ThingDef[defName="<YourPrefix>_Target"]</xpath>
          <match>
            <Operation Class="PatchOperationAdd">
              <xpath>Defs/ThingDef[defName="<YourPrefix>_Target"]</xpath>
              <value>
                <royaltyOnlyField>true</royaltyOnlyField>
              </value>
            </Operation>
          </match>
        </li>
      </operations>
    </Operation>
  </match>
</Operation>
```

> `success="Always"` 的含义：即使内部某个操作未命中目标，序列整体也标记为成功，不产生「Patch failed」红字。

---

## 六、调试 Patch 方法

1. **看日志**：启动游戏后查看 `Player.log`，搜索 `PatchOperation` 关键字。失败的 Patch 会输出 `Could not apply` 及对应 xpath。
2. **开发者日志**：开启开发者模式 → 日志面板可实时查看 Patch 应用情况。
3. **缩小范围**：把可疑 Patch 单独提取到一个文件，逐一测试。
4. **验证 xpath**：在 dnSpy 或 RimWorld 编辑器内无法直接验证 xpath，可在日志中确认 `xpath` 命中的节点数；若为 0 说明选择器有误。
5. **顺序问题**：若 Patch 依赖其他模组的 Def，务必在 `About.xml` 中 `loadAfter` 该模组。

---

## 七、最佳实践

- **优先用条件包裹**：对可能不存在目标（其他模组/DLC）的 Patch，始终用 `FindMod` 或 `Conditional` 包裹，避免红字。
- **xpath 精确**：尽量用 `defName` 精确定位，避免宽泛选择器误伤其他 Def。
- **合并文件**：相关 Patch 归入同一文件（如 `Patches_Royalty.xml`），便于维护。
- **加 `success="Always"`**：对非关键兼容补丁，序列容器加 `success="Always"` 防止报红。
- **不要 Patch 过深**：避免依赖其他模组内部私有字段，否则对方更新易破坏你的 Patch。
- **测试覆盖**：单独加载（Core + 目标模组 + 你的 Patch）验证，再叠加其他模组回归测试。
