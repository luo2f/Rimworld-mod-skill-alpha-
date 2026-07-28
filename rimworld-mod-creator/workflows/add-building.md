# 添加建筑

> **适用版本**: RimWorld 1.6 | **最后更新**: 2026-07-28

## 概述

本工作流用于在已有 Mod 中添加建筑，包括生产工作台、住宅建筑、防御建筑、储存建筑、装饰建筑等。建筑类型已有对应模板，可直接使用。

**适用场景**：
- Mod 已创建（含基本目录结构）
- 需要新增各类建筑或工作台
- 需要配置电力、可摧毁等组件
- 需要设置研究前置条件

> 如果还没有创建 Mod 项目，请先按 `workflows/new-mod.md` 创建。

---

## 步骤 1：确定建筑类型

根据需求确定建筑类型，不同类型的建筑需要配置不同的字段和组件：

| 建筑类型 | 典型特征 | 关键配置 |
|----------|---------|---------|
| 生产建筑 | 工作台、加工设备 | recipes、WorkTable 标签、电力 Comp |
| 住宅建筑 | 床、桌椅 | bed 类标签、社交类 Comp |
| 防御建筑 | 炮塔、陷阱 | turrets/verbs、电力 Comp |
| 储存建筑 | 仓储架、储物箱 | storage Comp |
| 装饰建筑 | 雕像、地毯 | 美观度 statBases |
| 生产设施 | 炉子、发电机 | 电力 Comp、输出功率 |

记录以下关键信息：
- defName（需加用户选择的前缀）
- 显示名称（label）
- 建筑尺寸（size，如 3x2）
- 建造分类（designationCategory）
- 建造材料（costList）
- 基础属性（statBases）

---

## 步骤 2：加载 building.xml 模板

读取 `templates/building.xml` 模板文件，模板已验证原版结构，包含完整的字段和注释。

模板中包含以下占位符：
- `<YourPrefix_BuildingName>` → 替换为带前缀的 defName
- `<YourBuildingLabel>` → 替换为中文显示名
- `<YourBuildingDescription>` → 替换为中文描述
- `<sizeX>` / `<sizeY>` → 替换为建筑尺寸
- 数值占位符 → 替换为实际数值

---

## 步骤 3：替换占位符

逐一替换模板中的占位符：

### 必填字段

| 占位符 | 替换为 | 注意事项 |
|--------|-------|---------|
| defName | `前缀_建筑名`（如 `XX_Workbench`） | 必须加前缀，确保唯一 |
| label | 中文显示名 | 游戏内显示名称 |
| description | 中文描述 | 鼠标悬停时显示 |
| texPath | 贴图路径 | 指向 Textures/ 下的路径 |
| size | 建筑尺寸 | 格式 `<x>2</x><y>1</y>` 表示 2x1 |

### 核心属性（statBases）

```xml
<statBases>
  <MaxHitPoints><!-- 建筑耐久 --></MaxHitPoints>
  <WorkToBuild><!-- 建造工作量 --></WorkToBuild>
  <Flammability><!-- 易燃性 0-1 --></Flammability>
  <Mass><!-- 重量 --></Mass>
  <Beauty><!-- 美观度，正数为加成 --></Beauty>
</statBases>
```

### 建造分类（designationCategory）

```xml
<designationCategory><!-- 分类名 --></designationCategory>
```

常用分类：
- `Production`：生产（工作台等）
- `Furniture`：家具（床、桌椅等）
- `Power`：电力（发电机等）
- `Security`：防御（炮塔、陷阱等）
- `Structure`：结构（墙、门等）
- `Misc`：杂项

### 建造材料（costList）

```xml
<costList>
  <Steel><!-- 钢材数量 --></Steel>
  <ComponentIndustrial><!-- 工业零件数量 --></ComponentIndustrial>
  <!-- 可添加其他材料 -->
</costList>
```

### 其他重要字段

| 字段 | 说明 | 示例 |
|------|------|------|
| `minifiedDef` | 可搬运（制作后变为物品） | `MinifiedThing`（可搬运）/ `MinifiedTree`（植物类） |
| `rotatable` | 是否可旋转 | `false`（方形建筑设为 false） |
| `blockLight` | 是否阻挡光线 | `true`/`false` |
| `fillPercent` | 占用率 | `0.5`（半墙）/ `1.0`（实心墙） |
| `passability` | 可通过性 | `Standable`/`PassThroughOnly`/`Impassable` |
| `placeWorkers` | 放置规则类 | 如需要特殊放置规则时填写 |

---

## 步骤 4：配置 Comps

根据建筑功能添加对应的 Comp：

### 电力 Comp（CompProperties_Power）

适用于需要用电的建筑：

```xml
<comps>
  <li Class="CompProperties_Power">
    <compClass>CompPowerTrader</compClass>
    <basePowerConsumption><!-- 功耗（负数表示发电） --></basePowerConsumption>
  </li>
  <!-- 需要电力连接时添加 -->
  <li Class="CompProperties_Flickable"/>
</comps>
```

- `basePowerConsumption`：正值表示耗电，负值表示发电
- 需要开关的建筑添加 `CompProperties_Flickable`
- 发电类建筑还需配置 `CompProperties_Glower`（光源）等

### 可摧毁 Comp（CompProperties_Breakdownable）

适用于可能故障的建筑：

```xml
<comps>
  <li Class="CompProperties_Breakdownable"/>
</comps>
```

- 建筑可能随机故障，需要修理
- 常用于工作台、发电机等

### 仓储 Comp（CompProperties_Storage）

适用于储存建筑：

```xml
<comps>
  <li Class="CompProperties_Storage">
    <defaultStorageSettings>
      <!-- 默认储存设置 -->
    </defaultStorageSettings>
  </li>
</comps>
```

### 光源 Comp（CompProperties_Glower）

适用于发光建筑：

```xml
<comps>
  <li Class="CompProperties_Glower">
    <glowRadius><!-- 光照半径 --></glowRadius>
    <glowColor>(255, 255, 255, 0)</glowColor>
  </li>
</comps>
```

### 自定义 ThingComp

如果需要自定义逻辑，编写 C# ThingComp：

```xml
<comps>
  <li Class="<!-- 用户自行填写：namespace.ClassName -->">
    <!-- 自定义属性 -->
  </li>
</comps>
```

> ThingComp 代码模板见 `templates/thingcomp.cs`，C# 开发详见 `references/05-csharp-basics.md`。

---

## 步骤 5：创建贴图

建筑贴图尺寸根据 size 计算：

**计算规则**：每格 256x256 像素

| 建筑尺寸 | 贴图尺寸（像素） |
|---------|----------------|
| 1x1 | 256x256 |
| 2x1 | 512x256 |
| 1x2 | 256x512 |
| 2x2 | 512x512 |
| 3x2 | 768x512 |
| 3x3 | 768x768 |

贴图要求：
- 格式：PNG
- 背景：透明
- 位置：`Textures/Things/Building/<defName>.png`
- 主体应占满贴图区域

```xml
<texPath>Things/Building/<defName></texPath>
```

**多朝向贴图**（如果建筑可旋转）：
- 默认朝南：`<defName>_south.png`
- 朝东：`<defName>_east.png`（可选，没有则镜像）
- 朝北：`<defName>_north.png`（可选，没有则镜像）
- 朝西：`<defName>_west.png`（可选，没有则镜像）

在 Def 中指定：

```xml
<graphicData>
  <texPath>Things/Building/<defName></texPath>
  <graphicClass>Graphic_Multi</graphicClass>
  <drawSize>(2,2)</drawSize>
</graphicData>
```

**测试阶段**：可暂时引用原版贴图路径，后续替换为原创贴图。

> 贴图制作详细说明见 `references/07-assets.md`。

---

## 步骤 6：添加研究前置

如果建筑需要研究后才能建造，添加研究前置：

```xml
<researchPrerequisites>
  <li><!-- 研究项目defName --></li>
</researchPrerequisites>
```

使用原版研究项目：
- `Machining`：机械加工
- `Electricity`：电力
- `Batteries`：电池
- `MicroelectronicsBasics`：微电子基础
- `Gunsmithing`：枪械制造
- `Smithing`：锻造

使用自定义研究项目：
- 在 `Defs/ResearchProjectDefs/` 中定义研究项目
- 然后在建筑的 researchPrerequisites 中引用

```xml
<!-- 自定义研究项目示例 -->
<ResearchProjectDef>
  <defName>前缀_ResearchName</defName>
  <label>研究名称</label>
  <description>研究描述</description>
  <techLevel>Industrial</techLevel>
  <researchViewX>10</researchViewX>
  <researchViewY>5</researchViewY>
  <baseCost>2000</baseCost>
  <prerequisites>
    <li>Smithing</li>
  </prerequisites>
</ResearchProjectDef>
```

> 如果不需要研究前置，删除 researchPrerequisites 节点。

---

## 步骤 7：添加建造配方（如需要）

大多数建筑使用 `costList` 即可直接建造，无需配方。但如果建筑需要通过特定方式获得，可添加配方：

```xml
<RecipeDef>
  <defName>前缀_Make_建筑名</defName>
  <label>制作建筑名</label>
  <description>制作建筑名的描述</description>
  <jobString>正在制作建筑名。</jobString>
  <workAmount><!-- 工作量 --></workAmount>
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
  <products>
    <前缀_建筑defName>1</前缀_建筑defName>
  </products>
  <recipeUsers>
    <li><!-- 工作台defName --></li>
  </recipeUsers>
</RecipeDef>
```

> 配方模板见 `templates/recipe.xml`。

---

## 步骤 8：测试

生成完成后，进行游戏内测试：

1. 启动 RimWorld，确保你的 Mod 已勾选
2. 开启 **DevMode**
3. 开启 **God Mode**（Debug 菜单 → God Mode）可免费建造
4. 在建筑菜单中找到你的建筑，尝试放置建造
5. 验证：
   - 建筑贴图是否正确显示（含旋转后的朝向）
   - 建造后属性是否正确
   - 如果有电力 Comp，检查电力连接是否正常
   - 如果有研究前置，检查是否需要先研究
   - 是否有红字报错
6. 如果是工作台，检查是否能正常工作

---

## 常见问题

### 建筑无法放置

- **原因**：size 设置错误或占用区域被阻挡
- **解决**：检查 size 格式是否正确，确认放置区域无障碍

### 贴图尺寸不对

- **原因**：贴图尺寸与建筑 size 不匹配
- **解决**：按 size x 256 像素重新制作贴图

### 电力建筑不工作

- **原因**：CompProperties_Power 配置错误或未连接电网
- **解决**：
  - 检查 basePowerConsumption 正负值是否正确
  - 确认 CompProperties_Flickable 已添加（需要开关时）
  - 确认建筑已连接到电网

### 建筑在菜单中找不到

- **原因**：designationCategory 设置错误或缺失
- **解决**：确认 designationCategory 使用了有效的分类名

### 旋转后贴图异常

- **原因**：未使用 Graphic_Multi 或多朝向贴图缺失
- **解决**：
  - 添加 `<graphicClass>Graphic_Multi</graphicClass>`
  - 提供 `_south`、`_east`、`_north`、`_west` 四个朝向贴图
  - 或设置 `<rotatable>false</rotatable>` 禁用旋转

### 工作台无法使用配方

- **原因**：未设置 `Building_WorkTable` 的 ParentName 或未添加相关 Comp
- **解决**：确认继承了正确的工作台基类，添加 `CompProperties_AffectedByFacilities` 等

### 研究前置不显示

- **原因**：研究项目 defName 不存在或研究项目未正确配置
- **解决**：确认研究项目 defName 在原版或本 Mod 中已定义

---

## 相关文档

- 新建 Mod：`workflows/new-mod.md`
- 添加物品：`workflows/add-item.md`
- XML Def 系统：`references/03-xml-defs.md`
- 资源制作：`references/07-assets.md`
- C# ThingComp 开发：`references/05-csharp-basics.md`
- 崩溃排查：`workflows/debug-crash.md`
