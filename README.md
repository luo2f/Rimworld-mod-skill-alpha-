# RimWorld Mod Creator

![version](https://img.shields.io/badge/version-v0.2-f78166) ![license](https://img.shields.io/badge/license-MIT-3fb950) ![rimworld](https://img.shields.io/badge/RimWorld-1.6-79c0ff)

> 一个通用的 RimWorld 模组制作 AI Skill，覆盖从环境搭建、XML Def、C# Harmony 到 Steam Workshop 发布的全流程。内置 8 个代码模板、7 套工作流和错误自学习机制，可接入任意支持 Skill 的 AI 助手。

**下载 v0.2**：[rimworld-mod-creator-v0.2.zip](./docs/rimworld-mod-creator-v0.2.zip) · **v0.1 归档**：[rimworld-mod-creator-v0.1.zip](./docs/rimworld-mod-creator-v0.1.zip) · **在线页面**：[GitHub Pages](https://luo2f.github.io/Rimworld-mod-skill-alpha-/)

---

## v0.2 有什么新变化

v0.2 从"纯知识参考"升级为"模板驱动 + 工作流编排 + 错误自学习"的完整制作体系：

| 维度 | v0.1 | v0.2 |
|------|------|------|
| 参考文档 | 6 份 | **11 份**（新增环境搭建、调试排错、Workshop 发布、API 速查） |
| 代码模板 | 无 | **8 个**（武器/服装/建筑/材料/配方/Harmony/ThingComp/需求清单） |
| 工作流 | 无 | **7 套**（新建/添加/修补/调试/正规化/批量） |
| 错误学习 | 无 | **自学习**（`learnings/errors.txt`，每次加载自动读取） |
| 决策机制 | 按类型加载参考 | **三层决策**（有模板→直接用 / 报错→查原版 / 无模板→查+存模板） |
| 制作流程 | 一步到位 | **测试版先行**（Test → Verify → Formalize） |
| 目标版本 | 1.4–1.6 | **聚焦 1.6**（Unity 2022.3.35 / .NET 4.7.2+） |

## 核心架构：三层决策

技能每次接收模组制作请求时，按优先级判断走哪条路径，避免不必要的原版源码查阅：

```
用户请求
  │
  ├─ ① 有模板？ ─── 武器/服装/建筑/资源/配方/Harmony/ThingComp
  │     └─ 直接用模板生成（模板已验证过原版结构）
  │
  ├─ ② 报错/调试？ ─── 红字/白窗/崩溃/NullReferenceException
  │     └─ 查原版源码搜索错误原因（grep 或 dnSpy）
  │
  └─ ③ 无模板的新类型？ ─── 植物/生物/派系/事件/地形/Hediff/研究...
        └─ 查原版 Def 结构 → 模仿写出 → 存储为新模板
```

模板已验证过原版字段名、枚举值和 ParentName 继承链，直接替换 `<Your...>` 占位符即可生成可用代码。遇到模板未覆盖的新类型，技能会查原版 Def 写出后**自动存为新模板**，下次同类请求直接免查。

## 代码模板

8 个模板覆盖最高频的模组制作需求，每个模板都带完整中文注释和原版参考路径：

| 模板 | 文件 | 说明 |
|------|------|------|
| 近战武器 | `templates/weapon-melee.xml` | 继承 `BaseMeleeWeapon`，含 tools/costList |
| 远程武器 | `templates/weapon-ranged.xml` | 继承 `BaseHumanMakeableGun`，含 verbs/projectile |
| 服装/护甲 | `templates/apparel.xml` | 继承 `ApparelBase`，含 layers/statBases |
| 原材料 | `templates/resource-stuff.xml` | `stuffProps` 完整配置 |
| 建筑/工作台 | `templates/building.xml` | 含 comps（Power/Flickable 等） |
| 制作配方 | `templates/recipe.xml` | ingredients/products/skillRequirements |
| Harmony 补丁 | `templates/harmony-patch.cs` | Prefix/Postfix 骨架 + 注释 |
| C# ThingComp | `templates/thingcomp.cs` | Comp + CompProperties 配对骨架 |

## 工作流

7 套工作流覆盖模组制作全生命周期，遵循"测试版先行"原则：

| 工作流 | 文件 | 触发场景 |
|--------|------|---------|
| 新建 Mod | `workflows/new-mod.md` | 从零创建项目（测试版先行） |
| 添加物品 | `workflows/add-item.md` | 往已有 mod 加武器/服装/材料 |
| 添加建筑 | `workflows/add-building.md` | 加建筑/工作台 |
| 修改原版 | `workflows/patch-vanilla.md` | XML Patch 或 Harmony 改原版行为 |
| 崩溃排查 | `workflows/debug-crash.md` | 红字/白窗/崩溃定位 |
| 正规化 | `workflows/formalize-mod.md` | 测试通过后转正式版 |
| 批量处理 | `workflows/batch-process.md` | 一次处理多个需求清单 |

测试版先行流程：先生成可进游戏测试的测试版（允许 `test.` 前缀、引用原版贴图），开发者在游戏中验证无 bug 后再正规化（正式 packageId、原创贴图、Preview.png、多语言）。

## 错误自学习

`learnings/errors.txt` 记录历史错误教训。技能每次加载时自动读取，避免重复犯错；每次排查完错误并修复后，自动追加一条记录：

```
2026-07-28 | XML | ParentName 写错导致 Def 加载失败，需对照原版 BaseWeapons.xml
2026-07-28 | C#  | Harmony patch ID 重复导致补丁冲突，需用唯一前缀
```

## 参考文档

11 份按编号排序的深度参考，覆盖制作全链路：

| 文档 | 覆盖内容 |
|------|---------|
| `01-environment.md` | 环境搭建（RimWorld 1.6 / Unity 2022.3.35 / .NET 4.7.2+） |
| `02-project-structure.md` | Mod 项目结构、About.xml、LoadFolders.xml |
| `03-xml-defs.md` | 16 种 Def 类型、ParentName 继承、引用关系 |
| `04-xml-patching.md` | 8 种 PatchOperation、XPath、兼容补丁范式 |
| `05-csharp-basics.md` | C# 项目配置、Mod 入口、ModSettings、ThingComp、序列化 |
| `06-harmony.md` | Prefix/Postfix/Transpiler、特殊参数、条件 Patch |
| `07-assets.md` | 贴图路径、着色器/Mask、音效、翻译系统 |
| `08-debugging.md` | 红字排查、Player.log 解读、常见崩溃定位 |
| `09-workshop.md` | Steam Workshop 发布流程、预览图、描述规范 |
| `10-api-reference.md` | RimWorld API 速查表 |
| `frameworks.md` | 12 个框架库清单、NuGet 包、DLC packageId |

## 核心原则

技能在生成任何模组时遵守以下约定：

- **命名规范**：所有 defName 和 C# 类名使用用户自选的唯一前缀，AI 不预设作者信息
- **安全实践**：Harmony 用唯一 patch ID、`[StaticConstructorOnStartup]` 初始化、优先用 PatchOperations 而非直接改原版
- **法律边界**：禁止复制原版 C# 源码/DLL、禁止使用第三方 IP（宝可梦、星战等）；允许模仿 Def 结构、反编译查看 API 签名
- **AI 标注**：生成内容在 About.xml 描述末尾、Workshop 页面、C# 文件头标注 `[AI 辅助生成]`

## 仓库结构

```
.
├── rimworld-mod-creator/              # 技能本体
│   ├── SKILL.md                       # 入口：三层决策 + 快速导航 + 核心原则
│   ├── references/                    # 11 份深度参考（按编号排序）
│   │   ├── 01-environment.md
│   │   ├── 02-project-structure.md
│   │   ├── 03-xml-defs.md
│   │   ├── 04-xml-patching.md
│   │   ├── 05-csharp-basics.md
│   │   ├── 06-harmony.md
│   │   ├── 07-assets.md
│   │   ├── 08-debugging.md
│   │   ├── 09-workshop.md
│   │   ├── 10-api-reference.md
│   │   └── frameworks.md
│   ├── templates/                     # 8 个代码模板（已验证原版结构）
│   │   ├── weapon-melee.xml
│   │   ├── weapon-ranged.xml
│   │   ├── apparel.xml
│   │   ├── resource-stuff.xml
│   │   ├── building.xml
│   │   ├── recipe.xml
│   │   ├── harmony-patch.cs
│   │   ├── thingcomp.cs
│   │   └── requirements-template.md
│   ├── workflows/                     # 7 套工作流
│   │   ├── new-mod.md
│   │   ├── add-item.md
│   │   ├── add-building.md
│   │   ├── patch-vanilla.md
│   │   ├── debug-crash.md
│   │   ├── formalize-mod.md
│   │   └── batch-process.md
│   └── learnings/
│       └── errors.txt                 # 错误自学习记录
├── docs/                              # GitHub Pages 站点
│   ├── index.html                     # 在线落地页
│   ├── rimworld-mod-creator-v0.2.zip  # v0.2 下载包（当前）
│   ├── rimworld-mod-creator-v0.1.zip  # v0.1 下载包（归档）
│   └── .nojekyll
├── .github/workflows/                 # CI：Pages 自动部署
│   └── deploy-pages.yml
└── rimworld-mod-guide/                # 完整 HTML 指南（离线可读）
    └── rimworld-mod-guide.html.zip
```

## 下载与部署

| 版本 | 下载 | 说明 |
|------|------|------|
| **v0.2（当前）** | [rimworld-mod-creator-v0.2.zip](./docs/rimworld-mod-creator-v0.2.zip) | 11 份参考 + 8 个模板 + 7 套工作流 + 错误学习 |
| v0.1（归档） | [rimworld-mod-creator-v0.1.zip](./docs/rimworld-mod-creator-v0.1.zip) | 6 份参考，早期版本 |

- **在线页面**：[GitHub Pages 落地页](https://luo2f.github.io/Rimworld-mod-skill-alpha-/)
- 下载包解压即得完整 `rimworld-mod-creator/` 目录，放入支持 Skill 的 AI 助手加载路径即可使用
- GitHub Pages 通过 `.github/workflows/deploy-pages.yml` 自动部署，首次需在 Settings → Pages 将 Source 设为 `GitHub Actions`

## 如何使用

本技能是通用 AI Skill，可接入任意支持 Skill 的 AI 助手。将 `rimworld-mod-creator` 目录放入对应助手的知识/技能加载路径后，技能会被自动识别。之后用自然语言描述需求即可触发：

- "帮我做一个 RimWorld 模组，新增一把突击步枪"
- "给这个 mod 写一个 Harmony patch，修改投射物速度"
- "游戏里报红字了，帮我排查"
- "测试通过了，帮我正规化这个 mod"
- "批量生成 5 把武器，需求清单在这个文件里"

技能会先判断走三层决策的哪条路径，加载对应模板或参考，生成测试版供你进游戏验证，确认无误后正规化输出。

## 源码查阅工具

技能内置两种原版源码查阅方式，按优先级使用：

- **grep**（第一选择）：搜索原版 `Data/Core/Defs/` 目录，查字段用法、ParentName 定义、枚举取值
- **dnSpy**（第二选择）：反编译 `Assembly-CSharp.dll`，查 C# 类和方法签名

有模板的类型直接用模板，无需查阅任何源码。

## 适用版本

目标版本 **RimWorld 1.6**（Unity 2022.3.35 / .NET Framework 4.7.2+）。DLC packageId 一览：

| DLC | packageId |
|-----|-----------|
| 核心 | `Ludeon.RimWorld` |
| 皇权 Royalty | `Ludeon.RimWorld.Royalty` |
| 文化 Ideology | `Ludeon.RimWorld.Ideology` |
| 生物科技 Biotech | `Ludeon.RimWorld.Biotech` |
| 异常 Anomaly | `Ludeon.RimWorld.Anomaly` |
| 奥德赛 Odyssey | `Ludeon.RimWorld.Odyssey` |

## 许可证

[MIT License](./LICENSE) © 2026 luo2f
