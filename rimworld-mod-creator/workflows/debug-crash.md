# 崩溃排查

> **适用版本**: RimWorld 1.6 | **最后更新**: 2026-07-28

## 概述

本工作流用于排查 RimWorld 中的游戏崩溃、红字报错、白窗/灰窗等问题。通过定位错误信息、判断错误类型、隔离测试、修复并记录的完整流程，系统性地解决 Mod 相关问题。

**排查原则**：先定位 → 再判断 → 后隔离 → 最后修复

---

## 第一步：定位错误信息

### Player.log 位置

Player.log 是 RimWorld 的运行日志，记录了所有错误和警告信息。

| 操作系统 | Player.log 路径 |
|---------|---------------|
| Windows | `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Player.log` |
| macOS | `~/Library/Logs/Unity/Player.log` |
| Linux | `~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Player.log` |

### 搜索 Mod 前缀

在 Player.log 中搜索你的 Mod 相关错误：

```bash
# 搜索你的defName前缀
grep "你的前缀" "Player.log路径"
# 搜索XML相关错误
grep -i "xml" "Player.log路径" | grep -i "error"
# 搜索defName相关错误
grep -i "defName" "Player.log路径" | grep -i "error"
```

### 快速定位方法

1. 打开 Player.log
2. 搜索 `Error` 或 `Exception` 关键词
3. 查看错误信息中是否包含你的 Mod 名称、defName 前缀或文件名
4. 记录错误类型、涉及的 Def 名称、错误详情

---

## 第二步：判断错误类型

### 红字错误

游戏左下角显示的红色错误信息，不影响游戏启动但功能异常。

| 错误信息 | 可能原因 | 解决方法 |
|---------|---------|---------|
| Could not resolve cross-reference | defName 拼写错误，引用了不存在的 Def | 检查 defName 拼写，确认被引用的 Def 存在 |
| Could not find type named xxx | C# 类未找到，DLL 未加载或类名错误 | 检查 CompProperties 的 Class 属性，确认 DLL 已正确打包 |
| XML parse error | XML 标签未闭合、语法错误 | 检查标签闭合、属性引号、编码声明 |
| Duplicate defName | defName 重复，前缀不唯一 | 检查所有 defName，确保前缀唯一 |
| Exception while parsing XML | XML 文件格式问题 | 检查文件编码（应为 UTF-8）、特殊字符转义 |
| Could not load texture | 贴图路径错误或文件缺失 | 检查 texPath 路径、文件是否存在、文件名大小写 |

### 白窗/灰窗

游戏无法正常启动，显示白屏或灰屏。

| 错误现象 | 可能原因 | 解决方法 |
|---------|---------|---------|
| 白窗（启动后白屏） | Mod 冲突，多个 Mod 修改了同一内容 | 二分法排查（见第三步） |
| 灰窗（启动后灰屏） | XML 解析错误，Def 加载失败 | 检查最后修改的 XML 文件，验证 XML 语法 |
| 启动时崩溃 | DLL 依赖缺失，引用了不存在的类 | 检查 C# 代码引用的命名空间和类是否正确 |
| 加载存档时崩溃 | 存档数据与 Mod 不兼容 | 检查 ExposeData 序列化逻辑，确认数据结构 |

### NullReferenceException

空引用异常，代码中访问了 null 对象。

| 错误现象 | 可能原因 | 解决方法 |
|---------|---------|---------|
| NullReferenceException | 代码中访问了 null 对象 | 检查 null 判断，在使用对象前确认非 null |
| NPE in Tick | 游戏循环中访问了 null | 检查 Tick 相关方法中的对象引用 |
| NPE in Draw | 绘制时贴图或图形为 null | 检查 graphicData 配置、贴图路径 |
| NPE in ExposeData | 存档加载时数据为 null | 检查序列化字段的默认值和加载逻辑 |

---

## 第三步：隔离测试

当不确定是哪个 Mod 导致问题时，使用隔离测试：

### 二分法排查

1. **只加载最小 Mod 组合**：Core + 你的 Mod（+ Harmony 如需要）
2. **测试是否复现错误**：
   - 如果错误消失 → 问题由其他 Mod 冲突导致，逐步添加其他 Mod 定位冲突源
   - 如果错误仍存在 → 问题在本 Mod 中
3. **如果确认是本 Mod 问题**：
   - 逐个禁用本 Mod 的 Def 文件，定位是哪个文件导致错误
   - 检查最近修改的文件，重点排查

### 隔离测试步骤

```
1. 关闭所有 Mod，只留 Core
   → 正常？ → 继续
   → 异常？ → RimWorld 本身问题，重装游戏

2. 添加你的 Mod（Core + 你的Mod）
   → 正常？ → 问题由 Mod 冲突导致，逐个添加其他Mod定位
   → 异常？ → 问题在本Mod中，继续排查

3. 在本Mod中逐个禁用Def文件
   → 定位到具体文件后，检查该文件的XML语法和内容
```

### 贴图问题隔离

如果贴图不显示：
1. 检查 texPath 路径是否正确
2. 检查文件是否存在（注意大小写）
3. 检查 PNG 文件是否有效
4. 检查文件是否在正确的 Textures/ 子目录下

---

## 第四步：修复并记录

### 修复

根据第二步判断的错误类型和第三步隔离的结果，修复对应问题：

- **XML 语法错误**：修正标签闭合、属性引号、编码声明
- **defName 拼写错误**：修正拼写，确保引用一致
- **贴图路径错误**：修正 texPath 或放置贴图到正确位置
- **C# 空引用**：添加 null 检查，确认对象已初始化
- **Mod 冲突**：调整 loadAfter / loadBefore 顺序，或修改 Patch 的 XPath

### 记录到 learnings/errors.txt

修复完成后，必须将关键教训总结为一句话追加到 `learnings/errors.txt`：

```
YYYY-MM-DD | <类别> | <一句话总结>
```

类别：
- `XML`：ThingDef/RecipeDef/Def 属性写错、枚举值错误、ParentName 错误
- `C#`：编译错误、空引用、类型错误、DLL 加载问题
- `Harmony`：Patch ID 冲突、补丁未生效、Prefix/Postfix 签名错误
- `Path`：文件路径错误、资源缺失、加载顺序问题
- `Other`：上述类别之外的错误

示例：
```
2026-07-28 | XML | defName引用了不存在的ParentName，导致cross-reference解析失败
2026-07-28 | Path | texPath路径大小写不一致导致贴图无法加载
```

> 每次加载 Skill 时会自动读取此文件，避免重复犯错。

---

## 常见错误速查表

| 错误信息 | 可能原因 | 解决方法 |
|---------|---------|---------|
| `Could not resolve cross-reference` | defName 拼写错误 | 检查 defName 拼写，确认引用的 Def 存在 |
| `Could not find type named` | C# 类名错误或 DLL 缺失 | 检查 Class 属性，确认 DLL 已打包 |
| `XML parse error: unclosed tag` | XML 标签未闭合 | 检查标签是否正确闭合 |
| `XML parse error: attribute` | XML 属性引号缺失 | 检查属性值是否用引号包裹 |
| `Duplicate defName` | defName 重复 | 确保所有 defName 使用唯一前缀 |
| `Could not load texture` | 贴图路径错误 | 检查 texPath、文件名大小写、文件格式 |
| `NullReferenceException` | 空引用 | 添加 null 检查 |
| `KeyNotFoundException` | 字典中找不到键 | 检查键是否存在，添加默认值处理 |
| `InvalidOperationException` | 非法操作 | 检查操作前置条件 |
| `Patch operation failed` | Patch XPath 错误 | 检查 XPath 路径是否正确匹配到目标节点 |
| `Exception in CompTick` | Comp 的 Tick 方法出错 | 检查 Comp 代码中的引用 |
| `Exception generating` | 生成物品时出错 | 检查 Def 的必填字段是否完整 |
| `Exception while drawing` | 绘制时出错 | 检查 graphicData 和贴图配置 |
| `Cannot convert` | 类型转换失败 | 检查字段值类型是否正确（数值、字符串、枚举） |
| `Failed to find texture` | 贴图文件缺失 | 确认贴图文件存在于指定路径 |

---

## 排查流程总结

```
收到错误报告
  │
  ├─ 第一步：定位错误信息
  │   └─ 打开 Player.log → 搜索 Mod 前缀 → 记录错误
  │
  ├─ 第二步：判断错误类型
  │   ├─ 红字错误 → 检查 defName / Class / XML 语法
  │   ├─ 白窗/灰窗 → 二分法排查 Mod 冲突
  │   └─ NullReferenceException → 检查 null 判断
  │
  ├─ 第三步：隔离测试
  │   └─ 只加载 Core + 你的 Mod → 确认问题来源
  │
  ├─ 第四步：修复并记录
  │   ├─ 根据错误类型修复
  │   └─ 追加到 learnings/errors.txt
  │
  └─ 验证修复
      └─ 重新进游戏测试 → 确认错误已解决
```

---

## 相关文档

- 调试与排错详解：`references/08-debugging.md`
- XML PatchOperations：`references/04-xml-patching.md`
- C# 开发基础：`references/05-csharp-basics.md`
- Harmony 补丁：`references/06-harmony.md`
- 历史错误记录：`learnings/errors.txt`
