# 从零创建一个新 Mod（测试版先行）

> **适用版本**: RimWorld 1.6 | **最后更新**: 2026-07-28

## 概述

本工作流遵循**测试版先行原则**：先生成可进游戏测试的测试版 Mod，开发者在游戏中验证功能正确、无报错后，再进行正规化处理。这样可以尽早暴露问题，避免在大量内容完成后才发现根本性错误。

```
用户需求 → 生成测试版 → 开发者进游戏测试
                        ├─ 有 bug → 修复 → 重新测试
                        └─ 确认无误 → 正规化（见 formalize-mod.md）
```

测试版的核心目标是**功能可验证**，因此允许：
- 使用 `test.` 前缀的 packageId
- 暂时引用原版贴图路径
- About.xml 使用最简配置
- 暂不添加多语言支持

---

## 阶段一：生成测试版

### 第 1 步：确定 Mod 信息

向开发者确认以下信息，未确认的字段标注为"用户自行填写"：

| 字段 | 说明 | 示例/规则 |
|------|------|----------|
| Mod 名称 | 显示名称 | 用户自行填写 |
| packageId（测试版） | 唯一标识 | `test.<前缀小写>.<mod名小写>` |
| defName 前缀 | 所有 defName 统一前缀 | 用户自行选择，如 `XX_` |
| 作者 | About.xml 中的 author | 用户自行填写 |
| 目标版本 | RimWorld 版本 | `1.6` |
| namespace | C# 命名空间 | `<前缀>.<Mod名>`（用户根据前缀确定） |

> 提醒开发者：前缀需唯一，避免与其他 Mod 冲突。建议使用 2-4 个大写字母。

### 第 2 步：验证原版结构

**铁律：生成 Def 前必须先查原版对应文件，确认字段名、枚举值、ParentName 继承链正确。**

- 有模板的类型（近战武器、远程武器、服装、材料、建筑、配方）：模板已验证原版结构，可直接使用，跳过此步。
- 无模板的新类型：用 grep 搜索原版 Defs 目录，模仿原版结构写出 Def。

```bash
# 示例：搜索原版中某字段的用法
grep -r "<字段名>" "<RimWorld安装路径>/Data/Core/Defs/"
# 示例：搜索 ParentName 定义
grep -r 'Name="BaseXxx"' "<RimWorld安装路径>/Data/Core/Defs/"
```

查找路径参考 `references/03-xml-defs.md` 中列出的原版 Def 文件分布。

### 第 3 步：创建测试版目录结构

创建以下最小化目录结构：

```
<Mod名>/
├── About/
│   └── About.xml          # 最简配置（见下文）
├── Defs/
│   └── ThingDefs/          # 根据内容类型可调整子目录
│       └── <文件名>.xml
├── Textures/
│   └── Things/
│       └── Item/           # 武器/物品贴图（根据类型调整）
│           └── <defName>.png
└── Patches/                # 如需修改原版（可空）
```

测试版 About.xml 最简配置：

```xml
<?xml version="1.0" encoding="utf-8"?>
<ModMetaData>
  <name><!-- 用户自行填写：Mod名称 --></name>
  <author><!-- 用户自行填写：作者名 --></author>
  <packageId>test.<!-- 用户自行填写：前缀小写 -->.<!-- 用户自行填写：mod名小写 --></packageId>
  <supportedVersions>
    <li>1.6</li>
  </supportedVersions>
  <description>测试版 - 功能验证中</description>
</ModMetaData>
```

### 第 4 步：生成 Def 和贴图

根据内容类型选择对应模板，替换 `<Your...>` 占位符：

| 内容类型 | 模板文件 | 贴图目录 |
|----------|---------|---------|
| 近战武器 | `templates/weapon-melee.xml` | `Textures/Things/Item/` |
| 远程武器 | `templates/weapon-ranged.xml` | `Textures/Things/Item/` |
| 服装/护甲 | `templates/apparel.xml` | `Textures/Things/Pawn/Humanlike/Apparel/` |
| 材料 | `templates/resource-stuff.xml` | `Textures/Things/Item/Resources/` |
| 建筑 | `templates/building.xml` | `Textures/Things/Building/` |
| 配方 | `templates/recipe.xml` | （配方无独立贴图） |

**测试版贴图处理**：可暂时引用原版贴图路径，待正规化时替换为原创贴图。示例：

```xml
<!-- 测试版：引用原版贴图 -->
<texPath>Things/Item/Equipment/WeaponMelee/Knife</texPath>
```

替换占位符时注意：
- `defName`：加用户选择的前缀，如 `XX_TestWeapon`
- `label`：中文显示名
- `description`：中文描述
- 数值参数：参考原版同类物品的数值范围进行调整

### 第 5 步：告知开发者测试方法

生成完成后，向开发者说明测试步骤：

1. **启动 RimWorld**
2. 进入 **Mod 列表**，勾选你的测试 Mod（确保 Core 已启用）
3. 进入游戏后开启 **DevMode**（选项 → 开发者模式）
4. 使用 **Debug 动作菜单**（顶部图标）→ **Spawn Thing** → 搜索你的 defName
5. 生成物品后检查：
   - 是否有红字报错（左下角日志）
   - 贴图是否正确显示
   - 鼠标悬停查看属性是否正确
6. 如需测试建筑：开启 **God Mode**（Debug 菜单 → God Mode）可免费建造

---

## 阶段二：测试 → 修复循环

开发者反馈后，按对照表处理：

| 反馈情况 | 处理动作 |
|---------|---------|
| 测试 OK，无报错 | → 进入阶段三：正规化 |
| 有红字报错 | → 读取 Player.log → 定位错误 → 修 XML → 重新测试 |
| 属性数值不对 | → 调整 statBases / verbs 中的数值 → 重新测试 |
| 贴图不显示 | → 检查 texPath 路径是否正确、文件名大小写 → 修复 → 重新测试 |
| 物品无法生成 | → 检查 defName 拼写、ParentName 是否存在、category 是否正确 → 修复 → 重新测试 |
| 制作配方无效 | → 检查 recipeDef 的 recipeUsers 或用 Patch 添加到工作台 → 修复 → 重新测试 |

> 详细的错误排查方法见 `workflows/debug-crash.md`。

每次修复后，提示开发者重新进入游戏验证，直到测试通过。

---

## 阶段三：正规化

测试通过后，进行正规化处理。详细步骤见 `workflows/formalize-mod.md`，此处列出核心要点：

1. **清理 packageId**：去掉 `test.` 前缀，改为正式的 `用户名.mod名` 格式
2. **补全 About.xml**：添加正式名称、完整描述（含 `[AI 辅助生成]` 标注）、modVersion
3. **替换正式贴图**：用原创 PNG 替换原版贴图引用
4. **添加 Preview.png**：640x360 像素的预览图
5. **添加多语言支持**：至少包含 `Languages/English/Keyed/`

---

## 检查清单

测试版完成前，逐项确认：

- [ ] **Dev Mode 下 Spawn 成功**：能在 Spawn Thing 菜单找到并生成
- [ ] **贴图正确显示**：无紫色方块、无缺失
- [ ] **属性正确**：伤害、耐久、工作量等数值符合预期
- [ ] **无红字报错**：Player.log 中无与本 Mod 相关的错误
- [ ] **开发者确认 OK**：开发者亲自验证功能正常

全部通过后，告知开发者可进入正规化阶段。

---

## 相关文档

- 正规化流程：`workflows/formalize-mod.md`
- 项目结构详解：`references/02-project-structure.md`
- XML Def 系统：`references/03-xml-defs.md`
- 崩溃排查：`workflows/debug-crash.md`
