# 08 - 调试与排错

> **适用版本**: RimWorld 1.6 | **最后更新**: 2026-07-28

本文档讲解 RimWorld 模组调试：开发者模式、日志文件位置、日志解读、常见错误速查、隔离测试与快速重载。

---

## 一、开发者模式

### 启用方式

主菜单 → 选项 → 进入游戏后顶部栏 → 勾选「Development mode（开发者模式）」。

### 常用开发工具（屏幕顶部工具栏）

| 工具 | 功能 | 用途 |
|------|------|------|
| **Spawn Thing** | 生成任意物品/生物 | 快速获取测试物品 |
| **Execute Incident** | 触发事件 | 测试事件逻辑、袭击 |
| **Apply Damage** | 对选中目标施加伤害 | 测试护甲、伤害计算 |
| **Give Hediff** | 给 Pawn 添加健康状态 | 测试 Hediff 效果 |
| **God Mode** | 神模式（免费建造/制作） | 快速搭建测试场景 |
| **Log Window** | 打开日志窗口 | 实时查看报错 |
| **Debug Inspector** | 检查器 | 查看选中对象的内部字段 |
| **Edit Think Tree** | 编辑思维树 | 调试 AI |
| **Dev Palette** | 开发命令面板 | 各类调试操作 |

> 日志窗口（Log Window）是排查问题的第一工具，红字错误会高亮显示。

---

## 二、Player.log 文件位置

游戏运行日志写入 `Player.log`，记录了加载、Def 解析、Patch 应用、运行时异常等全部信息。

| 操作系统 | 路径 |
|----------|------|
| **Windows** | `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Player.log` |
| **Linux** | `~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Player.log` |
| **macOS** | `~/Library/Logs/Ludeon Studios/RimWorld by Ludeon Studios/Player.log` |

> Windows 快速打开：在资源管理器地址栏粘贴 `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\` 回车。

每次启动游戏会覆盖旧日志，排查时建议先删除/备份旧日志再启动复现。

---

## 三、日志解读与自定义日志输出

### 自定义日志输出

```csharp
using Verse;

Log.Message("普通信息");        // 白色
Log.Warning("警告信息");        // 黄色
Log.Error("错误信息");          // 红色（不中断游戏）
Log.ErrorOnce("只报一次的错", 12345);  // 同一 ID 只报一次，避免刷屏
```

### 日志关键词解读

- `Could not resolve cross-reference`：Def 引用失败（见下表）。
- `Could not find type`：C# 类型未找到（DLL 未加载或类名错误）。
- `XML error`：XML 语法错误。
- `Duplicate defName`：defName 重复。
- `[Harmony]`：Harmony 补丁相关信息。
- `Exception`：运行时异常，附堆栈。

---

## 四、常见错误速查表

### 4.1 红字错误

| 错误信息 | 原因 | 解决 |
|----------|------|------|
| `Could not resolve cross-reference to ThingDef with defName XXX` | 引用了不存在的 defName | 核对拼写、前缀、加载顺序（`loadAfter`） |
| `Could not find type named XXX` | C# 类未找到 | 确认 DLL 已放入 `Assemblies/`、命名空间正确、类名与 XML 的 `Class` 一致 |
| `XML error: <XXX> doesn't correspond to anything` | XML 标签拼写错或字段名错 | 对照字段名，注意大小写 |
| `Duplicate defName: XXX` | defName 重复 | 加唯一前缀，删除重复定义 |
| `Tried to use null Thing` | 空引用对象 | 检查 Def 是否加载成功、对象是否生成 |
| `Index out of bounds` | 数组/列表越界 | 检查集合边界 |

### 4.2 白窗 / 灰窗排查

- **白窗（全白卡住）**：通常是某个 C# 模组在加载早期抛出未捕获异常导致渲染中断。隔离测试定位是哪个模组。
- **灰窗/灰屏**：可能是贴图加载失败或渲染崩溃，检查贴图路径、尺寸、格式。

### 4.3 NullReferenceException 安全写法对比

```csharp
// 危险：未判空，pawn/health/hediffSet 任一为 null 即崩溃
bool hasHediff = pawn.health.hediffSet.HasHediff(someDef);

// 安全：逐步判空
bool hasHediff = pawn?.health?.hediffSet?.HasHediff(someDef) ?? false;
```

```csharp
// 危险：Def 可能未加载
var def = DefDatabase<ThingDef>.GetNamed("SomeDef");
def.stuffCategories.Clear();  // def 为 null 时崩溃

// 安全：先判存在
var def = DefDatabase<ThingDef>.GetNamedSilentFail("SomeDef");
if (def != null)
{
    def.stuffCategories.Clear();
}
```

> 模组代码应大量使用 `?.` 与 `GetNamedSilentFail`，避免因其他模组缺失目标而崩溃。

---

## 五、隔离测试流程

当问题难以定位时，用最小模组集复现：

1. 复制一份 RimWorld 配置，或新建测试配置。
2. 仅加载：**Core + Harmony + 你的模组**（加必要 DLC）。
3. 复现问题：
   - 若仍出现 → 问题在你的模组本身，逐步禁用你的子模块定位。
   - 若消失 → 问题由其他模组交互引起，分批加回模组，二分法定位冲突项。
4. 定位到具体模组后，检查加载顺序（`loadAfter`/`loadBefore`）与 Patch 冲突。

> 工具辅助：RimSort 的冲突检测可快速发现相互 Patch 同一字段的模组。

---

## 六、开发中快速重载

| 改动类型 | 是否需重启游戏 | 说明 |
|----------|----------------|------|
| XML Def 文件 | 否（需重载） | 用开发者模式「Reload defs」或重启较快 |
| XML Patch 文件 | 是（建议） | Patch 在加载时应用，通常需重启 |
| 贴图 / 音效 | 视情况 | 部分可热重载，建议重启 |
| C# 代码 | **是（必须）** | 需重新编译 DLL 并重启游戏 |

### C# 快速迭代技巧

- 配置 .csproj 的 PostBuild 自动拷贝 DLL 到 `Assemblies/`，编译后直接重启游戏。
- 减少重启成本：把频繁改动的逻辑写成数据驱动（XML/设置），减少 C# 改动。
- 用 dnSpyEx 附加调试时，可热修部分方法（编辑 IL 后保存），加速调试。

---

## 七、常见问题 FAQ

**Q：日志里有一堆红字，但游戏能玩，要管吗？**
A：需要管。红字意味着某处逻辑异常，可能在特定条件下崩溃。至少确认不是你的模组产生的。

**Q：我的 C# 代码改了没生效？**
A：C# 必须重新编译并重启游戏。确认 PostBuild 已拷贝最新 DLL，且游戏完全重启（不是回主菜单）。

**Q：`Could not find type` 但我确认 DLL 在 `Assemblies/` 里？**
A：检查：① DLL 是否编译成功（看编译输出）；② 命名空间与类名是否与 XML 的 `Class` 完全一致（含大小写）；③ 目标框架是否为 net472；④ DLL 是否依赖了未加载的其他 DLL。

**Q：开发者模式的日志窗口和 Player.log 内容一样吗？**
A：基本一致。日志窗口是实时显示的子集，Player.log 是完整文件。完整排查建议看 Player.log。

**Q：怎么知道我的 Patch 是否应用成功？**
A：日志中搜索你的 Patch xpath 或 `PatchOperation`；若失败会有 `Could not apply` 提示。也可在游戏内检查目标 Def 是否已变化。

**Q：报错堆栈里看不到我的代码？**
A：你的代码可能被 Harmony 注入到游戏方法中，堆栈显示的是游戏方法名。结合 `Log.Message` 打点确认你的补丁是否执行。
