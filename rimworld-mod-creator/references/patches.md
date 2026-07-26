# Patch 补丁系统参考

Patch 文件用于在所有 Def 加载完成后修改原版或其他模组的已有 Def。根元素 `<Patch>`，内部含 `<Operation Class="PatchOperationXxx">`，通过 `xpath` 定位目标节点。

## PatchOperation 操作清单

| 操作 | 功能 | 关键子元素 |
|------|------|-----------|
| `PatchOperationAdd` | 添加节点 | `xpath`, `value` |
| `PatchOperationReplace` | 替换节点 | `xpath`, `value` |
| `PatchOperationRemove` | 删除节点 | `xpath` |
| `PatchOperationInsert` | 插入到列表指定位置 | `xpath`, `value`, `order`（Append/Prepend） |
| `PatchOperationFindMod` | 条件：检测模组是否存在 | `mods`, `match` |
| `PatchOperationSequence` | 操作序列打包 | `success`, `operations` |
| `PatchOperationConditional` | 条件分支 | `xpath`, `match`, `nomatch` |
| `PatchOperationAttributeSet` | 设置 XML 属性 | `xpath`, `attribute`, `value` |

## 常见操作示例

### PatchOperationAdd — 添加节点

```xml
<Operation Class="PatchOperationAdd">
  <xpath>Defs/ThingDef[thingClass="Pawn"]/comps</xpath>
  <value>
    <li><compClass>PickUpAndHaul.CompHauledToInventory</compClass></li>
  </value>
</Operation>
```

### PatchOperationReplace — 替换节点

```xml
<Operation Class="PatchOperationReplace">
  <xpath>Defs/HediffDef[defName="Lactating"]/comps/li[@Class="HediffCompProperties_Lactating"]</xpath>
  <value>
    <li Class="HediffCompProperties_EqualMilkingLactating">
      <ticksToFullCharge>15000</ticksToFullCharge>
    </li>
  </value>
</Operation>
```

### PatchOperationRemove — 删除节点

```xml
<Operation Class="PatchOperationRemove">
  <xpath>Defs/HediffDef[defName="Lactating"]/stages</xpath>
</Operation>
```

### PatchOperationInsert — 插入到列表指定位置

```xml
<Operation Class="PatchOperationInsert">
  <xpath>Defs/PawnTableDef[defName="Assign"]/columns/li[text()="HostilityResponse"]</xpath>
  <order>Append</order>   <!-- Append=之后, Prepend=之前 -->
  <value>
    <li>AM_LassoModePawnColumn</li>
  </value>
</Operation>
```

### PatchOperationFindMod — 条件检测模组

```xml
<Operation Class="PatchOperationFindMod">
  <mods>
    <li>Biotech</li>                    <!-- 友好名或 packageId 均可 -->
    <li>Ludeon.RimWorld.Royalty</li>
  </mods>
  <match Class="PatchOperationSequence">
    <success>Always</success>
    <operations>
      <li Class="PatchOperationAdd">...</li>
    </operations>
  </match>
</Operation>
```

### PatchOperationConditional — 条件分支（节点存在性判断）

```xml
<!-- comps 节点存在则往里加 li，不存在则创建 comps 节点 -->
<Operation Class="PatchOperationConditional">
  <xpath>Defs/ThingDef[defName='MeleeWeapon_Club']/comps</xpath>
  <match Class="PatchOperationAdd">
    <xpath>Defs/ThingDef[defName='MeleeWeapon_Club']/comps</xpath>
    <value>
      <li Class="MyMod.CompProperties_Grip">...</li>
    </value>
  </match>
  <nomatch Class="PatchOperationAdd">
    <xpath>Defs/ThingDef[defName='MeleeWeapon_Club']</xpath>
    <value>
      <comps><li Class="MyMod.CompProperties_Grip">...</li></comps>
    </value>
  </nomatch>
</Operation>
```

### PatchOperationAttributeSet — 设置 XML 属性

```xml
<Operation Class="PatchOperationSequence">
  <operations>
    <li Class="PatchOperationAttributeSet">
      <xpath>Defs/ThingDef[defName="Human"]</xpath>
      <attribute>Class</attribute>
      <value>AlienRace.ThingDef_AlienRace</value>
    </li>
  </operations>
</Operation>
```

## XPath 用法总结

| 模式 | 示例 | 说明 |
|------|------|------|
| 按 defName | `Defs/ThingDef[defName="Milk"]` | 最常用，按唯一名定位 |
| 按 Name 属性 | `Defs/ThingDef[@Name="BasePawn"]` | 定位抽象基类 |
| 按 thingClass | `Defs/ThingDef[thingClass="Pawn"]` | 按类定位 |
| 按 Class 属性 | `.../comps/li[@Class="HediffCompProperties_Lactating"]` | 定位特定 comp |
| 多名 OR | `Defs/ThingDef[defName='A' or defName='B']` | 一次匹配多个 |
| 按文本值 | `.../columns/li[text()="HostilityResponse"]` | 定位某值的列表项 |

## 兼容补丁标准范式

用 `PatchOperationFindMod` 检测 DLC/模组、`PatchOperationConditional` 处理节点存在性、`PatchOperationSequence`+`success="Always"` 防止整体失败——这三者组合是兼容性 patch 的标准范式。
