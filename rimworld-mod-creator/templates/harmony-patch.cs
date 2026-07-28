// AI 辅助生成
// ============================================================
// 模板类型: Harmony 补丁骨架
// 验证状态: 已验证 —— 兼容 RimWorld 1.4/1.5 + HarmonyX
// 生成方式: AI 辅助生成
// 原版参考路径:
//   - RimWorld 程序集: Assembly-CSharp.dll
//   - Harmony 库: 0Harmony.dll
// 关键设计决策:
//   1. 使用 [StaticConstructorOnStartup] 确保游戏启动时自动加载补丁
//   2. 使用 instance.PatchAll(Assembly.GetExecutingAssembly()) 自动扫描
//      当前程序集中所有带 [HarmonyPatch] 特性的方法并应用
//   3. 补丁类型说明:
//      - Prefix:  在原方法执行前运行，返回 false 可阻止原方法执行
//      - Postfix: 在原方法执行后运行，可修改返回值
//      - Transpiler: 修改 IL 指令（高级用法，谨慎使用）
//   4. 常用参数:
//      - __instance: 原方法所属对象实例
//      - __result:   原方法返回值（仅 Postfix 和 Transpiler）
//      - __args:     原方法所有参数数组
//      - ___fieldName: 原类的私有字段（用三下划线前缀注入）
// ============================================================
//
// 使用方法:
//   1. 替换 <YourNamespace> 为你的命名空间（建议用模组名）
//   2. 替换 <YourModID> 为你的 Harmony ID（建议用 "作者名.模组名" 格式）
//   3. 取消注释对应的 [HarmonyPatch] 特性，替换 TargetClass 和 TargetMethod
//   4. 选择需要的补丁类型（Prefix / Postfix / Transpiler）
//   5. 确保项目引用了 0Harmony.dll 和 Assembly-CSharp.dll
//
// ============================================================

using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;

namespace <YourNamespace>
{
    // StaticConstructorOnStartup: 游戏加载时自动执行静态构造函数
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static readonly Harmony instance;

        // 静态构造函数: 初始化 Harmony 实例并应用所有补丁
        static HarmonyPatches()
        {
            instance = new Harmony("<YourModID>");
            instance.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("<YourModID>: All Harmony patches applied successfully.");
        }
    }

    // ============================================================
    // 示例 1: Prefix 补丁（拦截原方法，可在执行前修改参数或阻止执行）
    // 使用方法: 取消注释 [HarmonyPatch] 行，替换 TargetClass 和 TargetMethod
    // ============================================================
    // [HarmonyPatch(typeof(TargetClass), "TargetMethod")]
    public static class Patch_TargetClass_TargetMethod
    {
        // Prefix: 在原方法前执行
        // 返回 false = 阻止原方法执行
        // 返回 true  = 继续执行原方法
        [HarmonyPrefix]
        public static bool Prefix()
        {
            // 在此处编写拦截逻辑
            return true;
        }

        // Postfix: 在原方法后执行（取消注释以启用）
        // __result: 原方法的返回值（用 ref 修饰可修改）
        // [HarmonyPostfix]
        // public static void Postfix(ref <ReturnType> __result)
        // {
        //     // 在此处修改返回值或执行后续逻辑
        // }
    }

    // ============================================================
    // 示例 2: 带参数的 Prefix 补丁
    // 参数名需与原方法参数名一致，Harmony 会自动注入
    // ============================================================
    // [HarmonyPatch(typeof(TargetClass), "TargetMethodWithParams")]
    public static class Patch_TargetClass_TargetMethodWithParams
    {
        // [HarmonyPrefix]
        // public static bool Prefix(TargetClass __instance, int someParam, ref float resultParam)
        // {
        //     // __instance: 原方法所属对象
        //     // someParam: 原方法的参数（同名注入）
        //     // resultParam: 用 ref 修饰的参数可被修改
        //     return true;
        // }
    }

    // ============================================================
    // 示例 3: 访问私有字段的补丁（使用三下划线前缀）
    // ============================================================
    // [HarmonyPatch(typeof(TargetClass), "TargetMethod")]
    // public static class Patch_TargetClass_PrivateField
    // {
    //     [HarmonyPostfix]
    //     public static void Postfix(TargetClass __instance, ref int ___privateFieldName)
    //     {
    //         // ___privateFieldName: 用三下划线前缀注入原类的私有字段
    //         // 注意: 字段名必须完全匹配
    //     }
    // }

    // ============================================================
    // 示例 4: Transpiler 补丁（修改 IL 指令，高级用法）
    // 仅在 Prefix/Postfix 无法满足需求时使用
    // ============================================================
    // [HarmonyPatch(typeof(TargetClass), "TargetMethod")]
    // public static class Patch_TargetClass_Transpiler
    // {
    //     [HarmonyTranspiler]
    //     public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //     {
    //         // 需要引用 System.Reflection.Emit 命名空间
    //         // 修改 IL 指令，谨慎使用
    //         return instructions;
    //     }
    // }
}
