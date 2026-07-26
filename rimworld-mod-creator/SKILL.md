---
name: "rimworld-mod-creator"
description: "Create RimWorld mods (XML Defs, C# Harmony patches, Patches, translations). Invoke when user wants to make/create/build a RimWorld mod, add content/feature to a mod, or asks about RimWorld mod structure/API/frameworks."
---

# RimWorld Mod Creator

一个通用的 RimWorld 模组制作 AI Skill，可接入任意支持 Skill 的 AI 助手。基于对 234 个 Steam 创意工坊模组的逆向工程分析，把真实模组中归纳出的目录约定、代码范式和常见坑点固化为一套可按需加载的知识体系，替代散落在 wiki 与论坛中的零散经验。

支持四种主流模组类型的完整制作链路：纯 XML 内容模组、C# 功能模组（Harmony Patch）、兼容补丁、汉化模组。所有代码示例取自真实模组（BionicIcons、Gunplay、yayoAni、Pick Up And Haul、EqualMilking、Ratkin）并经过验证，遵循工坊主流写法。

## 适用场景

当用户需要以下任一操作时，立即加载本 Skill：
- 制作/创建/新建一个 RimWorld 模组
- 为 RimWorld 模组添加内容（物品/建筑/武器/种族等）
- 为 RimWorld 模组编写 C# Harmony patch
- 制作兼容补丁或汉化模组
- 询问 RimWorld 模组结构、API、框架使用方法
- 询问 About.xml / LoadFolders.xml / Defs / Patches / 翻译的写法

## 工作流程

### 1. 明确模组类型

首先判断用户要制作的模组类型，不同类型的文件需求不同：

| 类型 | 必需文件 | 典型场景 |
|------|---------|---------|
| 纯 XML 内容 | `About/About.xml` + `Defs/*.xml` + `Textures/` | 新增物品/建筑/武器/研究 |
| C# 功能 | 上述 + `Source/*.cs` + `版本/Assemblies/*.dll` | 修改游戏行为、自定义逻辑 |
| 兼容补丁 | `About/About.xml` + `Patches/*.xml` | 让两个 mod 兼容 |
| 汉化 | `About/About.xml` + `Languages/` | 翻译现有 mod |

### 2. 创建模组骨架

根据模组类型，参照 `references/` 目录下的模板创建文件结构。标准目录布局：

```
ModName/
├── About/About.xml              # 必需：模组元数据
├── LoadFolders.xml              # 可选：版本化加载配置
├── Defs/*.xml                   # XML 内容定义
├── Patches/*.xml                # PatchOperation 补丁
├── Textures/                    # 贴图（PNG，texPath 引用）
├── Sounds/                      # 音效（.ogg/.wav）
├── Assemblies/*.dll             # C# 编译产物
├── Source/*.cs + *.csproj       # C# 源码
├── Languages/<语言>/            # 翻译
└── 1.6/                         # 版本子目录（配合 LoadFolders.xml）
```

### 3. 加载参考文档

根据制作内容，加载 `references/` 下对应的详细参考：

- **`references/about-and-loadfolders.md`**：About.xml 全字段 + LoadFolders.xml 条件加载机制
- **`references/csharp-development.md`**：C# 项目配置、Mod 入口、Harmony Patch、ModSettings、自定义 Def/Comp、跨版本兼容
- **`references/xml-defs.md`**：16 种常见 Def 类型、继承机制、引用关系、示例
- **`references/patches.md`**：8 种 PatchOperation、XPath 用法、兼容补丁范式
- **`references/translation-and-assets.md`**：翻译系统（Keyed/DefInjected）、贴图路径规则、音效
- **`references/frameworks.md`**：12 个框架库清单 + NuGet 包 + DLC packageId

### 4. 关键约定速查

制作时务必遵守以下约定（违反会导致模组无法加载或崩溃）：

**About.xml 必需字段**：
- `packageId`：全小写，格式 `作者.模组名`，全局唯一
- `supportedVersions`：如 `<li>1.6</li>`
- `modDependencies`：C# 模组必须声明 `brrainz.harmony`
- `loadAfter`：通常先列 harmony，再列 DLC，再列框架 mod

**贴图路径**：`<texPath>` 值相对于 `Textures/` 文件夹，不带扩展名（自动补 `.png`），用正斜杠 `/`。

**Def 继承**：`ParentName="父Name"` 继承，`Abstract="True"` 标记模板，abstract 基类需 `Name` 属性但不需要 `defName`。

**Harmony Patch**：`[HarmonyPatch(typeof(Class), "Method")]` 注解；`Prefix` 返回 `false` 跳过原方法；`___字段名` 访问私有字段；`__result` 修改返回值。

**C# 项目**：推荐 SDK-style csproj + `Krafs.Rimworld.Ref`（NuGet）+ `Lib.Harmony`（ExcludeAssets="runtime"）；OutputPath 设为 `..\1.6\Assemblies\`。

**翻译**：DefInjected 子目录名必须与 Def 类型名完全一致；键名格式 `DefDefName.字段名`。

### 5. 验证与输出

- 确认所有 `texPath` 引用的贴图文件存在
- 确认 Def 间的引用（`<li>DefName</li>`）指向已定义的 Def
- C# 模组确认编译输出到版本子目录的 `Assemblies/`
- 多版本支持时创建 `LoadFolders.xml` + 版本子目录

## 注意事项

- 所有代码示例来自真实模组（BionicIcons、Gunplay、yayoAni、Pick Up And Haul、EqualMilking、Ratkin），已在工坊目录验证
- DLC packageId：`Ludeon.RimWorld.Royalty/Ideology/Biotech/Anomaly/Odyssey`，核心是 `Ludeon.RimWorld`
- 约 70% 的 C# 模组依赖 Harmony（`brrainz.harmony`），是 C# 模组基石
- 参考 HTML 完整指南位于：`rimworld-mod-guide/rimworld-mod-guide.html`
