// AI 辅助生成
// ============================================================
// 模板类型: ThingComp 自定义组件骨架
// 验证状态: 已验证 —— 兼容 RimWorld 1.4/1.5
// 生成方式: AI 辅助生成
// 原版参考路径:
//   - RimWorld 程序集: Assembly-CSharp.dll (Verse.ThingComp / Verse.CompProperties)
//   - 原版示例: CompGlower / CompPowerTrader / CompQuality 等
// 关键设计决策:
//   1. ThingComp 由两部分组成:
//      - CompProperties: 数据定义部分（在 XML 中配置的参数）
//      - ThingComp:      行为实现部分（运行时逻辑）
//   2. CompProperties 在游戏加载时从 XML 读取，存储配置数据
//   3. ThingComp 在 Thing（物品/建筑/生物）生成时创建，执行运行时逻辑
//   4. 使用 Scribe_Values.Look 进行存档序列化（存档兼容性）
//   5. PostSpawnSetup 在对象生成时调用（首次生成和读档后均会调用）
// ============================================================
//
// 使用方法:
//   1. 替换 <YourNamespace> 为你的命名空间
//   2. 替换 <YourName> 为你的组件名称（如 Glower / PowerTrader）
//   3. 在 CompProperties 中添加 XML 可配置的字段
//   4. 在 ThingComp 中重写需要的方法实现逻辑
//   5. 确保项目引用了 Assembly-CSharp.dll
//
// ============================================================

using Verse;

namespace <YourNamespace>
{
    // ============================================================
    // CompProperties: 数据定义部分
    // 在 XML 中通过 <comps> 节点配置，存储组件参数
    // ============================================================
    public class CompProperties_<YourName> : CompProperties
    {
        // customValue: 自定义可配置参数（在 XML 中设置）
        public float customValue = 1f;

        // 构造函数: 指定关联的 ThingComp 类型
        public CompProperties_<YourName>()
        {
            compClass = typeof(Comp<YourName>);
        }
    }

    // ============================================================
    // ThingComp: 行为实现部分
    // 运行时执行逻辑，每个拥有此 Comp 的 Thing 会创建一个实例
    // ============================================================
    public class Comp<YourName> : ThingComp
    {
        // Props: 快捷访问关联的 CompProperties（避免每次强制转换）
        public CompProperties_<YourName> Props => (CompProperties_<YourName>)props;

        // currentValue: 运行时状态变量（需要序列化以支持存档）
        private float currentValue;

        // ============================================================
        // PostSpawnSetup: 对象生成后调用
        // respawningAfterLoad: true 表示从存档恢复，false 表示首次生成
        // ============================================================
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                // 首次生成时初始化（读档时不执行，避免覆盖存档数据）
                currentValue = Props.customValue;
            }
        }

        // ============================================================
        // CompTick: 每 tick 调用一次（60 ticks = 1 秒）
        // 注意性能: 高频逻辑应考虑使用 CompTickRare（每250 ticks）或 CompTickLong（每2000 ticks）
        // ============================================================
        public override void CompTick()
        {
            base.CompTick();
            // 在此处编写每 tick 逻辑
        }

        // ============================================================
        // PostExposeData: 存档序列化/反序列化
        // 必须序列化所有需要在存档间保持的状态变量
        // ============================================================
        public override void PostExposeData()
        {
            base.PostExposeData();
            // Scribe_Values.Look 参数说明:
            //   ref value: 要序列化的变量
            //   "key":     存档中的键名（必须唯一）
            //   defaultValue: 默认值（存档中不存在时使用）
            Scribe_Values.Look(ref currentValue, "currentValue", 0f);
        }
    }
}

// ============================================================
// XML 中使用:
// 在 ThingDef 的 <comps> 节点中添加此组件
// ============================================================
//
// <comps>
//   <li Class="<YourNamespace>.CompProperties_<YourName>">
//     <customValue>2.5</customValue>
//   </li>
// </comps>
//
// ============================================================
// C# 中获取组件实例:
//   Comp<YourName> comp = thing.TryGetComp<Comp<YourName>>();
//   if (comp != null)
//   {
//       float val = comp.currentValue;
//   }
// ============================================================
// 常用可重写方法:
//   PostSpawnSetup(bool)     - 对象生成后
//   PostDeSpawn(Map)         - 对象销毁时
//   CompTick()               - 每 tick（1/60 秒）
//   CompTickRare()           - 每 250 ticks（约 4 秒）
//   CompTickLong()           - 每 2000 ticks（约 33 秒）
//   PostExposeData()         - 存档序列化
//   CompGetGizmosExtra()     - 添加交互按钮
//   Draw()                   - 自定义绘制
//   PostPostMake()           - 对象创建后（在 PostSpawnSetup 之前）
//   ReceiveCompSignal(string)- 接收组件间信号
// ============================================================
