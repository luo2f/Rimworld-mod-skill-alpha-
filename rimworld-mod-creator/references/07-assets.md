# 07 - 资源制作

> **适用版本**: RimWorld 1.6 | **最后更新**: 2026-07-28

本文档讲解模组资源制作规范：纹理格式、贴图路径、渲染类型、着色器、Mask 染色系统、翻译系统、音效资源与工具推荐。

---

## 一、纹理格式规范

- **格式**：必须为 PNG。
- **透明背景**：需要镂空的区域设为完全透明（alpha=0），不要用纯色填充。
- **命名**：与 defName 对应，纯英文小写、下划线，便于查找。

### 尺寸建议表

| 资源类型 | 推荐尺寸（像素） | 说明 |
|----------|------------------|------|
| 小型物品贴图 | 128 × 128 | 武器、弹药、小型资源 |
| 中型物品贴图 | 256 × 256 | 装备、较大物品 |
| 建筑贴图 | 格数 × 256 | 1 格建筑 256×256，2 格 512×256，依此类推 |
| UI 图标 | 32 × 32 或 64 × 64 | 按钮、状态图标 |
| Preview.png | 640 × 360 | 工坊预览图（必需） |
| Pawn 全身 | 128 × 128 | 标准 Pawn 贴图 |

> 尺寸为 2 的幂次最稳妥；非 2 次幂贴图也能用，但性能与兼容性以 2 次幂为佳。

---

## 二、贴图路径约定

- `texPath` 相对于模组的 `Textures/` 目录。
- **不带文件扩展名**（`.png` 省略）。
- 用**正斜杠** `/` 分隔目录，即使 Windows 上也是如此。

```xml
<graphicData>
  <!-- 实际文件：Textures/Things/Item/Equipment/YourTexture.png -->
  <texPath>Things/Item/Equipment/YourTexture</texPath>
</graphicData>
```

> 路径写错会导致贴图缺失（物体显示为粉黑格子）。

---

## 三、贴图渲染类型 graphicClass

| graphicClass | 渲染方式 | 文件命名约定 |
|--------------|----------|--------------|
| `Graphic_Single` | 单张图 | 一张 `Tex.png` |
| `Graphic_Multi` | 四方向，前后左右 | `Tex_n`、`Tex_s`、`Tex_e`、`Tex_w`（n=北/后，s=南/前，e=东/右，w=西/左） |
| `Graphic_Random` | 同目录随机抽取一张 | `Tex0`、`Tex1`、`Tex2`... |
| `Graphic_Animated` | 序列帧动画 | `Tex0`、`Tex1`... 配合 `ticksPerFrame` |
| `Graphic_Slicer` | 大型建筑切片 | 按格切片 |

### Graphic_Multi 方向后缀说明

RimWorld 中 `_n`/`_s`/`_e`/`_w` 分别对应北、南、东、西。通常南面（`_s`，朝向玩家）是正面，北面（`_n`）是背面。左右（`_e`/`_w`）通常互为镜像。

```xml
<graphicData>
  <texPath>Things/Building/YourBuilding</texPath>
  <graphicClass>Graphic_Multi</graphicClass>
  <drawSize>(2,2)</drawSize>
</graphicData>
<!-- 文件：YourBuilding_n.png / _s.png / _e.png / _w.png -->
```

---

## 四、着色器类型 shaderType

`shaderType` 决定贴图的渲染方式，影响透明、光照、染色表现。

| shaderType | 特点 | 典型用途 |
|-----------|------|----------|
| `Cutout` | 硬边缘透明，不染主色 | 建筑外观（不受颜色影响） |
| `CutoutComplex` | 硬透明 + 支持遮罩染色 + 光照 | 可染色物品、装备（最常用） |
| `Transparent` | 半透明渐变 | 玻璃、光效、烟雾 |
| `MetaOverlay` | 元数据贴图（地图绘制用） | 建筑 meta 图 |
| `Skin` | 皮肤专用，支持肤色染色 | Pawn 皮肤贴图 |
| `None` | 不指定，用默认 | 一般情况 |

```xml
<graphicData>
  <texPath>Things/Item/YourItem</texPath>
  <graphicClass>Graphic_Single</graphicClass>
  <shaderType>CutoutComplex</shaderType>
</graphicData>
```

---

## 五、Mask 图层染色系统

RimWorld 支持物品染色（如材料色、装备染色）。通过 **Mask 图层**实现，文件名加 `_m` 后缀。

### 原理

- 主贴图 `Tex.png`：物体的基础外观。
- 遮罩贴图 `Tex_m.png`：RGB 三个通道分别控制三块染色区域：
  - **R（红）通道**：主色（color）染色区域。
  - **G（绿）通道**：副色（colorTwo）染色区域。
  - **B（蓝）通道**：不染色（保持原色）区域。

### XML 配置

```xml
<graphicData>
  <texPath>Things/Item/YourColoredItem</texPath>
  <graphicClass>Graphic_Single</graphicClass>
  <shaderType>CutoutComplex</shaderType>
</graphicData>
```

在 Def 上指定 `color`（主色）与 `colorTwo`（副色）：

```xml
<color>(180,180,180)</color>
<colorTwo>(80,80,80)</colorTwo>
```

> Mask 贴图与主贴图同名加 `_m`，如 `YourColoredItem.png` + `YourColoredItem_m.png`。玩家可在游戏中通过染色器选择颜色，遮罩决定哪些区域被染色。

---

## 六、翻译系统

RimWorld 翻译分两种机制：Keyed（键值）与 DefInjected（Def 字段注入）。

### 6.1 目录结构

```
Languages/
├── English/
│   ├── LangIcon.png              # 语言图标（可选）
│   ├── LanguageInfo.xml          # 语言元信息
│   ├── Keyed/                    # 键值翻译
│   │   └── YourMod_Keys.xml
│   └── DefInjected/              # Def 字段翻译
│       └── ThingDef/
│           └── YourMod_Things.xml
├── ChineseSimplified/
│   ├── Keyed/
│   └── DefInjected/
└── ...
```

### 6.2 LanguageInfo.xml

每个语言文件夹下需有 `LanguageInfo.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<LanguageInfo>
  <friendlyNameEnglish>Chinese Simplified</friendlyNameEnglish>
  <friendlyNameNative>简体中文</friendlyNameNative>
  <languageFolderName>ChineseSimplified</languageFolderName>
  <credits>
    <li YourName="Translator" />
  </credits>
</LanguageInfo>
```

### 6.3 Keyed 键值翻译

用 `{0}`、`{1}` 占位符支持动态参数：

```xml
<!-- English/Keyed/YourMod_Keys.xml -->
<LanguageData>
  <YourPrefix_Greeting>Hello, {0}!</YourPrefix_Greeting>
  <YourPrefix_Enabled>Feature enabled.</YourPrefix_Enabled>
</LanguageData>
```

```xml
<!-- ChineseSimplified/Keyed/YourMod_Keys.xml -->
<LanguageData>
  <YourPrefix_Greeting>你好，{0}！</YourPrefix_Greeting>
  <YourPrefix_Enabled>功能已启用。</YourPrefix_Enabled>
</LanguageData>
```

C# 中引用：`"YourPrefix_Greeting".Translate(pawn.NameStringShort)`。

### 6.4 DefInjected Def 字段翻译

按 Def 类型分子文件夹，翻译 Def 的 `label`、`description` 等字段：

```xml
<!-- ChineseSimplified/DefInjected/ThingDef/YourMod_Things.xml -->
<LanguageData>
  <YourPrefix_IronGlaive.label>铁刃长戟</YourPrefix_IronGlaive.label>
  <YourPrefix_IronGlaive.description>一柄长铁戟，便于保持距离。</YourPrefix_IronGlaive.description>
</LanguageData>
```

> 翻译键格式：`<defName>.<字段名>`，如 `YourPrefix_IronGlaive.label`。

### 6.5 汉化模组命名约定

独立汉化模组（翻译他人模组）通常命名为：
`[Language] <ModName> - 简繁中文汉化包`

并在 `About.xml` 中将原模组列为 `loadAfter` 依赖，`Patches/` 或 `Languages/` 中提供翻译。本模组自带翻译则直接放在模组 `Languages/` 下，无需单独汉化模组。

---

## 七、音效资源

### 7.1 格式与路径

- 格式：WAV 或 OGG。
- 路径约定：放在 `Sounds/` 下，`SoundDef` 中用相对路径（带扩展名）引用。

### 7.2 SoundDef 示例

```xml
<Defs>
  <SoundDef>
    <defName><YourPrefix>_HitSound</defName>
    <sustain>False</sustain>
    <subSounds>
      <li>
        <grains>
          <li Class="AudioGrain_Folder">
            <!-- 文件夹路径，随机播放其中音频 -->
            <clipFolderPath>Interact/YourHitSound</clipFolderPath>
          </li>
        </grains>
        <pitchRange>0.95~1.05</pitchRange>
        <volumeRange>30~40</volumeRange>
      </li>
    </subSounds>
  </SoundDef>
</Defs>
```

> 路径 `Interact/YourHitSound` 对应 `Sounds/Interact/YourHitSound/` 文件夹，内放 wav/ogg 文件。

---

## 八、工具推荐表

| 工具 | 用途 | 平台 | 备注 |
|------|------|------|------|
| GIMP | 位图编辑（免费） | 全平台 | 支持图层、通道，适合做 Mask |
| Paint.NET | 位图编辑（免费） | Windows | 轻量易上手，PNG 处理好 |
| Aseprite | 像素画 | 全平台 | 适合像素风贴图、动画 |
| Photoshop | 专业位图编辑 | 全平台 | 付费，功能最全 |
| Audacity | 音频编辑（免费） | 全平台 | 录制、剪辑、转 wav/ogg |
| BFXR | 音效生成（免费） | 全平台 | 生成复古游戏音效 |
| TexturePacker | 贴图打包 | 全平台 | 可选，批量处理 |

> 提示：制作 Mask 染色贴图时，务必在支持 RGB 通道分离的工具（GIMP/Photoshop）中精确绘制各通道灰度，错误的通道会导致染色错乱。
