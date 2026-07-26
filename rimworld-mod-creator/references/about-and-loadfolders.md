# About.xml 与 LoadFolders.xml 参考

## About.xml 完整字段

```xml
<?xml version="1.0" encoding="utf-8"?>
<ModMetaData>
  <name>显示名</name>
  <author>作者（可多个用逗号）</author>
  <packageId>作者.模组名</packageId>           <!-- 小写，全局唯一 -->
  <modVersion>1.6.1</modVersion>                  <!-- 可选 -->
  <url>https://github.com/...</url>             <!-- 可选 -->
  <modIconPath>Icon/路径</modIconPath>        <!-- 可选 -->
  <supportedVersions>
    <li>1.4</li><li>1.5</li><li>1.6</li>
  </supportedVersions>
  <description>支持 CDATA 与富文本颜色标签</description>
  <modDependencies>
    <li>
      <packageId>brrainz.harmony</packageId>
      <displayName>Harmony</displayName>
      <steamWorkshopUrl>steam://url/CommunityFilePage/2009463077</steamWorkshopUrl>
      <downloadUrl>https://github.com/...</downloadUrl>
    </li>
  </modDependencies>
  <loadAfter>
    <li>brrainz.harmony</li>
    <li>Ludeon.RimWorld</li>              <!-- 核心 -->
    <li>Ludeon.RimWorld.Royalty</li>      <!-- DLC -->
  </loadAfter>
  <loadBefore>
    <li>Ludeon.RimWorld</li>              <!-- Harmony 用，确保最先加载 -->
  </loadBefore>
  <incompatibleWith>
    <li>某些冲突mod.packageId</li>
  </incompatibleWith>
</ModMetaData>
```

## 字段使用统计（234 个模组）

| 字段 | 使用数 | 占比 | 说明 |
|------|--------|------|------|
| `loadAfter` | 173 | 74% | 最常见的加载顺序声明 |
| `modDependencies` | 153 | 65% | 声明依赖的其他 mod |
| `url` | 54 | 23% | 主页或工坊链接 |
| `modIconPath` | 41 | 18% | 模组图标路径 |
| `modVersion` | 31 | 13% | 模组版本号 |
| `incompatibleWith` | 22 | 9% | 声明不兼容的 mod |
| `loadBefore` | 13 | 6% | 声明需在本 mod 之前加载的项 |

## 约定要点

- `packageId` 全小写，格式 `作者.模组名`，全局唯一
- DLC packageId：`Ludeon.RimWorld.Royalty/Ideology/Biotech/Anomaly/Odyssey`，核心是 `Ludeon.RimWorld`
- `loadAfter` 通常先列 harmony、再列 DLC、再列所依赖的框架 mod
- `description` 支持 `<b>`、`<color=...>`、`<size=...>` 等富文本标签，常用 `<![CDATA[...]]>` 包裹
- 汉化包典型写法：`packageId` 用 `ZH.原id` 或 `RWZH.ChinesePack.原名`，`loadAfter` 指向原 mod

## LoadFolders.xml 加载机制

无此文件时，游戏加载模组根目录全部内容；有此文件时，仅加载指定文件夹。共 85 个模组使用了此文件。

### 基本结构

根节点 `<loadFolders>`，按 RimWorld 版本用 `<v1.x>` 子节点分组。

### 条件属性

| 属性 | 使用数 | 含义 |
|------|--------|------|
| `IfModActive="id1,id2"` | 48 | 任一 mod 激活即加载（最常用） |
| `IfModNotActive="id"` | 7 | 未激活时加载（回退/兼容补丁） |
| `IfModActiveAll="id1,id2"` | 2 | 全部激活才加载 |

### 写法 1：根目录 + 版本子目录（最主流）

```xml
<loadFolders>
  <v1.6><li>/</li><li>1.6</li></v1.6>
</loadFolders>
```

`<li>/</li>` 表示加载模组根目录（公共内容），再叠加版本专属目录。

### 写法 2：IfModActive 按 DLC 条件加载

```xml
<loadFolders>
  <v1.6>
    <li>/</li><li>1.6</li>
    <li IfModActive="Ludeon.RimWorld.Ideology">1.6/Mods/Ideology</li>
    <li IfModActive="Ludeon.RimWorld.Odyssey">1.6/Mods/Odyssey</li>
  </v1.6>
</loadFolders>
```

### 写法 3：补丁矩阵（为多个 mod 各准备兼容目录）

```xml
<loadFolders>
  <v1.6>
    <li>/</li><li>1.6</li>
    <li IfModActive="Nals.FacialAnimation">Patch_FacialAnimation/1.6</li>
    <li IfModActive="erdelf.humanoidalienraces">Patch_AlienRaces/1.6</li>
    <li IfModActive="ceteam.combatextended">Patch_CombatExtended/1.6</li>
  </v1.6>
</loadFolders>
```

### 写法 4：IfModActiveAll + IfModNotActive 组合

```xml
<v1.6>
  <li>/</li><li>1.6</li>
  <!-- 两个 mod 都激活才加载 -->
  <li IfModActiveAll="nals.facialanimation,erdelf.HumanoidAlienRaces">1.6/Mod/FacialAnimation</li>
  <!-- Biotech 未激活时加载的回退补丁 -->
  <li IfModNotActive="Ludeon.RimWorld.Biotech">1.6/NonBiotechPatch</li>
</v1.6>
```

## 关键约定

- 文件名大小写不敏感，推荐放根目录命名 `LoadFolders.xml`
- `<li>/</li>` 的 `/` 代表模组根目录，是否带 `/` 决定根目录公共内容是否被加载
- 多 packageId 写法用逗号分隔，常为同一 mod 的 Steam 版（`xxx_steam`）与 GitHub 本地版两种 id 并列
- 版本节点出现频次：v1.6 出现 83 次、v1.5 出现 70 次、v1.4 出现 49 次
