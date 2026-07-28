# 框架库清单

> **适用版本**: RimWorld 1.6 | **最后更新**: 2026-07-28

本文档汇总 RimWorld 模组生态中常见的框架库、NuGet 包与 DLC packageId，供依赖声明与开发参考。

---

## 一、框架库清单表

框架库（Framework）本身不直接提供游戏内容，而是为其他模组提供可复用的功能底座。下表列出常见框架，模组可在 `About.xml` 中将其声明为 `modDependencies` 前置。

| 框架名称 | packageId | 功能 | 典型被依赖场景 |
|----------|-----------|------|----------------|
| Harmony | `brrainz.harmony` | 运行时方法插桩，修改游戏行为 | 所有需要 C# Patch 的模组的基础前置 |
| HugsLib | `UnlimitedHugs.HugsLib` | 通用工具库：设置持久化、日志、Tick 调度、资源加载等增强 | 需要复用通用基础设施的中大型模组 |
| Humanoid Alien Races | `Erdelf.HumanoidAlienRaces` | 自定义异星种族框架，扩展 Pawn 种族系统 | 添加新可游玩种族的模组 |
| Vanilla Expanded Framework | `OskarPotocki.VanillaExpandedFramework` | 原版扩展系列通用功能库：Tab、Comp、渲染、工具类 | 原版扩展系列及大量衍生内容模组 |
| Vehicle Framework | `SmashPhil.Vehicles` | 载具系统：可驾驶载具、路径、装载 | 添加载具的模组 |
| Facial Animation | `Nals.FacialAnimation` | 面部表情动画系统，增强 Pawn 表现 | 需要更丰富面部动画的模组 |
| Adaptive Storage Framework | `Soul.AdaptiveStorageFramework` | 自适应存储框架，高性能可扩展存储 | 存储类、容器类内容模组 |
| Processor Framework | `Syrchalis.ProcessorFramework` | 加工处理框架：物品随时间加工转化 | 酿造、发酵、加工类模组 |
| Custom Quest Framework | 见工坊页面 | 自定义任务/事件脚本框架，扩展任务生成 | 添加自定义任务线的模组 |
| Ancot Library | `Ancot.AncotLibrary` | 社区通用工具库：实用功能与优化 | 依赖该库的中文社区内容模组 |
| Ariandel Library | `Ariandel.AriandelLibrary` | 社区通用工具库：Def/Comp 扩展工具 | 依赖该库的中文社区内容模组 |
| Oberonia Aurea Framework | 见工坊页面 | 社区系列模组通用功能底座 | 同系列内容模组的前置 |

> 注意：packageId 须与各框架最新发布版本一致。部分社区框架的 packageId 以其创意工坊页面前置声明为准，引用前请核对目标框架 `About.xml` 中的 `packageId` 字段，避免因版本更新或维护交接导致标识变化。

### 引用方式示例

在 `About.xml` 中声明框架前置：

```xml
<modDependencies>
  <li>
    <packageId>brrainz.harmony</packageId>
    <displayName>Harmony</displayName>
    <steamWorkshopUrl>steam://url/CommunityFilePage/2009463077</steamWorkshopUrl>
  </li>
  <li>
    <packageId>OskarPotocki.VanillaExpandedFramework</packageId>
    <displayName>Vanilla Expanded Framework</displayName>
  </li>
</modDependencies>

<loadAfter>
  <li>brrainz.harmony</li>
  <li>OskarPotocki.VanillaExpandedFramework</li>
</loadAfter>
```

---

## 二、NuGet 包表

开发 C# 模组时通过 NuGet 引用的工具包。

| NuGet 包 | 用途 | 用法 |
|----------|------|------|
| `Krafs.Rimworld.Ref` | 提供游戏引用程序集（Assembly-CSharp、UnityEngine.*），按版本匹配 | `<PackageReference Include="Krafs.Rimworld.Ref" Version="1.6.*" />` |
| `Lib.Harmony` | Harmony 完整运行时（含实现），适合需要自带 Harmony DLL 的场景 | `<PackageReference Include="Lib.Harmony" Version="2.*" />` |
| `Lib.Harmony.Thin` | Harmony 瘦包（仅接口/特性），运行依赖游戏内已加载的 Harmony，不重复携带 DLL | `<PackageReference Include="Lib.Harmony.Thin" Version="2.*" />` |
| `Krafs.Publicizer` | 编译期公开游戏 internal/private 成员，免反射访问 | `<PackageReference Include="Krafs.Publicizer" Version="2.*" PrivateAssets="all" />` + `<Publicize Include="Assembly-CSharp" />` |
| `Zetrith.Prepatcher` | 编译期预生成成员访问器（生成源码式访问私有字段），与 Publicizer 思路不同，运行时零开销 | `<PackageReference Include="Zetrith.Prepatcher" Version="*" />` + `<RimworldPrepatcher Publicize="Assembly-CSharp" />` |

> 选用建议：引用游戏程序集用 `Krafs.Rimworld.Ref`；Harmony 优先 `Lib.Harmony.Thin`（避免重复 DLL）；访问私有成员二选一 `Krafs.Publicizer`（编译期改可见性）或 `Zetrith.Prepatcher`（生成访问器源码）。

---

## 三、DLC packageId 表

在 `About.xml` 中声明 DLC 依赖、或在 Patch/LoadFolders 中检测 DLC 时使用的 packageId。

| DLC | packageId | 说明 |
|-----|-----------|------|
| 核心（Core） | `Ludeon.RimWorld` | 游戏本体，无需声明依赖 |
| 皇权（Royalty） | `Ludeon.RimWorld.Royalty` | 贵族、心灵能力、帝国 |
| 文化（Ideology） | `Ludeon.RimWorld.Ideology` | 信仰体系、意识形态 |
| 生物科技（Biotech） | `Ludeon.RimWorld.Biotech` | 基因、机械体、育儿 |
| 异常（Anomaly） | `Ludeon.RimWorld.Anomaly` | 异常实体、研究、威胁 |
| 远征（Odyssey） | `Ludeon.RimWorld.Odyssey` | 太空远征内容 |

### DLC 检测方式

**XML 层（Patch / LoadFolders / MayRequire）：**

```xml
<!-- 条件 Patch：仅当 Royalty 启用 -->
<Operation Class="PatchOperationFindMod">
  <mods>
    <li>Ludeon.RimWorld.Royalty</li>
  </mods>
  <match><!-- ... --></match>
</Operation>

<!-- LoadFolders 条件加载 -->
<li IfModActive="Ludeon.RimWorld.Biotech">/Biotech/</li>

<!-- Def 字段级条件 -->
<someField MayRequire="Ludeon.RimWorld.Anomaly" />
```

**C# 层：**

```csharp
// 运行时检测 DLC
bool royalty   = ModsConfig.RoyaltyActive;
bool ideology  = ModsConfig.IdeologyActive;
bool biotech   = ModsConfig.BiotechActive;
bool anomaly   = ModsConfig.AnomalyActive;
bool odyssey   = ModsConfig.OdysseyActive;
```

> DLC 的 `packageId` 永久固定，可在 `modDependencies` 中声明以提示玩家缺少 DLC，也可在 `loadAfter` 中确保加载顺序。
