# 03 - XML Def 系统

> **适用版本**: RimWorld 1.6 | **最后更新**: 2026-07-28

本文档讲解 RimWorld 的核心数据系统——Def：什么是 Def、常见 Def 类型、ThingDef 详解、继承机制、条件加载、引用关系与特殊语法。

---

## 一、什么是 Def

RimWorld 采用「数据与行为分离」的架构：

- **XML 定义数据（Def）**：物品属性、配方、研究、建筑、贴图路径等，全部写在 `Defs/` 下的 XML 中。
- **C# 定义行为（ThingComp / ThingClass）**：物品在游戏中的逻辑行为写在 C# 代码里，通过 `thingClass` 或 `comps` 与 Def 关联。

一个 Def 就是一条由 XML 描述的对象定义。游戏启动时会把所有 XML 解析为强类型的 C# 对象（如 `ThingDef`、`RecipeDef`），存入 `DefDatabase` 供运行时查询。

```xml
<!-- 一条最简单的 ThingDef -->
<Defs>
  <ThingDef>
    <defName><YourPrefix>_SampleItem</defName>
    <label>sample item</label>
    <description>A sample item.</description>
  </ThingDef>
</Defs>
```

> 所有 Def 必须包裹在根元素 `<Defs>` 内，一个文件可包含多条不同类型的 Def。

---

## 二、16 种常见 Def 类型速查表

| Def 类型 | 作用 | 典型字段 |
|----------|------|----------|
| `ThingDef` | 物品/建筑/植物/ Pawn 等一切「东西」 | category, thingClass, statBases, comps |
| `PawnKindDef` | 生成 Pawn 的模板（兵种、动物种类） | race, combatPower, lifeStages |
| `RecipeDef` | 配方（制作、手术、烹饪） | products, ingredients, workers |
| `HediffDef` | 健康/状态修饰（疾病、改造、增益） | hediffClass, stages, initialSeverity |
| `ResearchProjectDef` | 研究项目 | researchCost, prerequisites, techLevel |
| `QuestScriptDef` | 任务脚本模板 | root, rulesStrings |
| `RulePackDef` | 规则包（文本生成、名称） | rulePack |
| `DamageDef` | 伤害类型（切割、钝击、燃烧） | harmCategory, armorCategory |
| `BodyDef` | 身体结构定义 | corePart, parts |
| `BodyPartDef` | 身体部位定义 | hitPoints, frostbiteVulnerability |
| `WorkGiverDef` | 工作分配器（决定小人做什么工作） | giverClass, workType, priorityInType |
| `JobDef` | 工作（Job）定义 | driverClass, reportString |
| `ThoughtDef` | 想法/情绪 | stages, thoughtClass |
| `GeneDef` | 基因（Biotech） | displayCategory, biostatCpx |
| `StatDef` | 属性定义（自定义数值属性） | workerClass, defaultBaseValue |
| `SoundDef` | 音效定义 | sustain, subSounds |

---

## 三、ThingDef 详解

ThingDef 是使用频率最高的 Def。以下按功能模块讲解关键字段。

### 3.1 基础信息

```xml
<ThingDef ParentName="BaseWeapon">
  <defName><YourPrefix>_IronSword</defName>   <!-- 唯一标识 -->
  <label>iron sword</label>                   <!-- 显示名 -->
  <description>A simple iron sword.</description>
  <thingClass>Verse.ThingWithComps</thingClass> <!-- C# 行为类 -->
  <category>Item</category>                    <!-- 分类：Item/Pawn/Building/Ethereal -->
  <tradeability>Sellable</tradeability>
  <drawGUIOverlay>true</drawGUIOverlay>
</ThingDef>
```

### 3.2 物品分类（category）

| category | 含义 |
|----------|------|
| `Item` | 可拾取物品 |
| `Building` | 建筑 |
| `Pawn` | 生物 |
| `Ethereal` | 虚拟物（无实体，如投射物逻辑） |
| `Plant` | 植物 |
| `Fleshlie` | 尸怪（Anomaly） |

### 3.3 statBases（属性基础值）

```xml
<statBases>
  <MaxHitPoints>100</MaxHitPoints>
  <MarketValue>120</MarketValue>
  <Mass>1.2</Mass>
  <Flammability>0.6</Flammability>
  <DeteriorationRate>0</DeteriorationRate>
  <Beauty>2</Beauty>
  <WorkToMake>800</WorkToMake>
</statBases>
```

### 3.4 材料系统 vs costList

RimWorld 有两种成本定义方式：

**stuff 材料系统**：物品可由任意「材料（stuff）」制造，属性随材料变化。需同时声明 `stuffCategories`（允许的材料类别）与 `costStuffCount`（所需材料数量）。

```xml
<stuffCategories>
  <li>Metallic</li>
  <li>Woody</li>
</stuffCategories>
<costStuffCount>40</costStuffCount>
```

**costList 固定配方**：使用固定材料组合，不随材料变化，适合复合配方或非材料制品。

```xml
<costList>
  <Steel>30</Steel>
  <ComponentIndustrial>2</ComponentIndustrial>
</costList>
```

| 对比 | stuff 材料系统 | costList 固定配方 |
|------|---------------|-------------------|
| 材料是否可选 | 是，玩家选材料 | 否，固定 |
| 属性是否随材料变 | 是 | 否 |
| 典型用途 | 武器、建筑 | 弹药、复合装置 |

> 二者可共存：一件物品既可消耗固定零件，又可选材料主体。

### 3.5 战斗属性（武器）

```xml
<verbs>
  <li>
    <verbClass>Verb_MeleeAttack</verbClass>
    <hasStandardCommand>true</hasStandardCommand>
    <meleeDamageBaseAmount>12</meleeDamageBaseAmount>
    <meleeDamageDef>Cut</meleeDamageDef>
    <meleeArmorPenetration>0.25</meleeArmorPenetration>
  </li>
</verbs>
```

### 3.6 comps 组件系统

`comps` 让一个 Def 以组合方式挂载可复用功能模块（每个 comp 对应一对 `CompProperties` + `ThingComp`）：

```xml
<comps>
  <li Class="CompProperties_Quality">
    <compClass>CompQuality</compClass>
  </li>
  <li Class="CompProperties_Forbiddable" />
  <li Class="CompProperties_Explosive">
    <explosiveDamageType>Bomb</explosiveDamageType>
    <explosiveRadius>2.5</explosiveRadius>
  </li>
</comps>
```

> 自定义 Comp 详见 `05-csharp-basics.md`「ThingComp 自定义组件」。

### 3.7 贴图 graphicData

```xml
<graphicData>
  <texPath>Things/Item/Equipment/WeaponMelee/<YourPrefix>_IronSword</texPath>
  <graphicClass>Graphic_Single</graphicClass>
  <drawSize>1.0</drawSize>
  <color>(255,255,255)</color>
</graphicData>
```

### 3.8 建筑特殊标签

```xml
<thingClass>Building</thingClass>
<category>Building</category>
<building>
  <isEdifice>false</isEdifice>           <!-- 是否占据格子 -->
  <claimable>false</claimable>            <!-- 是否可被占领 -->
  <deconstructible>true</deconstructible>
</building>
<fillPercent>0.5</fillPercent>            <!-- 掩体遮蔽程度 -->
<passability>PassThroughOnly</passability>
<pathCost>50</pathCost>
<size>(1,1)</size>                        <!-- 占地格数 -->
<rotatable>true</rotatable>
```

---

## 四、ParentName 继承机制

RimWorld 的 Def 支持「模板继承」：用 `ParentName` 指定父模板，子 Def 自动继承父模板的全部字段，并可覆盖。父模板用 `Abstract="true"` 标记为不可实例化，并必须有 `Name` 属性供引用。

### 4.1 完整继承链示意

原版武器/建筑普遍采用多层继承，避免重复定义。典型继承链如下：

**近战武器链：**
```
BaseWeapon
  └─ BaseMeleeWeapon
       └─ BaseMeleeWeapon_Sharp
            └─ BaseMeleeWeapon_Sharp_Quality
```

**远程武器链：**
```
BaseGun
  └─ BaseGunWithQuality
       └─ BaseMakeableGun
            └─ BaseHumanMakeableGun
```

**建筑链：**
```
BaseBuilding
  └─ BuildingBase
```

### 4.2 继承写法

```xml
<!-- 父模板：抽象，不生成实例 -->
<ThingDef Name="BaseMeleeWeapon_Sharp" Abstract="True">
  <category>Item</category>
  <thingClass>Verse.ThingWithComps</thingClass>
  <equipmentType>Primary</equipmentType>
  <techLevel>Industrial</techLevel>
  <comps>
    <li Class="CompProperties_Quality" />
    <li Class="CompProperties_Forbiddable" />
  </comps>
</ThingDef>

<!-- 子 Def：继承并覆盖 -->
<ThingDef ParentName="BaseMeleeWeapon_Sharp">
  <defName><YourPrefix>_SteelBlade</defName>
  <label>steel blade</label>
  <!-- 未写的字段自动继承父模板 -->
</ThingDef>
```

### 4.3 Abstract 与 Name 属性

- `Abstract="True"`：模板不实例化，仅作继承基底。
- `Name="XXX"`：给模板命名，供 `ParentName` 引用。非抽象 Def 也可有 `Name` 并被继承。

---

## 五、MayRequire / MayRequireAnyOf 条件加载

在 Def 字段或整个 Def 上加 `MayRequire` 属性，可让该字段/Def 仅在指定 DLC 或模组激活时生效。

```xml
<!-- 单个条件：仅当 Royalty DLC 启用时该 VersepDlcOnly 字段生效 -->
<ThingDef ParentName="BaseWeapon">
  <defName><YourPrefix>_RoyalBlade</defName>
  <comps>
    <li Class="CompProperties_Quality" />
    <!-- 仅 Biotech 启用时加载此 comp -->
    <li Class="SomeCompProperties" MayRequire="Ludeon.RimWorld.Biotech" />
  </comps>
  <!-- 仅 Royalty 启用时该贴图生效 -->
  <graphicData MayRequire="Ludeon.RimWorld.Royalty">
    <texPath>Things/RoyalBlade</texPath>
  </graphicData>
</ThingDef>
```

- `MayRequire="id"`：单个模组/DLC 激活时生效。
- `MayRequireAnyOf="id1,id2"`：任一指定的模组/DLC 激活时生效。

> 这是一种轻量级的条件加载，无需新建 `LoadFolders.xml` 文件夹，适合少量字段的 DLC 兼容。

---

## 六、Def 继承 Abstract 与 Name 属性（小结）

| 属性 | 作用 | 用法 |
|------|------|------|
| `Abstract="True"` | 标记为不可实例化模板 | 放在父 Def 上 |
| `Name="XXX"` | 命名模板供引用 | 父 Def 命名，子用 `ParentName="XXX"` |
| `ParentName="XXX"` | 指定父模板 | 子 Def 继承父字段 |
| `Inherit="False"` | 取消列表继承（见下节） | 加在 `<li>` 或列表节点上 |

---

## 七、Inherit="false" 取消列表继承

默认情况下，子 Def 的列表字段会**追加**到父模板的列表上。若想让子 Def 的列表完全**替换**父列表，在列表节点上加 `Inherit="false"`：

```xml
<!-- 父模板 -->
<ThingDef Name="BaseWeapon" Abstract="True">
  <comps>
    <li Class="CompProperties_Quality" />
    <li Class="CompProperties_Forbiddable" />
  </comps>
</ThingDef>

<!-- 子 Def：Inherit="false" 使 comps 完全替换，不保留父的两项 -->
<ThingDef ParentName="BaseWeapon">
  <defName><YourPrefix>_SpecialWeapon</defName>
  <comps Inherit="false">
    <li Class="CompProperties_RestrictedPlacement" />
  </comps>
</ThingDef>
```

> 不加 `Inherit="false"` 时，子 Def 的 comps 会是「Quality + Forbiddable + RestrictedPlacement」三项。

---

## 八、引用关系

Def 之间通过字符串相互引用，引用方式有三种：

### 8.1 li 列表引用

```xml
<ingredients>
  <li>Steel</li>          <!-- 引用 ThingDef 的 defName -->
</ingredients>
```

### 8.2 字段引用

```xml
<stuffCategories>
  <li>Metallic</li>
</stuffCategories>
<race>Human</race>          <!-- 引用 PawnKindDef / ThingDef -->
<damageDef>Cut</damageDef>  <!-- 引用 DamageDef -->
```

### 8.3 贴图引用

```xml
<graphicData>
  <texPath>Things/Item/YourTexture</texPath>  <!-- 相对 Textures/，不带扩展名 -->
</graphicData>
```

> 引用错误是模组报红的头号原因，详见 `08-debugging.md`「Could not resolve cross-reference」。

---

## 九、特殊语法

### 9.1 ~ 随机范围

部分数值字段支持 `~` 表示随机范围：

```xml
<marketValue>100~150</marketValue>   <!-- 100 到 150 随机 -->
```

### 9.2 颜色格式

```xml
<color>(255,128,0)</color>          <!-- RGB 0-255 -->
<color>(255,128,0,200)</color>      <!-- RGBA，A 为不透明度 0-255 -->
```

### 9.3 graphicClass 类型

| graphicClass | 渲染方式 |
|--------------|----------|
| `Graphic_Single` | 单张图 |
| `Graphic_Multi` | 四方向（前/后/左/右，用后缀 _n/_s/_e/_w 区分） |
| `Graphic_Random` | 同目录多张图随机抽取 |
| `Graphic_Animated` | 序列帧动画 |
| `Graphic_Slicer` | 切片（用于大型建筑） |

---

## 十、完整武器 Def 示例（带详细注释）

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>

  <!-- 抽象父模板：定义所有近战尖锐武器共有属性 -->
  <ThingDef Name="<YourPrefix>_BaseMeleeSharp" Abstract="True">
    <category>Item</category>
    <thingClass>Verse.ThingWithComps</thingClass>
    <equipmentType>Primary</equipmentType>
    <techLevel>Industrial</techLevel>
    <tradeability>Sellable</tradeability>
    <drawGUIOverlay>true</drawGUIOverlay>
    <statBases>
      <MaxHitPoints>100</MaxHitPoints>
      <Flammability>0.6</Flammability>
      <DeteriorationRate>0</DeteriorationRate>
    </statBases>
    <comps>
      <li Class="CompProperties_Quality" />
      <li Class="CompProperties_Forbiddable" />
    </comps>
    <weaponTags>
      <li>Melee</li>
    </weaponTags>
  </ThingDef>

  <!-- 具体武器：继承父模板 -->
  <ThingDef ParentName="<YourPrefix>_BaseMeleeSharp">
    <defName><YourPrefix>_IronGlaive</defName>
    <label>iron glaive</label>
    <description>A long iron glaive, good for keeping foes at distance.</description>

    <!-- 可选材料：金属/木质 -->
    <stuffCategories>
      <li>Metallic</li>
      <li>Woody</li>
    </stuffCategories>
    <costStuffCount>50</costStuffCount>

    <!-- 覆盖父模板属性 -->
    <statBases>
      <MaxHitPoints>120</MaxHitPoints>
      <Mass>2.5</Mass>
      <MarketValue>180</MarketValue>
      <WorkToMake>1200</WorkToMake>
      <MeleeWeapon_Cooldown>2.2</MeleeWeapon_Cooldown>
    </statBases>

    <!-- 贴图：单图，相对 Textures/ -->
    <graphicData>
      <texPath>Things/Item/Equipment/WeaponMelee/<YourPrefix>_IronGlaive</texPath>
      <graphicClass>Graphic_Single</graphicClass>
      <drawSize>(1.2,1.2)</drawSize>
    </graphicData>

    <!-- 战斗属性 -->
    <verbs>
      <li>
        <verbClass>Verb_MeleeAttack</verbClass>
        <hasStandardCommand>true</hasStandardCommand>
        <meleeDamageBaseAmount>14</meleeDamageBaseAmount>
        <meleeDamageDef>Cut</meleeDamageDef>
        <meleeArmorPenetration>0.30</meleeArmorPenetration>
      </li>
    </verbs>

    <!-- 仅当 Biotech DLC 启用时，挂载一个自定义 comp -->
    <comps>
      <li Class="CompProperties_Quality" />
      <li Class="CompProperties_Forbiddable" />
      <li Class="<YourPrefix>.CompProperties_Coating" MayRequire="Ludeon.RimWorld.Biotech" />
    </comps>
  </ThingDef>

</Defs>
```

---

## 十一、常见问题 FAQ

**Q：为什么我的 Def 报红「Could not resolve cross-reference」？**
A：引用了不存在的 defName。检查大小写、拼写、前缀，并确认被引用的 Def 已加载（注意加载顺序与 `loadAfter`）。

**Q：子 Def 修改了 `statBases` 里的一个值，其他值会丢失吗？**
A：不会。`statBases` 是按子项合并的，只覆盖你写的子项，未写的继承父模板。但列表类字段（如 `comps`、`weaponTags`）默认追加，需用 `Inherit="false"` 才能替换。

**Q：`Abstract="True"` 的模板会出现在游戏里吗？**
A：不会。抽象模板只作继承基底，不生成实例，也不会出现在搜索/生成中。

**Q：`MayRequire` 和 `LoadFolders.xml` 该用哪个？**
A：少量字段的 DLC/模组兼容用 `MayRequire`；整批文件（多个 Def、贴图、DLL）的条件加载用 `LoadFolders.xml` 文件夹。

**Q：贴图路径要不要写 `.png` 后缀？**
A：不要。`texPath` 相对于 `Textures/`，且省略扩展名，用正斜杠分隔目录。
