# 02 - Mod 项目结构

> **适用版本**: RimWorld 1.6 | **最后更新**: 2026-07-28

本文档介绍 RimWorld 模组的标准目录结构、`About.xml` 字段详解、命名规范与 `LoadFolders.xml` 条件加载机制。

---

## 一、标准目录树

一个功能完整的模组典型目录结构如下：

```
<YourMod>/
├── About/                        # 模组元信息（必需）
│   ├── About.xml                 # 元信息主文件（必需）
│   ├── Preview.png               # 工坊预览图 640x360（发布必需）
│   └── PublishedFileId.txt       # 工坊 ID（上传后自动生成）
├── Defs/                         # Def 定义（数据）
│   ├── Things/
│   ├── Recipes/
│   └── ...
├── Patches/                      # XML PatchOperations（修改其他模组/原版 Def）
├── Assemblies/                   # 编译后的 C# DLL
│   └── <YourPrefix>.dll
├── Textures/                     # 贴图资源（PNG）
│   ├── Things/
│   ├── Buildings/
│   └── UI/
├── Sounds/                       # 音效资源（WAV/OGG）
├── Languages/                    # 翻译
│   └── English/
│       ├── Keyed/
│       └── DefInjected/
├── 1.6/                          # 版本专属内容（条件加载）
│   ├── Defs/
│   ├── Patches/
│   └── Assemblies/
├── Source/                       # C# 源码（可选，供他人学习）
├── LoadFolders.xml               # 条件加载配置（可选）
└── ModSync.xml                   # RimPy 同步信息（可选）
```

> 说明：`About/`、`Defs/` 等顶层目录在所有版本通用；版本专属内容放入 `1.6/`、`1.5/` 等以版本号命名的子目录，配合 `LoadFolders.xml` 实现条件加载。

---

## 二、About.xml 完整字段详解

`About/About.xml` 是模组的身份证，定义了模组的基本信息、依赖与加载顺序。以下为完整字段（带注释）：

```xml
<?xml version="1.0" encoding="utf-8"?>
<ModMetaData>

  <!-- 唯一标识符，格式 推荐 作者名.模组名，全小写，禁止空格。一旦发布不可更改 -->
  <packageId><YourPrefix>.yourmodname</packageId>

  <!-- 显示名称（支持翻译键，形如 <YourPrefix>_ModName>） -->
  <name>Your Mod Name</name>

  <!-- 作者，多个作者用逗号分隔 -->
  <author>YourName</author>

  <!-- 模组简介描述，显示在模组列表中 -->
  <description>简要描述模组功能。</description>

  <!-- 支持的游戏版本列表 -->
  <supportedVersions>
    <li>1.6</li>
    <li>1.5</li>
  </supportedVersions>

  <!-- 硬依赖：缺失则无法加载（模组与 DLC 均可声明于此） -->
  <modDependencies>
    <li>
      <packageId>brrainz.harmony</packageId>
      <displayName>Harmony</displayName>
      <steamWorkshopUrl>steam://url/CommunityFilePage/2009463077</steamWorkshopUrl>
      <downloadUrl>https://github.com/pardeike/HarmonyRimWorld/releases/latest</downloadUrl>
    </li>
    <!-- DLC 依赖声明（可选，声明后游戏会提示缺少 DLC） -->
    <li>
      <packageId>Ludeon.RimWorld.Royalty</packageId>
      <displayName>RimWorld - Royalty</displayName>
    </li>
  </modDependencies>

  <!-- 按版本区分的硬依赖（1.6 起常用写法） -->
  <modDependenciesByVersion>
    <v1.6>
      <li>
        <packageId>brrainz.harmony</packageId>
        <displayName>Harmony</displayName>
      </li>
    </v1.6>
  </modDependenciesByVersion>

  <!-- 在以下模组之后加载（保证被 patch 的模组先加载） -->
  <loadAfter>
    <li>brrainz.harmony</li>
  </loadAfter>

  <!-- 在以下模组之前加载（被别人依赖时使用） -->
  <loadBefore>
    <li>some.other.mod</li>
  </loadBefore>

  <!-- 与以下模组不兼容（同时加载会警告） -->
  <incompatibleWith>
    <li>some.conflicting.mod</li>
  </incompatibleWith>

  <!-- 模组列表图标路径（可选） -->
  <modIconPath>Textures/UI/ModIcon</modIconPath>

</ModMetaData>
```

### 字段速查表

| 字段 | 必需 | 说明 |
|------|------|------|
| `packageId` | 是 | 全局唯一标识，发布后不可改 |
| `name` | 是 | 显示名称 |
| `author` | 是 | 作者 |
| `description` | 否 | 简介 |
| `supportedVersions` | 是 | 支持版本列表 |
| `modDependencies` | 否 | 硬依赖（全版本） |
| `modDependenciesByVersion` | 否 | 按版本的硬依赖 |
| `loadAfter` | 否 | 后于指定模组加载 |
| `loadBefore` | 否 | 先于指定模组加载 |
| `incompatibleWith` | 否 | 不兼容模组 |
| `modIconPath` | 否 | 列表图标路径 |

> 注意：`packageId` 一旦在工坊发布并积累用户，**绝不能修改**，否则游戏会视为全新模组，导致玩家配置与存档关联断裂。

---

## 三、命名规范

### 3.1 defName 前缀

所有自定义 `defName` 必须加唯一前缀，避免与其他模组冲突：

```xml
<!-- 正确：带前缀 -->
<defName><YourPrefix>_IronSword</defName>

<!-- 错误：无前缀，极易冲突 -->
<defName>IronSword</defName>
```

- 前缀统一使用你的 packageId 中的作者名或缩写，如 ` packageId` 为 `<YourPrefix>.xxx` 则 defName 前缀用 `<YourPrefix>_`。
- 全部使用驼峰或下划线，保持一致。

### 3.2 namespace

C# 代码命名空间统一使用作者名前缀：

```csharp
namespace <YourPrefix>.YourMod
{
    public class YourClass { }
}
```

### 3.3 文件命名

- XML 文件按 Def 类型分组：`Things_Weapons.xml`、`Recipes.xml`、`Patches_Weapons.xml`。
- 贴图文件名与 defName 对应，便于查找。
- 翻译文件名与被翻译的 Def 文件对应。

---

## 四、LoadFolders.xml 条件加载机制

`LoadFolders.xml`（放在模组根目录）控制不同游戏版本、不同模组激活状态下加载哪些文件夹。文件本身**可选**，但当需要版本专属内容或条件加载时必需。

根元素为 `<loadFolders>`，其下用 `<li>` 表示一条加载规则，每条 `<li>` 可带 `IfModActive`、`IfModNotActive`、`IfModActiveAll` 等属性，并可用 `<v1.6>` 等版本节点包裹。

### 4.1 写法示例一：纯版本专属目录

最常见用法，为不同版本提供专属文件夹：

```xml
<?xml version="1.0" encoding="utf-8"?>
<loadFolders>
  <li IfModActive="brrainz.harmony">/</li>
  <li>/</li>
  <li IfModActive="Ludeon.RimWorld.Royalty">/Royalty/</li>
  <li IfModActive="Ludeon.RimWorld.Biotech">/Biotech/</li>
</loadFolders>
```

### 4.2 写法示例二：版本节点包裹

用 `<v1.6>` 等节点为特定版本指定不同加载目录：

```xml
<?xml version="1.0" encoding="utf-8"?>
<loadFolders>
  <li>
    <v1.5>/1.5/</v1.5>
    <v1.6>/1.6/</v1.6>
  </li>
  <li>/Common/</li>
</loadFolders>
```

> 含义：1.5 加载 `/1.5/` + `/Common/`；1.6 加载 `/1.6/` + `/Common/`。`/Common/` 为所有版本共享。

### 4.3 写法示例三：IfModActive 条件加载

当某个模组激活时，额外加载指定目录（常用于可选兼容内容）：

```xml
<?xml version="1.0" encoding="utf-8"?>
<loadFolders>
  <li>/</li>
  <li IfModActive="UnlimitedHugs.HugsLib">/Compat_HugsLib/</li>
  <li IfModActive="OskarPotocki.VanillaExpandedFramework">/Compat_VEF/</li>
</loadFolders>
```

### 4.4 写法示例四：IfModNotActive / IfModActiveAll

- `IfModNotActive="id"`：当指定模组**未**激活时加载。
- `IfModActiveAll="id1,id2"`：当列出的模组**全部**激活时加载（需同时存在）。

```xml
<?xml version="1.0" encoding="utf-8"?>
<loadFolders>
  <li>/</li>
  <!-- 当某冲突模组未启用时，加载自己的替代实现 -->
  <li IfModNotActive="some.conflicting.mod">/Replacement/</li>
  <!-- 当两个前置框架都启用时，加载深度集成内容 -->
  <li IfModActiveAll="UnlimitedHugs.HugsLib,OskarPotocki.VanillaExpandedFramework">/DeepIntegration/</li>
</loadFolders>
```

### 条件属性速查

| 属性 | 触发条件 | 典型场景 |
|------|----------|----------|
| （无） | 总是加载 | 基础内容 |
| `IfModActive="id"` | 指定模组已启用 | 可选兼容补丁 |
| `IfModNotActive="id"` | 指定模组未启用 | 提供替代实现 |
| `IfModActiveAll="a,b"` | 列出模组全部启用 | 多框架深度集成 |
| `<v1.6>` 节点 | 指定游戏版本 | 版本专属内容 |

> 重要：被 `<v1.6>` 包裹的版本节点，其版本号必须出现在 `About.xml` 的 `supportedVersions` 中，否则不生效。

---

## 五、快速检查清单

- [ ] `About/About.xml` 存在且 `packageId` 符合 `作者名.模组名` 格式、无空格。
- [ ] `supportedVersions` 包含目标版本 `1.6`。
- [ ] 所有 `defName` 均带唯一前缀。
- [ ] C# 命名空间统一带前缀。
- [ ] 硬依赖已写入 `modDependencies` 或 `modDependenciesByVersion`。
- [ ] `loadAfter` 包含所有被 patch 的模组，保证加载顺序正确。
- [ ] 如有版本专属内容，已配置 `LoadFolders.xml` 且版本节点与 `supportedVersions` 一致。
- [ ] `Preview.png` 为 640x360（发布前）。
- [ ] 目录结构清晰，无多余调试文件残留。
