# XML Def 内容体系参考

## 常见 Def 类型清单

| Def 类型 | 用途 | 关键字段 |
|----------|------|---------|
| `ThingDef` | 物品/建筑/武器/投射物/Pawn/服装 | `ParentName`, `thingClass`, `graphicData`, `statBases`, `comps`, `verbs` |
| `PawnKindDef` | Pawn 种类模板 | `race`, `combatPower`, `apparelRequired`, `defaultFactionType` |
| `RecipeDef` | 制作配方/手术 | `ingredients`, `products`, `recipeUsers`, `workerClass` |
| `HediffDef` | 健康状态/修饰 | `hediffClass`, `stages`, `comps`, `capMods` |
| `ResearchProjectDef` | 研究项目 | `baseCost`, `techLevel`, `prerequisites`, `requiredResearchBuilding` |
| `QuestScriptDef` | 任务脚本 | `root Class="QuestNode_Sequence"` |
| `RulePackDef` | 规则包（日志/文本生成） | `rulesStrings`, `include` |
| `DamageDef` | 伤害类型 | `defaultDamage`, `harmsHealth`, `explosionCellFleck` |
| `BodyDef` | 身体结构（部位树） | `corePart`（嵌套 `parts`） |
| `BodyPartDef` | 身体部位属性 | `hitPoints`, `skinCovered`, `tags` |
| `WorkGiverDef` | 工作给予者 | `giverClass`, `workType`, `priorityInType` |
| `JobDef` | 任务定义 | `driverClass`, `reportString` |
| `ThoughtDef` | 想法定义 | `durationDays`, `stages` |
| `GeneDef` | 基因定义（Biotech） | `displayCategory`, `statOffsets`, `iconPath` |
| `StatDef` | 属性定义 | `workerClass`, `category`, `toStringStyle` |
| `SoundDef` | 音效定义 | `subSounds`, `grains`, `volumeRange` |

所有 Def 文件根元素为 `<Defs>`，游戏递归扫描 Defs/ 下所有 .xml，仅根据 XML 标签名识别 Def 类型，文件名和子目录名不影响加载。

## Def 继承（ParentName）与 Abstract

```xml
<!-- 抽象基类，不会被实例化，需要 Name 属性供引用 -->
<ThingDef ParentName="BuildingBase" Name="EM_MilkingBase" Abstract="True">
  <thingClass>EqualMilking.Building_Milking</thingClass>
  <rotatable>false</rotatable>
</ThingDef>

<!-- 继承 EM_MilkingBase，只需填差异部分 -->
<ThingDef ParentName="EM_MilkingBase">
  <defName>EM_MilkingSpot</defName>
  <label>milking spot</label>
  <graphicData>
    <texPath>Things/Building/Production/MilkingSpot</texPath>
    <graphicClass>Graphic_Single</graphicClass>
  </graphicData>
</ThingDef>
```

### 继承要点

- 继承链可多层：`BuildingBase`（原版）→ `EM_MilkingBase`（abstract）→ 具体建筑
- `Name` 属性在 abstract 基类上必需（供 ParentName 引用）
- abstract 基类不需要 `defName`（因为不实例化），但需要 `Name`
- 子 Def 的字段覆盖父 Def 同名字段，列表字段默认合并父子列表

### Inherit="false"（取消列表继承）

```xml
<weaponTags Inherit="false">
  <li>RK_1TierRange</li>
</weaponTags>
```

## 引用关系（li）

- **列表引用**：`<prerequisites><li>GunTurrets</li></prerequisites>`
- **字段引用**：`<race>Ratkin</race>`、`<damageDef>Bullet</damageDef>`
- **嵌套引用**：`<recipeUsers><li>RK_FueledSmithy</li></recipeUsers>`
- **贴图引用**：`<texPath>Things/Building/MilkingSpot</texPath>`（路径引用，非 Def）

## 代表性 Def 示例

### ThingDef — 武器（含 comps/verbs/tools）

```xml
<ThingDef ParentName="RK_NeolithicRangeWeapon">
  <defName>RK_Crossbow</defName>
  <label>cross bow</label>
  <graphicData>
    <texPath>Weapon/RK_Crossbow</texPath>
    <graphicClass>Graphic_Single</graphicClass>
  </graphicData>
  <costList><WoodLog>40</WoodLog><Steel>20</Steel></costList>
  <statBases>
    <WorkToMake>2400</WorkToMake>
    <RangedWeapon_Cooldown>1.2</RangedWeapon_Cooldown>
  </statBases>
  <verbs>
    <li>
      <verbClass>Verb_Shoot</verbClass>
      <defaultProjectile>Bolt_RK_Crossbow</defaultProjectile>
      <warmupTime>1.4</warmupTime>
      <range>22.9</range>
      <soundCast>Bow_Small</soundCast>
    </li>
  </verbs>
  <tools>
    <li><label>limb</label><power>9</power></li>
  </tools>
</ThingDef>
```

### ThingDef — 建筑（含 CompProperties）

```xml
<ThingDef ParentName="BuildingBase">
  <defName>DNX_Turret</defName>
  <comps>
    <li Class="CompProperties_Power">
      <compClass>CompPowerTrader</compClass>
      <basePowerConsumption>20</basePowerConsumption>
    </li>
    <li Class="CompProperties_Flickable" />
  </comps>
</ThingDef>
```

### HediffDef — 含 stages/comps/capMods

```xml
<HediffDef>
  <defName>AM_KnockedOut</defName>
  <hediffClass>HediffWithComps</hediffClass>
  <comps>
    <li Class="HediffCompProperties_Disappears">
      <disappearsAfterTicks>3600~5400</disappearsAfterTicks>
    </li>
  </comps>
  <stages>
    <li>
      <capMods>
        <li><capacity>Consciousness</capacity><setMax>0.1</setMax></li>
      </capMods>
    </li>
  </stages>
</HediffDef>
```

### BodyDef — 身体部位树

```xml
<BodyDef>
  <defName>RK_Body_Ratkin</defName>
  <corePart>
    <def>Torso</def>
    <height>Middle</height>
    <parts>
      <li>
        <def>Heart</def>
        <coverage>0.020</coverage>
        <depth>Inside</depth>
      </li>
      <li>
        <def>Neck</def>
        <coverage>0.075</coverage>
        <parts>
          <li>
            <def>Head</def>
            <coverage>0.80</coverage>
          </li>
        </parts>
      </li>
    </parts>
  </corePart>
</BodyDef>
```

### ResearchProjectDef

```xml
<ResearchProjectDef>
  <defName>DNX_MediumCaliber</defName>
  <label>medium-caliber weapons</label>
  <baseCost>1200</baseCost>
  <techLevel>Industrial</techLevel>
  <prerequisites>
    <li>DNX_AutomatedSentrySystems</li>
  </prerequisites>
  <requiredResearchBuilding>HiTechResearchBench</requiredResearchBuilding>
  <researchViewX>1.60</researchViewX>
  <researchViewY>2.00</researchViewY>
</ResearchProjectDef>
```

## 自定义 Def 类型

C# 模组可注册自定义 Def 类型，XML 中用 `命名空间.DefType名` 作为标签：

```xml
<AM.AnimDef Name="AnimDuelBase" Abstract="True">
  <label>Duel</label>
  <type>Duel</type>
</AM.AnimDef>
```

## 特殊语法

| 语法 | 说明 | 示例 |
|------|------|------|
| `~` 随机范围 | 运行时取区间随机值 | `<disappearsAfterTicks>3600~5400</...>` |
| 颜色格式 | `(R, G, B, A)`，值域 0~1 或 0~255 | `(255,97,179,255)` |
| `graphicClass` | 贴图渲染类型 | `Graphic_Single`、`Graphic_Multi`、`Graphic_Random` |
