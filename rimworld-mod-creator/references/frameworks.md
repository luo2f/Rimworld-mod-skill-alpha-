# 库与框架参考

## 框架库清单（12 个）

以下框架/库模组处于依赖链底层，被大量模组依赖。制作依赖这些框架的模组时，需在 About.xml 声明 `modDependencies` 和 `loadAfter`。

| 框架 | packageId | 功能 | 典型被依赖场景 |
|------|-----------|------|---------------|
| **Harmony** | `brrainz.harmony` | 运行时方法 patch 库，C# 模组基石 | 约 70% 的 C# 模组依赖；几乎所有 patch 都基于此 |
| **HugsLib** | `UnlimitedHugs.HugsLib` | 通用工具库，提供设置、日志、热重载等基础设施 | 许多老模组的基础依赖；含 Publisher+ 发布工具 |
| **Humanoid Alien Races (HAR)** | `erdelf.HumanoidAlienRaces` | 自定义种族框架 | Ratkin、MoeLotl、Miho、Rabbie 等所有种族模组 |
| **Vanilla Expanded Framework (VEF)** | `OskarPotocki.VanillaFactionsExpanded.Core` | Vanilla Expanded 系列的公共框架 | 所有 VE 系列 mod、Vehicles Expanded、Outposts |
| **Vehicle Framework** | `SmashPhil.VehicleFramework` | 载具系统框架 | Vanilla Vehicles Expanded 等 |
| **Facial Animation** | `Nals.FacialAnimation` | 面部动画框架 | 各种族的面部动画补丁均依赖此框架 |
| **Adaptive Storage Framework** | `adaptive.storage.framework` | 存储系统框架 | Neat Storage、Fridge 等 |
| **Processor Framework** | `syrchalis.processor.framework` | 处理器框架 | Rabbie 种族等 |
| **Custom Quest Framework** | `HaiLuan.CustomQuestFramework` | 自定义任务框架 | 自定义任务类模组 |
| **Ancot Library** | `Ancot.AncotLibrary` | Ancot 系列公共库 | Kiiro Race、Milira 相关 |
| **Ariandel Library** | `Ariandel.AriandelLibrary` | Ariandel 系列公共库 | Milira Imperium |
| **Oberonia Aurea Framework** | `OARK.OberoniaAurea.Framework` | OA 系列公共框架 | OA 系列扩展 |

## NuGet 包（C# 开发用）

| NuGet 包 | 用途 | 典型用法 |
|----------|------|---------|
| `Krafs.Rimworld.Ref` | RimWorld API 引用（替代本地 DLL） | `<PackageReference Version="1.6.*-*"/>` |
| `Lib.Harmony` | Harmony 库引用（依赖合并的单文件版，推荐） | `ExcludeAssets="runtime"`（不打包运行时） |
| `Lib.Harmony.Thin` | Harmony 库引用（不含依赖，需自行提供运行时依赖） | 需自行保证 `0Harmony.dll` 在运行时可用 |
| `Krafs.Publicizer` | 将 private/internal 成员公开为 public（MSBuild 插件） | `<Publicize Include="Assembly-CSharp"/>`，须配 `<PrivateAssets>all</PrivateAssets>` |
| `Zetrith.Prepatcher` | 预 patch 工具（编译期生成字段访问） | yayoAni 等使用 |

## DLC packageId

声明 DLC 依赖时使用：

| DLC | packageId |
|-----|-----------|
| 核心 | `Ludeon.RimWorld` |
| 皇权 Royalty | `Ludeon.RimWorld.Royalty` |
| 文化 Ideology | `Ludeon.RimWorld.Ideology` |
| 生物科技 Biotech | `Ludeon.RimWorld.Biotech` |
| 异常 Anomaly | `Ludeon.RimWorld.Anomaly` |
| 奥德赛 Odyssey | `Ludeon.RimWorld.Odyssey` |

## 模组类型分布（234 个模组统计）

| 类型 | 数量 | 占比 | 特征 |
|------|------|------|------|
| C# 代码模组 | 142 | 60.7% | 含编译 dll 或源码，功能实现主力 |
| 纯 XML 内容模组 | 37 | 15.8% | 仅用 Defs/Patches 定义内容，无程序集 |
| 汉化/翻译模组 | 37 | 15.8% | 仅含 Languages 目录，依赖原 mod |
| 库/框架模组 | 12 | 5.1% | 提供公共 API/Harmony 基础设施 |
| 纯资源模组 | 6 | 2.6% | 只有 Textures，无 Defs 无代码 |

## 常见高频被依赖项

- `brrainz.harmony`：被约 70% 的模组依赖
- `HugsLib`、`HAR`、`VEF`、`FacialAnimation`、`RatkinRaceMod` 是高频被依赖项
- IfModActive 最常指向的目标：Biotech(27)、Ideology(27)、Royalty(13)、Odyssey(9)、Anomaly(8)、CombatExtended(16)、RatkinRaceMod(12)、FacialAnimation(12)
