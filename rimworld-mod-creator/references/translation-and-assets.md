# 翻译与资源引用参考

## 翻译系统

RimWorld 翻译分两层：**Keyed**（键值翻译，用于 C# 代码硬编码的界面文本）和 **DefInjected**（自动翻译 Def 字段）。

### 目录结构

```
Languages/
├── ChineseSimplified/          # 语言文件夹（也可写 ChineseSimplified (简体中文)）
│   ├── LanguageInfo.xml        # 语言元信息（必须）
│   ├── Keyed/                  # 键值翻译
│   │   └── Translations.xml
│   └── DefInjected/            # Def 注入翻译（按 DefType 分目录）
│       ├── ThingDef/
│       ├── HediffDef/
│       └── ResearchProjectDef/
├── English/
├── Japanese/
└── ...
```

### LanguageInfo.xml

```xml
<?xml version="1.0" encoding="utf-8"?>
<LanguageInfo>
  <friendlyNameNative>简体中文</friendlyNameNative>
  <friendlyNameEnglish>ChineseSimplified</friendlyNameEnglish>
  <canBeTiny>true</canBeTiny>
  <credits>
    <li Class="CreditRecord_Role">
      <roleKey>翻译</roleKey>
      <creditee>译者名</creditee>
    </li>
  </credits>
</LanguageInfo>
```

### Keyed/ — 键值翻译

根元素 `<LanguageData>`，每行 `<键>翻译文本</键>`。Keyed 键名通常是 `模组前缀.功能名`，C# 中通过 `"键名".Translate()` 调用。支持 `{0}`、`{1}` 等格式化占位符。

```xml
<LanguageData>
  <PUAH.allowCorpses>允许将尸体放入背包</PUAH.allowCorpses>
  <PUAH.minimumFreeInventorySpace>考虑搬运到背包的最小空闲空间</PUAH.minimumFreeInventorySpace>
</LanguageData>
```

### DefInjected/ — Def 字段翻译

按 Def 类型名分子目录（**目录名必须与 Def 类型名完全一致**）。键名格式 `DefDefName.字段名`，自动注入到对应 Def 的对应字段。

```xml
<!-- 文件：DefInjected/ThingDef/Desc.xml -->
<LanguageData>
  <EM_MilkingSpot.description>指定一个挤奶位置</EM_MilkingSpot.description>
  <EM_MilkingSpot.label>挤奶点</EM_MilkingSpot.label>
</LanguageData>
```

可翻译的字段包括 `label`、`description`、`labelNoun`、`labelPlural`、`reportString`、`gerund`、`verb`、`stages/li/label`、`stages/li/description` 等。

### 汉化模组命名约定

- 边缘汉化组（leafzxg）：`RWZH.ChinesePack.*` 前缀
- MZMGOW 等：`ZH.*` 前缀复制原 packageId
- 其他：`_zh`/`zh-pack`/`汉化`/`chs` 后缀
- 多数仅含 `Languages/` 目录，依赖原 mod 并 `loadAfter` 原 mod，支持版本常超前到 1.7–1.9 占位

## 贴图引用路径规则

`<texPath>` 的值是相对于模组根目录下 `Textures/` 文件夹的路径，**不带扩展名**（自动补 `.png`）。路径用正斜杠 `/` 分隔。

| texPath 值 | 实际文件位置 |
|------------|-------------|
| `Things/Building/Production/MilkingSpot` | `Textures/Things/Building/Production/MilkingSpot.png` |
| `Things/Projectile/Bullet_Small` | `Textures/Things/Projectile/Bullet_Small.png` |
| `UI/Icons/Genes/Gene_Lactation` | `Textures/UI/Icons/Genes/Gene_Lactation.png` |
| `Weapon/RK_Crossbow` | `Textures/Weapon/RK_Crossbow.png` |

## 贴图渲染类型（graphicClass）

| graphicClass | 用途 | 贴图命名约定 |
|--------------|------|-------------|
| `Graphic_Single` | 单图渲染 | 单一 PNG 文件 |
| `Graphic_Multi` | 四方向渲染 | 附加 `_north`、`_south`、`_east`、`_west` 后缀 |
| `Graphic_Random` | 随机抽图 | 多张图随机选取 |

配合 `shaderType`（如 `CutoutFlying`、`CutoutSkin`）和 `drawSize` 控制渲染效果。

## 音效资源

音效文件放 `Sounds/` 目录，支持 `.ogg`/`.wav` 格式。通过 `SoundDef` 定义，含 `subSounds`、`grains`、`volumeRange`、`pitchRange` 等字段控制播放参数。
