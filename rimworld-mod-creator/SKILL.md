---
name: "rimworld-mod-creator"
description: "RimWorld mod 制作全流程指南——覆盖环境搭建、XML Def、C# 开发、Harmony 补丁、资源制作、调试和 Steam Workshop 发布。触发词：RimWorld, 环世界, rimworld, RW, mod, Mod, 模组, Def, XML, ThingDef, Harmony, Patch, 补丁, 武器, 建筑, 物品, 种族, 派系, 事件, Steam Workshop, 创意工坊, C#, DLL, 编译"
---

# RimWorld Mod 制作指南

## 核心决策：何时查原版，何时直接用模板

**三层决策——每次接收 mod 制作请求时，按以下优先级判断：**

```
用户请求
  │
  ├─ ① 有模板？ ─── 武器/服装/建筑/资源/配方/Harmony/ThingComp
  │     └─ ✅ 直接用模板生成（模板已验证过原版结构）
  │
  ├─ ② 报错/调试？ ─── 红字/白窗/崩溃/NullReferenceException
  │     └─ 🔍 查原版源码搜索错误原因（grep 或 dnSpy）
  │
  └─ ③ 无模板的新类型？ ─── 植物/生物/派系/事件/地形/Hediff/研究...
        └─ 📡 查原版 Def 结构 → 模仿写出 Def → 存储为新模板
```

### 模板 ↔ Def 类型对照表

| 用户需求 | 对应模板 | 查原版？ |
|----------|---------|:-------:|
| 近战武器 | `templates/weapon-melee.xml` | ❌ 免 |
| 远程武器 | `templates/weapon-ranged.xml` | ❌ 免 |
| 服装/护甲/头饰 | `templates/apparel.xml` | ❌ 免 |
| 原材料/建筑材料 | `templates/resource-stuff.xml` | ❌ 免 |
| 建筑/工作台 | `templates/building.xml` | ❌ 免 |
| 制作配方 | `templates/recipe.xml` | ❌ 免 |
| Harmony 补丁 | `templates/harmony-patch.cs` | ❌ 免 |
| C# ThingComp | `templates/thingcomp.cs` | ❌ 免 |
| 修改原版（XML） | `references/04-xml-patching.md` | ❌ 免 |
| **植物 / 生物 / 派系 / 事件** | **无模板** | 📡 查原版 |
| **地形 / Hediff / 研究 / 工作** | **无模板** | 📡 查原版 |
| 任何**错误排查** | `references/08-debugging.md` | 🔍 查原版 |

---

## 学习系统

每次加载此 Skill 时，必须先读取 `learnings/errors.txt` 中的历史错误记录，并在后续工作中避免重复犯错。

排查完错误并修复后，必须将关键教训总结为一句话追加到 `learnings/errors.txt`。
格式：`YYYY-MM-DD | <类别> | <一句话总结>`

| 类别 | 适用场景 |
|------|---------|
| XML | ThingDef/RecipeDef/Def 属性写错、枚举值错误、ParentName 错误 |
| C# | 编译错误、空引用、类型错误、DLL 加载问题 |
| Harmony | Patch ID 冲突、补丁未生效、Prefix/Postfix 签名错误 |
| Path | 文件路径错误、资源缺失、加载顺序问题 |
| Other | 上述类别之外的错误 |

---

## 核心工作流：测试版先行（Test → Verify → Formalize）

**所有 mod 生成后，默认先生成测试版本。开发者确认无 bug 后，再正规化并可选发布。**

```
用户需求 → 生成测试版 Mod → 开发者进游戏测试
                              ├─ 有 bug → 修复 → 重新测试
                              └─ 确认无误 → 正规化 → 可选：上传 Steam
```

---

## 快速导航

根据需求自动加载对应的参考文档和模板。

### 我想...

| 需求 | 加载内容 |
|------|---------|
| 新建一个 mod 项目 | `workflows/new-mod.md` + `references/02-project-structure.md` |
| 添加武器/物品/建筑/服装/植物 | `references/03-xml-defs.md` + 对应 `templates/*.xml` |
| 修改原版机制/打补丁 | `references/04-xml-patching.md`（XML）/ `references/06-harmony.md`（C#） |
| 编写 C# 代码/DLL | `references/05-csharp-basics.md` |
| 用 Harmony 拦截方法 | `references/06-harmony.md` + `templates/harmony-patch.cs` |
| 添加纹理/音效资源 | `references/07-assets.md` |
| 排查报错/崩溃/红字 | `references/08-debugging.md` |
| 查看历史错误记录 | `learnings/errors.txt`（自动读取） |
| 测试通过，正规化 mod | `workflows/formalize-mod.md` |
| 批量处理多个需求 | `workflows/batch-process.md` + `templates/requirements-template.md` |
| 发布到 Steam Workshop | `references/09-workshop.md` |
| 查询 API/类/方法 | `references/10-api-reference.md` |
| 查看框架/库依赖 | `references/frameworks.md` |

---

## 核心原则

### 1. 命名规范

- **前缀**：所有 defName 和 C# 类名使用唯一前缀（由用户自行选择，如 `RCS_`、`AWE_` 等），避免 mod 冲突
- **namespace**：`你的前缀.Mod名`（由用户根据前缀确定）
- **packageId**：`你的用户名.mod名`（全小写，无空格，用户自行填写）
- **作者名**：所有 `<author>`、作者字段必须由用户自行填写，AI 不预设作者信息

### 2. 版本注意

- **目标版本**：RimWorld 1.6
- **Unity 版本**：2022.3.35
- **.NET Framework**：4.7.2+
- 每个 reference 文件顶部标注适用版本和最后更新日期

### 3. 安全实践

- 始终在 `[StaticConstructorOnStartup]` 中初始化 Harmony
- Harmony patch 方法使用唯一的 patch ID
- ExposeData 中正确保存/加载自定义数据
- 避免在构造函数中做重型操作
- 优先使用 PatchOperations 而非直接修改原版文件

### 4. 法律边界

#### 完全合法

| 行为 | 依据 |
|------|------|
| 编写原创 XML Def | RimWorld 公开数据接口 |
| 用原版 ParentName 继承 | 等同于调用公开 API |
| 模仿原版 Def 结构 | 等同于参考 API 文档 |
| 用 dnSpy 查原版代码 | mod 社区标准做法 |
| 发布原创 mod 到 Steam Workshop | 官方支持的发布渠道 |

#### 绝对禁止

| 行为 | 原因 |
|------|------|
| 复制原版 C# 源码到你的 mod | 侵犯 Ludeon 版权 |
| 打包原版 DLL 到你的 mod | 侵犯版权 |
| 复制其他 mod 的代码/资产/贴图 | 侵犯原作者权利 |
| 使用第三方 IP（宝可梦、星战、漫威等）的角色/名称/资产 | 商标/版权侵权 |

#### 灰色地带（允许但有风险）

| 行为 | 建议 |
|------|------|
| 受其他游戏启发的武器/物品 | 合法——灵感不侵权，名称/设计要原创 |
| 反编译查看原版方法签名 | 行业惯例——只查 API，不抄实现 |
| 参考原版数值来平衡你的武器 | 参考值范围，不直接复制 |

### 5. AI 生成标注

每次生成 mod 后，必须在以下位置标注 AI 辅助生成：
- **About.xml 的 `<description>` 末尾** — 添加一行 `[AI 辅助生成]`
- **Steam Workshop 页面描述** — 标注"本 Mod 部分内容由 AI 辅助生成"
- **C# 代码文件头部注释** — 添加 `// AI 辅助生成`
- 不在 label、defName、游戏内图标等影响游玩体验的位置标注

### 6. 三层决策详解

#### ① 有模板 → 直接用

模板内的字段名、枚举值、ParentName 继承链、Comps 配置均已在编写时对照原版验证。直接使用模板生成，仅替换 `<Your...>` 占位符即可。

#### ② 报错/调试 → 查原版查原因

当用户报告游戏内错误时，用 grep 搜索原版 Defs 目录或 dnSpy 反编译 C# 查找根本原因。

**grep 兜底搜索**：
```bash
# 搜索字段的所有用法
grep -r "extraMeleeDamages" "<RimWorld>/Data/Core/Defs/"
# 搜索 ParentName 定义
grep -r 'Name="BaseMeleeWeapon"' "<RimWorld>/Data/Core/Defs/"
```

#### ③ 无模板的新类型 → 查原版 → 写 → 存模板

遇到模板未覆盖的 Def 类型，完整流程如下：
1. 查对照表，确认当前无模板
2. 在原版 `RimWorld/Data/Core/Defs/` 中搜索类似 Def
3. 根据原版结构写出完整 Def + 中文注释（遵循模板格式规范）
4. **Write 到 `templates/<新类型>.xml`**（必须写入文件系统）
5. **Edit SKILL.md 对照表**：添加新行
6. 下次遇到同类请求直接免查原版

---

## 源码查阅工具

### 优先级

```
① 有模板 → 直接用模板 → 不调任何外部工具
② 无模板 → grep > dnSpy
③ 有错误 → grep > dnSpy
```

### grep（查原版 Def 结构——第一选择）

```bash
# 搜索字段的所有用法
grep -r "extraMeleeDamages" "<RimWorld>/Data/Core/Defs/"
# 搜索 ParentName 定义
grep -r 'Name="BaseMeleeWeapon"' "<RimWorld>/Data/Core/Defs/"
# 搜索所有 techLevel 取值
grep -rh "techLevel" "<RimWorld>/Data/Core/Defs/" | sort -u
```

### dnSpy（反编译 C# 源码——第二选择）

无网络时用 dnSpy 打开 `<RimWorld>/RimWorldWin64_Data/Managed/Assembly-CSharp.dll`，浏览命名空间和类。

---

## 资源链接

- [RimWorld Wiki Modding Tutorials](https://rimworldwiki.com/wiki/Modding_Tutorials)
- [RimWorld 中文维基](https://rimworld.huijiwiki.com)
- [Harmony 官方文档](https://github.com/pardeike/Harmony/wiki)
- [RimWorld Mod Template (VS)](https://github.com/truemogician/RimWorld-Mod-Template)

---

## 子文件索引

### 知识参考 (references/)

1. `references/01-environment.md` — 环境搭建
2. `references/02-project-structure.md` — Mod 项目结构（About.xml + LoadFolders.xml）
3. `references/03-xml-defs.md` — XML Def 系统（16 种 Def 类型 + 继承机制）
4. `references/04-xml-patching.md` — XML PatchOperations（8 种操作 + XPath）
5. `references/05-csharp-basics.md` — C# Mod 开发（入口/Settings/Comp/序列化）
6. `references/06-harmony.md` — Harmony 补丁（Prefix/Postfix/Transpiler）
7. `references/07-assets.md` — 资源制作（贴图/着色器/Mask/音效/翻译）
8. `references/08-debugging.md` — 调试与排错
9. `references/09-workshop.md` — Steam Workshop 发布
10. `references/10-api-reference.md` — API 速查表
11. `references/frameworks.md` — 框架库清单 + NuGet 包 + DLC packageId

### 代码模板 (templates/)

- `templates/weapon-melee.xml` — 近战武器 Def
- `templates/weapon-ranged.xml` — 远程武器 Def
- `templates/apparel.xml` — 服装/护甲 Def
- `templates/resource-stuff.xml` — 原材料/建筑材料 Def
- `templates/building.xml` — 建筑 Def
- `templates/recipe.xml` — 配方 Def
- `templates/harmony-patch.cs` — Harmony 补丁骨架
- `templates/thingcomp.cs` — ThingComp 骨架
- `templates/requirements-template.md` — 批量需求文件格式模板

### 错误学习 (learnings/)

- `learnings/errors.txt` — 历史错误记录（每次加载 Skill 时自动读取）

### 工作流 (workflows/)

- `workflows/new-mod.md` — 从零创建 mod（测试版先行）
- `workflows/formalize-mod.md` — 测试通过后正规化 + 可选发布
- `workflows/debug-crash.md` — 崩溃排查
- `workflows/add-item.md` — 添加物品
- `workflows/add-building.md` — 添加建筑
- `workflows/batch-process.md` — 批量处理需求清单
- `workflows/patch-vanilla.md` — 修改原版
