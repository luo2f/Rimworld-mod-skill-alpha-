# 测试通过后正规化 + 可选发布

> **适用版本**: RimWorld 1.6 | **最后更新**: 2026-07-28

## 前提

- 测试版已通过开发者验证（功能正常、无报错、贴图正确、属性符合预期）
- 测试版中使用的 `test.` 前缀 packageId 需要清理
- 所有临时引用的原版贴图需要替换为原创贴图

> 如果尚未完成测试版验证，请先按 `workflows/new-mod.md` 生成测试版并测试通过。

---

## 步骤 1：清理命名

将测试版中的临时标识替换为正式标识：

| 项目 | 测试版 | 正式版 |
|------|--------|-------|
| packageId | `test.<前缀>.<mod名>` | `<!-- 用户自行填写 -->` 格式：`<用户名>.<mod名>`（全小写，无空格） |
| defName 前缀 | 已使用用户选择的前缀 | 确认前缀唯一性，检查是否与其他 Mod 冲突 |

**检查前缀唯一性**：
- 搜索所有 Def 文件，确认每个 defName 都带有用户选择的前缀
- 前缀建议 2-4 个大写字母，避免使用过于通用的前缀
- 如前缀可能与现有 Mod 冲突，建议开发者更换前缀

修改 About.xml 中的 packageId：

```xml
<!-- 修改前 -->
<packageId>test.<!-- 前缀小写 -->.<!-- mod名小写 --></packageId>

<!-- 修改后 -->
<packageId><!-- 用户自行填写：用户名小写 -->.<!-- 用户自行填写：mod名小写 --></packageId>
```

---

## 步骤 2：补全 About.xml

将测试版的最简 About.xml 替换为完整配置：

```xml
<?xml version="1.0" encoding="utf-8"?>
<ModMetaData>
  <name><!-- 用户自行填写：正式Mod名称 --></name>
  <author><!-- 用户自行填写：作者名 --></author>
  <packageId><!-- 用户自行填写：用户名.mod名（全小写无空格） --></packageId>
  <supportedVersions>
    <li>1.6</li>
  </supportedVersions>
  <description><!-- 用户自行填写：完整的Mod描述 -->

[AI 辅助生成]</description>
  <modVersion ignoreIfMissing="true"><!-- 用户自行填写：版本号如1.0.0 --></modVersion>

  <!-- 如有依赖的Mod，添加modDependencies -->
  <modDependencies>
    <!-- 示例：依赖其他Mod时添加（如不需要则删除此节点） -->
    <!--
    <li>
      <packageId><!-- 依赖Mod的packageId --></packageId>
      <displayName><!-- 依赖Mod的名称 --></displayName>
      <steamWorkshopURL><!-- Steam Workshop链接 --></steamWorkshopURL>
      <downloadUrl><!-- 备用下载链接 --></downloadUrl>
    </li>
    -->
  </modDependencies>

  <!-- 如需在特定Mod之后加载，添加loadAfter -->
  <loadAfter>
    <!-- 示例：需要在某Mod之后加载时添加（如不需要则删除此节点） -->
    <!--
    <li><!-- 目标Mod的packageId --></li>
    -->
  </loadAfter>

  <!-- 如需在特定Mod之前加载，添加loadBefore -->
  <loadBefore>
    <!-- 示例：需要在某Mod之前加载时添加（如不需要则删除此节点） -->
  </loadBefore>
</ModMetaData>
```

**注意**：
- `<description>` 末尾必须添加 `[AI 辅助生成]` 标注
- `modDependencies`、`loadAfter`、`loadBefore` 仅在确实需要时添加，不需要则删除整个节点
- `modVersion` 建议使用语义化版本号（如 `1.0.0`）

---

## 步骤 3：替换正式贴图

将测试版中引用的原版贴图全部替换为原创 PNG 贴图：

1. **检查所有 Def 中的 texPath**：搜索所有 `<texPath>` 标签，找出引用原版路径的条目
2. **创建原创贴图**：根据物品类型创建对应的 PNG 文件
3. **更新 texPath**：将原版路径改为本 Mod 内的路径

贴图放置规则：

| 物品类型 | 贴图目录 | 备注 |
|----------|---------|------|
| 武器/物品 | `Textures/Things/Item/` | PNG 格式 |
| 服装 | `Textures/Things/Pawn/Humanlike/Apparel/` | 需要对应身体部位 |
| 材料 | `Textures/Things/Item/Resources/` | PNG 格式 |
| 建筑 | `Textures/Things/Building/` | 按建筑尺寸制作 |

贴图制作要点：
- 格式：PNG
- 背景：透明
- 武器/物品贴图建议尺寸：128x128 或 256x256
- 建筑贴图按实际尺寸计算（见 `workflows/add-building.md`）

```xml
<!-- 修改前：测试版引用原版贴图 -->
<texPath>Things/Item/Equipment/WeaponMelee/Knife</texPath>

<!-- 修改后：使用本Mod原创贴图 -->
<texPath>Things/Item/<!-- 你的defName或自定义名 --></texPath>
```

> 贴图制作详细说明见 `references/07-assets.md`。

---

## 步骤 4：添加 Preview.png

为 Mod 添加 Steam Workshop 预览图：

- **文件名**：`Preview.png`
- **位置**：Mod 根目录（与 About 文件夹同级）
- **尺寸**：640x360 像素
- **格式**：PNG

预览图内容建议：
- 展示 Mod 的核心内容（武器外观、建筑效果等）
- 画面清晰、主体突出
- 可包含简单文字说明

```
<Mod名>/
├── About/
│   └── About.xml
├── Defs/
├── Textures/
├── Patches/
└── Preview.png          ← 添加预览图
```

---

## 步骤 5：添加多语言支持

至少添加 English 语言支持，建议同时添加简体中文：

```
<Mod名>/
├── Languages/
│   ├── English/
│   │   └── Keyed/
│   │       └── <Mod名>.xml       ← 英文翻译
│   └── ChineseSimplified/
│       └── Keyed/
│           └── <Mod名>.xml       ← 中文翻译（可选，开发语言为中文时）
```

Keyed 翻译文件格式：

```xml
<?xml version="1.0" encoding="utf-8"?>
<LanguageData>
  <!-- 英文版示例 -->
  <<!-- 你的defName-->.label>English Label</<!-- 你的defName-->.label>
  <<!-- 你的defName-->.description>English Description</<!-- 你的defName-->.description>
</LanguageData>
```

> 如果 Def 中 label 和 description 已使用中文硬编码，English 版本需提供对应英文翻译。
> 翻译系统详细说明见 `references/07-assets.md`。

---

## 步骤 6：版本管理

选择以下方式之一进行版本管理：

### 方式 A：About.xml 内嵌版本（推荐简单 Mod）

已在步骤 2 中添加的 `<modVersion>` 字段：

```xml
<modVersion ignoreIfMissing="true">1.0.0</modVersion>
```

### 方式 B：ModSync.xml（推荐使用 ModSync 的社区）

在 Mod 根目录创建 `ModSync.xml`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<ModSync>
  <Name><!-- 用户自行填写：Mod名称 --></Name>
  <Id><!-- 用户自行填写：唯一标识（建议与packageId一致） --></Id>
  <Version><!-- 用户自行填写：版本号如1.0.0 --></Version>
  <!-- 如有依赖项 -->
  <Dependencies>
    <!--
    <li><!-- 依赖Mod的标识 --></li>
    -->
  </Dependencies>
  <!-- 如有外部资源链接 -->
  <ManifestSource/>
</ModSync>
```

> 版本号建议遵循语义化版本规范：主版本.次版本.修订号。

---

## 步骤 7：可选 — 发布到 Steam Workshop

如果开发者希望发布到 Steam Workshop，参考 `references/09-workshop.md` 中的完整发布流程。

核心要点：
1. 在 Steam 中安装 RimWorld 的开发工具
2. 通过游戏内界面上传 Mod
3. 填写 Workshop 页面信息（标题、描述、标签、预览图）
4. Workshop 描述中标注"本 Mod 部分内容由 AI 辅助生成"
5. 设置可见性（公开/仅好友/私密）

---

## 正规化检查清单

逐项确认正规化完成：

### 命名与标识
- [ ] packageId 已去掉 `test.` 前缀，使用正式格式
- [ ] defName 前缀唯一性已确认
- [ ] 所有 defName 均带有用户选择的前缀

### About.xml
- [ ] 使用正式 Mod 名称
- [ ] author 字段已由用户填写
- [ ] description 完整且末尾包含 `[AI 辅助生成]` 标注
- [ ] supportedVersions 包含 1.6
- [ ] modDependencies / loadAfter / loadBefore 按需配置
- [ ] modVersion 已设置

### 贴图
- [ ] 所有 texPath 已替换为本 Mod 内的原创贴图
- [ ] 无残留的原版贴图引用
- [ ] Preview.png 已添加（640x360）

### 多语言
- [ ] English/Keyed/ 已添加
- [ ] 所有需要翻译的 label/description 都有对应条目

### 版本管理
- [ ] 已选择版本管理方式（About.xml 或 ModSync.xml）
- [ ] 版本号已设置

### 最终验证
- [ ] 清理后重新进游戏测试，确认无报错
- [ ] 所有功能正常

全部通过后，Mod 已完成正规化，可正常使用或发布。

---

## 相关文档

- 测试版创建：`workflows/new-mod.md`
- 项目结构详解：`references/02-project-structure.md`
- 资源制作：`references/07-assets.md`
- Steam Workshop 发布：`references/09-workshop.md`
