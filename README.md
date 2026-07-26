# RimWorld Mod Creator

![version](https://img.shields.io/badge/version-v0.1-f78166) ![license](https://img.shields.io/badge/license-MIT-3fb950) ![rimworld](https://img.shields.io/badge/RimWorld-1.4%E2%80%931.6-79c0ff)

> 一个通用的 RimWorld 模组制作 AI Skill。基于对 234 个 Steam 创意工坊模组的逆向工程分析，把散落在 wiki、论坛帖子和他人源码里的模组制作经验，提炼成一套结构化、可按需加载的知识体系，可接入任意支持 Skill 的 AI 助手。

**下载 v0.1**：[rimworld-mod-creator-v0.1.zip](./docs/rimworld-mod-creator-v0.1.zip) · **在线页面**：[GitHub Pages](https://luo2f.github.io/Rimworld-mod-skill-alpha-/)

RimWorld 的模组生态庞大，但官方文档有限，真正可靠的制作知识往往要靠阅读别人的源码和反复试错才能积累。这个技能把从 234 个真实工坊模组中统计出的约定、范式和坑点固化下来，让 AI 助手在用户提出模组制作需求时，能直接调取对应领域的准确参考，而不是凭模糊记忆拼凑。

## 核心能力

技能覆盖四种主流模组类型的完整制作链路，从模组骨架搭建到引用校验：

| 模组类型 | 必需文件 | 典型场景 |
|----------|---------|---------|
| 纯 XML 内容 | `About/About.xml` + `Defs/*.xml` + `Textures/` | 新增物品、建筑、武器、研究项目 |
| C# 功能 | 上述 + `Source/*.cs` + `版本/Assemblies/*.dll` | 修改游戏行为、自定义逻辑 |
| 兼容补丁 | `About/About.xml` + `Patches/*.xml` | 让两个 mod 协同工作 |
| 汉化 | `About/About.xml` + `Languages/` | 翻译现有 mod |

每一种类型都配套对应的目录约定与验证要点。例如 C# 模组必须声明 `brrainz.harmony` 依赖、编译输出落到版本子目录的 `Assemblies/`；汉化包则用 `loadAfter` 指向原 mod、`DefInjected` 子目录名必须与 Def 类型名完全一致。这些从真实模组中归纳出的约束，能避免模组无法加载或运行时崩溃。

## 知识来源

技能中的每一条约定和代码示例都不是凭空编写，而是来自对 234 个工坊模组的实际拆解。统计结果让技能知道什么才是"主流做法"，而非个例。

```mermaid
pie title 234 个模组的类型分布
    "C# 代码模组" : 142
    "纯 XML 内容模组" : 37
    "汉化/翻译模组" : 37
    "库/框架模组" : 12
    "纯资源模组" : 6
```

几个有代表性的发现：约 60.7% 的模组含 C# 代码，是功能实现的主力；约 70% 的 C# 模组依赖 Harmony（`brrainz.harmony`），它事实上是 C# 模组的基石；`loadAfter` 是最常见的加载顺序声明，出现在 74% 的模组中。技能内的框架清单、DLC packageId、高频被依赖项，都源自这些统计。

## 参考文档

技能在运行时按需加载 `references/` 下的六份深度参考，每份聚焦一个领域：

| 文档 | 覆盖内容 |
|------|---------|
| `about-and-loadfolders.md` | `About.xml` 全字段说明、字段使用统计、`LoadFolders.xml` 条件加载机制与四种写法 |
| `xml-defs.md` | 16 种常见 Def 类型、`ParentName` 继承机制、引用关系、代表性 Def 示例 |
| `csharp-development.md` | SDK-style 项目配置、三种 Mod 入口模式、Harmony Patch 写法、ModSettings、自定义 Def/ThingComp、跨版本兼容 |
| `patches.md` | 8 种 PatchOperation、XPath 用法、兼容补丁标准范式 |
| `translation-and-assets.md` | Keyed/DefInjected 翻译系统、贴图路径规则、`graphicClass` 渲染类型、音效 |
| `frameworks.md` | 12 个框架库清单、NuGet 包、DLC packageId、模组类型分布统计 |

所有代码示例取自真实模组（BionicIcons、Gunplay、yayoAni、Pick Up And Haul、EqualMilking、Ratkin），已在工坊目录中验证过写法的正确性。

## 工作流程

技能在接到模组制作需求后，按以下流程推进，根据模组类型动态决定加载哪份参考：

```mermaid
flowchart TD
    A["明确模组类型"] --> B["创建模组骨架"]
    B --> C{"需要哪类参考？"}
    C -->|"XML 内容"| D1["xml-defs.md"]
    C -->|"C# 功能"| D2["csharp-development.md"]
    C -->|"兼容补丁"| D3["patches.md"]
    C -->|"汉化 / 资源"| D4["translation-and-assets.md"]
    C -->|"元数据 / 加载"| D5["about-and-loadfolders.md"]
    C -->|"框架依赖"| D6["frameworks.md"]
    D1 & D2 & D3 & D4 & D5 & D6 --> E["验证引用与输出"]
```

验证环节会检查 `texPath` 引用的贴图是否存在、Def 间的 `<li>` 引用是否指向已定义的 Def、C# 编译产物是否落到正确目录，避免最常见的加载失败。

## 仓库结构

```
.
├── rimworld-mod-creator/        # 技能本体
│   ├── SKILL.md                 # 技能入口与工作流程定义
│   └── references/              # 六份按需加载的深度参考
│       ├── about-and-loadfolders.md
│       ├── csharp-development.md
│       ├── frameworks.md
│       ├── patches.md
│       ├── translation-and-assets.md
│       └── xml-defs.md
├── docs/                        # GitHub Pages 站点
│   ├── index.html               # 在线落地页
│   ├── rimworld-mod-creator-v0.1.zip  # v0.1 下载包
│   └── .nojekyll
├── .github/workflows/           # CI：Pages 自动部署
│   └── deploy-pages.yml
└── rimworld-mod-guide/          # 完整 HTML 指南（离线可读）
    └── rimworld-mod-guide.html.zip
```

`SKILL.md` 是技能的入口文件，定义了适用场景、工作流程和关键约定速查；`references/` 下的文档由技能在运行时按需加载，普通用户无需手动查阅。`docs/` 托管在线落地页与下载包，推送后由 GitHub Actions 自动部署到 Pages。`rimworld-mod-guide` 是同一套知识的独立 HTML 合集，解压后可在浏览器中通读。

## 下载与部署

当前版本为 **v0.1**。两种获取方式：

- **直接下载**：[rimworld-mod-creator-v0.1.zip](./docs/rimworld-mod-creator-v0.1.zip)，解压即得到 `SKILL.md` 与 6 份参考文档
- **在线页面**：[GitHub Pages 落地页](https://luo2f.github.io/Rimworld-mod-skill-alpha-/)，可在浏览器中预览技能概览

下载包内容与仓库 `rimworld-mod-creator/` 目录一致，可直接放入支持 Skill 的 AI 助手的加载路径使用。

GitHub Pages 通过 `.github/workflows/deploy-pages.yml` 自动部署：每当 `docs/` 目录有更新推送到主分支，Action 会重新构建并发布站点。首次使用需在仓库 Settings → Pages 中将 Source 设为 `GitHub Actions`。

## 如何使用

本技能是通用 AI Skill，可接入任意支持 Skill 的 AI 助手。将 `rimworld-mod-creator` 目录放入对应助手的知识/技能加载路径后，技能会被自动识别。之后用自然语言描述需求即可触发，无需记忆任何命令。

触发示例：

- "帮我做一个 RimWorld 模组，新增一把中世纪弩"
- "给这个 mod 写一个 Harmony patch，修改投射物速度"
- "做一个 Ratkin 和 Facial Animation 的兼容补丁"
- "把这个 mod 汉化成简体中文"
- "About.xml 里 loadAfter 应该怎么写"

技能会先判断模组类型，再加载对应参考，最终产出符合工坊规范的文件结构与代码。

## 一段最小示例

新增一把中世纪弩，只需一个 `ThingDef`，继承自原版或自定义基类：

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

`texPath` 相对 `Textures/` 目录、不带扩展名、用正斜杠分隔；`defaultProjectile` 指向另定义的投射物 Def。这类约定在 `xml-defs.md` 与 `translation-and-assets.md` 中有完整说明。

## 适用版本与 DLC

技能覆盖 RimWorld 1.4 / 1.5 / 1.6，并适配全部 DLC。DLC 的 `packageId` 一览：

| DLC | packageId |
|-----|-----------|
| 核心 | `Ludeon.RimWorld` |
| 皇权 Royalty | `Ludeon.RimWorld.Royalty` |
| 文化 Ideology | `Ludeon.RimWorld.Ideology` |
| 生物科技 Biotech | `Ludeon.RimWorld.Biotech` |
| 异常 Anomaly | `Ludeon.RimWorld.Anomaly` |
| 奥德赛 Odyssey | `Ludeon.RimWorld.Odyssey` |

多版本支持通过 `LoadFolders.xml` 配合版本子目录实现，具体写法见 `about-and-loadfolders.md`。

## 许可证

[MIT License](./LICENSE) © 2026 luo2f
