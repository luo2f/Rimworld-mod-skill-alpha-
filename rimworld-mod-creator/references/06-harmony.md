# 06 - Harmony 补丁

> **适用版本**: RimWorld 1.6 | **最后更新**: 2026-07-28

本文档讲解 Harmony 运行时补丁：如何在不修改游戏原始 DLL 的前提下改变方法行为，涵盖基础架构、三种补丁类型、特殊参数、实战模式、条件 Patch 与调试。

---

## 一、概述

Harmony 是 .NET 运行时方法插桩库。RimWorld 内置 Harmony，模组可借此在游戏方法执行前后注入自己的逻辑，而无需修改游戏原始 DLL。XML Patch 改的是「数据」，Harmony Patch 改的是「行为」。

---

## 二、基础架构

### 2.1 入口类

用一个 `[StaticConstructorOnStartup]` 标记的类作为补丁应用入口（也可在 `Mod` 构造函数中应用）：

```csharp
using Verse;
using HarmonyLib;

namespace <YourPrefix>.YourMod
{
    [StaticConstructorOnStartup]
    public static class HarmonyMain
    {
        public static readonly Harmony harmony = new Harmony("<YourPrefix>.yourmod");

        static HarmonyMain()
        {
            harmony.PatchAll();
            Log.Message("[YourMod] Harmony patches applied.");
        }
    }
}
```

### 2.2 三要素

- **Harmony ID**：`new Harmony("<YourPrefix>.yourmod")`，全局唯一字符串。
- **PatchAll()**：扫描程序集内所有带 `[HarmonyPatch]` 特性的方法并应用。
- **补丁方法**：用特性标注目标方法与补丁类型。

---

## 三、三种补丁类型详解

### 3.1 Prefix（前置拦截）

在原方法**执行前**运行。可读取/修改参数，可决定是否跳过原方法。

```csharp
// 目标：在 RecipeDef.MakeProductProducts 之前拦截，可跳过原方法
[HarmonyPatch(typeof(RecipeDef), "MakeProductProducts")]
public static class Patch_MakeProductProducts
{
    // 返回 false 则跳过原方法；返回 true（或 void）则继续执行原方法
    static bool Prefix(RecipeDef __instance, ref ThingDef stuffDef)
    {
        // 可修改 ref 参数：把材料换成自定义
        if (__instance.defName == "<YourPrefix>_SpecialRecipe")
        {
            stuffDef = ThingDefOf.Steel;
        }
        // 返回 true 继续原方法
        return true;
    }
}
```

**关键点**：
- 返回 `bool`：`false` 跳过原方法，`true` 执行原方法。
- 返回 `void`：原方法照常执行。
- 用 `ref` 修饰参数可在原方法执行前改写其值。

### 3.2 Postfix（后置拦截）

在原方法**执行后**运行。可读取并修改返回值。

```csharp
[HarmonyPatch(typeof(Pawn), "GetGizmos")]
public static class Patch_GetGizmos
{
    // __result 是原方法返回值（引用类型用 ref，值类型用 ref）
    static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
    {
        // 给小人追加一个自定义按钮
        var list = __result.ToList();
        list.Add(new Command_Action
        {
            defaultLabel = "Custom",
            action = () => Log.Message("clicked " + __instance)
        });
        __result = list;
    }
}
```

**关键点**：
- `ref __result` 可改写返回值。
- 不返回值（void）时仅读取，不改写。

### 3.3 Transpiler（IL 级别修改）

在中间语言（IL）层面改写方法，功能最强但最复杂，适合精确修改方法内部指令。

```csharp
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

[HarmonyPatch(typeof(SomeClass), "SomeMethod")]
public static class Patch_Transpiler
{
    // 接收原方法 IL 指令序列，返回修改后的序列
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var code in instructions)
        {
            // 示例：把某常量 10 替换为 20
            if (code.opcode == OpCodes.Ldc_I4_S && (sbyte)code.operand == 10)
            {
                yield return new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)20);
            }
            else
            {
                yield return code;
            }
        }
    }
}
```

> Transpiler 调试困难、易随游戏更新失效，非必要不使用。优先用 Prefix/Postfix 解决问题。

---

## 四、特殊参数速查表

Harmony 通过约定参数名自动注入对应值：

| 参数名 | 含义 | 可修饰 |
|--------|------|--------|
| `__instance` | 被补丁方法的 this（实例方法可用） | `ref`（改 this 引用，少用） |
| `__result` | 原方法返回值（Postfix/Transpiler） | `ref` |
| `__state` | 在 Prefix 存值，Postfix 取值（需成对） | `ref` |
| `___fieldName` | 读取原类的私有/实例字段（三个下划线） | `ref` |
| `__args` | 原方法全部参数数组 | `ref` |
| `与原方法同名的参数` | 直接绑定原方法对应参数 | `ref` |

### __state 用法（Prefix 存、Postfix 取）

```csharp
[HarmonyPatch(typeof(SomeClass), "SomeMethod")]
public static class Patch_StateExample
{
    static void Prefix(out float __state)
    {
        __state = SomeGlobal.Value;  // 记录前置状态
    }

    static void Postfix(float __state)
    {
        // 用 __state 对比前后变化
        Log.Message("Changed by: " + (SomeGlobal.Value - __state));
    }
}
```

---

## 五、常见实战模式

### 5.1 修改配方产出

```csharp
// 让某配方额外产出一个物品
[HarmonyPatch(typeof(GenRecipe), "PostProcessProduct")]
public static class Patch_ExtraProduct
{
    static void Postfix(Thing product, RecipeDef recipeDef)
    {
        if (recipeDef.defName == "<YourPrefix>_BonusRecipe")
        {
            // 在产出物旁生成额外物品（伪代码）
            Log.Message("Bonus product for " + product);
        }
    }
}
```

### 5.2 修改 AI 行为

```csharp
// 拦截 ThinkNode，改变小人决策
[HarmonyPatch(typeof(ThinkNode_JoinVoluntarily), "TryIssueJobPackage")]
public static class Patch_AIJoin
{
    static bool Prefix(Pawn pawn)
    {
        if (pawn.health.hediffSet.HasHediff(<YourPrefix>_FearHediffDef))
        {
            return false; // 恐惧状态下不加入战斗
        }
        return true;
    }
}
```

### 5.3 拦截事件触发

```csharp
[HarmonyPatch(typeof(IncidentWorker), "TryExecute")]
public static class Patch_Incident
{
    static void Prefix(IncidentWorker __instance, ref bool __result)
    {
        // 在事件执行前后记录或修改
    }
}
```

---

## 六、条件 Patch（Prepare 方法）

`Prepare` 方法在补丁应用前执行，返回 `false` 则不应用该补丁。常用于按 DLC/模组/版本条件启用。

```csharp
[HarmonyPatch(typeof(SomeRoyaltyClass), "SomeMethod")]
public static class Patch_RoyaltyOnly
{
    // 仅当 Royalty DLC 启用时才应用此补丁
    static bool Prepare()
    {
        return ModsConfig.RoyaltyActive;
    }

    static void Postfix()
    {
        // ...
    }
}
```

> `Prepare` 也可返回动态结果；若返回 `false`，Prefix/Postfix 都不会注入。

---

## 七、Harmony ID 冲突避免

- 每个模组的 Harmony ID 必须**全局唯一**，建议用 packageId 同名（如 `<YourPrefix>.yourmod`）。
- 重复 ID 会导致补丁混乱、卸载异常。
- 查看已注册补丁：开发者模式日志中可见 Harmony 输出，或用 `Harmony.GetPatchedMethods()` 调试。

---

## 八、调试 Harmony 补丁

1. **日志确认**：`PatchAll()` 后日志会列出注入的方法，确认目标方法被命中。
2. **`Log.Message` 打点**：在 Prefix/Postfix 首行加日志，确认是否被执行。
3. **断点调试**：用 dnSpyEx 附加到 RimWorld 进程，在补丁方法设断点。
4. **目标方法签名核对**：用 dnSpyEx 反编译确认方法名、参数类型与数量与补丁特性一致。
5. **泛型/重载**：目标方法有重载时，需在 `[HarmonyPatch]` 中显式指定参数类型：
   ```csharp
   [HarmonyPatch(typeof(Thing), "TakeDamage",
       new System.Type[] { typeof(DamageInfo) })]
   ```
6. **Patch 失败排查**：日志搜索 `Harmony` 与 `Exception`，常见原因有方法名拼写错、参数不匹配、目标为泛型方法未指定。

---

## 九、与 XML Patch 对比表

| 对比项 | XML Patch（PatchOperation） | Harmony Patch |
|--------|----------------------------|---------------|
| 修改对象 | Def 数据 | 方法行为 |
| 时机 | Def 加载后 | 运行时方法调用 |
| 需要 C# | 否 | 是 |
| 灵活度 | 数据增删改 | 任意逻辑拦截 |
| 性能开销 | 几乎无 | 有（方法插桩） |
| 兼容风险 | 低（数据层面） | 较高（依赖方法签名） |
| 适用场景 | 改属性/配方/列表 | 改逻辑/算法/事件 |

> 原则：能用 XML Patch 解决的不用 Harmony；必须改行为时才用 Harmony，并尽量用条件 Patch 限定范围、用 `Prepare` 做运行时保护。
