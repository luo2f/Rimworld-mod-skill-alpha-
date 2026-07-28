# 05 - C# Mod 开发基础

> **适用版本**: RimWorld 1.6 | **最后更新**: 2026-07-28

本文档讲解 RimWorld C# 模组开发基础：项目文件配置、入口点、设置持久化、存档兼容、自定义组件、Component 体系、DefModExtension、自定义 Def 与跨版本兼容。

---

## 一、概述

当 XML 无法表达动态逻辑（如条件行为、随机事件、复杂计算、UI 交互）时，需用 C# 编写模组。C# 代码编译为 DLL 放入 `Assemblies/`，游戏启动时自动加载。

---

## 二、项目文件配置

### 2.1 SDK-style vs 旧式 csproj 对比

| 对比项 | 现代 SDK-style | 旧式 csproj |
|--------|---------------|-------------|
| 简洁度 | 极简，默认包含所有 .cs | 冗长，逐项列出文件 |
| NuGet | 原生 `<PackageReference>` | 需 packages.config |
| 引用 DLL | NuGet 自动 | 手动 `<Reference HintPath>` |
| 目标框架 | `<TargetFramework>` | `<TargetFrameworkVersion>` |
| 推荐度 | 推荐 | 兼容旧工具链时使用 |

### 2.2 现代 SDK-style .csproj 完整示例

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- 目标框架与游戏 Mono 运行时匹配 -->
    <TargetFramework>net472</TargetFramework>
    <AssemblyName><YourPrefix>.YourMod</AssemblyName>
    <RootNamespace><YourPrefix>.YourMod</RootNamespace>
    <LangVersion>latest</LangVersion>
    <!-- 不生成 pdb 以减少体积（可选） -->
    <DebugType>none</DebugType>
  </PropertyGroup>

  <!-- 游戏引用程序集 -->
  <ItemGroup>
    <PackageReference Include="Krafs.Rimworld.Ref" Version="1.6.*" />
  </ItemGroup>

  <!-- Harmony 运行时补丁库 -->
  <ItemGroup>
    <PackageReference Include="Lib.Harmony" Version="2.*" />
  </ItemGroup>

  <!-- Publicizer：公开游戏 internal 成员 -->
  <ItemGroup>
    <PackageReference Include="Krafs.Publicizer" Version="2.*" PrivateAssets="all" />
    <Publicize Include="Assembly-CSharp" />
  </ItemGroup>

  <!-- 编译后自动拷贝到模组 Assemblies 目录 -->
  <Target Name="PostBuild" AfterTargets="PostBuildEvent">
    <Copy SourceFiles="$(TargetPath)"
          DestinationFolder="$(SolutionDir)..\Assemblies\" />
  </Target>

</Project>
```

### 2.3 Publicizer 精确控制用法

`Krafs.Publicizer` 默认公开整个 `Assembly-CSharp`。可精确控制只公开需要的类型，减小影响面：

```xml
<ItemGroup>
  <Publicize Include="Assembly-CSharp" />
</ItemGroup>

<!-- 仅公开指定类型（更安全） -->
<ItemGroup>
  <Publicize Include="Assembly-CSharp::RimWorld.SomeInternalClass" />
  <Publicize Include="Assembly-CSharp::Verse.AnotherClass" />
</ItemGroup>
```

> 使用 Publicizer 后，原本 `internal`/`private` 的成员在编译期变为可访问，但运行时仍是原可见性，请确保仅用于读取，避免破坏封装。

### 2.4 旧式 csproj 直引本地 DLL

不便使用 NuGet 时，可手动引用游戏目录下的 DLL：

```xml
<Reference Include="Assembly-CSharp">
  <HintPath>C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed\Assembly-CSharp.dll</HintPath>
  <Private>False</Private>
</Reference>
<Reference Include="UnityEngine">
  <HintPath>C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed\UnityEngine.dll</HintPath>
  <Private>False</Private>
</Reference>
```

> `Private=False` 表示不把游戏 DLL 复制到输出目录（避免误打包游戏程序集）。

---

## 三、Mod 类入口点

继承 `Verse.Mod` 类是模组的主入口，负责注册设置、应用 Harmony 补丁。

```csharp
using Verse;
using HarmonyLib;

namespace <YourPrefix>.YourMod
{
    public class YourMod : Mod
    {
        // 单例引用，便于全局访问设置
        public static YourModSettings Settings;

        public YourMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<YourModSettings>();
            // 应用全部 Harmony 补丁
            var harmony = new Harmony("<YourPrefix>.yourmod");
            harmony.PatchAll();
            Log.Message("[YourMod] Loaded successfully.");
        }
    }
}
```

### 轻量入口：[StaticConstructorOnStartup]

若无需设置面板，只需在启动时执行一次性逻辑，可用 `[StaticConstructorOnStartup]` 标记的静态构造函数作为轻量入口：

```csharp
using Verse;
using HarmonyLib;

namespace <YourPrefix>.YourMod
{
    [StaticConstructorOnStartup]
    public static class YourModStartup
    {
        static YourModStartup()
        {
            new Harmony("<YourPrefix>.yourmod").PatchAll();
            Log.Message("[YourMod] Static startup complete.");
        }
    }
}
```

> `Mod` 类与 `[StaticConstructorOnStartup]` 二者择一即可；需要设置面板则用 `Mod` 类，否则用静态构造更简洁。

---

## 四、ModSettings 设置持久化

通过继承 `ModSettings` 实现玩家可配置项的存取与 UI。

```csharp
using UnityEngine;
using Verse;

namespace <YourPrefix>.YourMod
{
    public class YourModSettings : ModSettings
    {
        public bool enableFeature = true;
        public float damageMultiplier = 1.0f;

        // 存档读写：序列化/反序列化字段
        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableFeature, "enableFeature", true);
            Scribe_Values.Look(ref damageMultiplier, "damageMultiplier", 1.0f);
            base.ExposeData();
        }

        // 设置窗口 UI
        public void DoWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.CheckboxLabeled("Enable feature", ref enableFeature);
            listing.Label("Damage multiplier: " + damageMultiplier.ToString("0.00"));
            damageMultiplier = listing.Slider(damageMultiplier, 0.1f, 5.0f);

            listing.End();
        }
    }
}
```

在 `Mod` 类中重写 `DoSettingsWindowContents` 与 `SettingsCategory` 即可在游戏「选项 → 模组」中显示面板：

```csharp
public override string SettingsCategory() => "Your Mod";

public override void DoSettingsWindowContents(Rect inRect)
{
    Settings.DoWindowContents(inRect);
    WriteSettings();
}
```

---

## 五、ExposeData 存档兼容

`Scribe` 是 RimWorld 的序列化系统。`ExposeData()` 中根据 `Scribe.mode`（Save / Load / Meta）自动读写。

### Scribe 方法速查表

| 方法 | 用途 | 典型签名 |
|------|------|----------|
| `Scribe_Values.Look` | 值类型（int/float/bool/string/enum） | `Look(ref int field, "label", defaultValue)` |
| `Scribe_References.Look` | 对象引用（如 Thing/Pawn） | `Look(ref Thing t, "label", saveDestroyedThings)` |
| `Scribe_Defs.Look` | Def 引用 | `Look(ref ThingDef def, "label")` |
| `Scribe_Collections.Look` | 集合（List/Dict） | `Look(ref List<T> list, "label", lookMode, keyLookMode)` |
| `Scribe_Deep.Look` | 深拷贝嵌套对象（调用其 ExposeData） | `Look(ref T obj, "label")` |

### 存档兼容要点

- **新增字段**：始终提供默认值，旧存档加载时自动取默认值，不报错。
- **删除字段**：旧存档中的多余数据会被忽略（需保证剩余结构自洽）。
- **改名**：用旧 label 加载再赋给新字段，做迁移。
- **lookMode**：集合元素是值类型用 `LookMode.Value`，引用用 `LookMode.Reference`，Def 用 `LookMode.Def`，嵌套对象用 `LookMode.Deep`。

```csharp
// 兼容旧存档：旧字段叫 oldCount，迁移到 newCount
public override void ExposeData()
{
    int oldCount = -1;
    Scribe_Values.Look(ref oldCount, "oldCount", -1);
    if (Scribe.mode == LoadSaveMode.LoadingVars && oldCount >= 0)
        newCount = oldCount;
    Scribe_Values.Look(ref newCount, "newCount", 0);
    base.ExposeData();
}
```

---

## 六、ThingComp 自定义组件

`ThingComp` 为物品/建筑挂载自定义行为，需配合 `CompProperties` 在 XML 中声明数据。

### 6.1 CompProperties（数据定义）

```csharp
using Verse;

namespace <YourPrefix>.YourMod
{
    // 注意：类名需与 XML 中 Class 属性一致
    public class CompProperties_Cooldown : CompProperties
    {
        public float cooldownSeconds = 5f;

        public CompProperties_Cooldown()
        {
            this.compClass = typeof(CompCooldown);
        }
    }
}
```

### 6.2 ThingComp（行为实现）

```csharp
using Verse;

namespace <YourPrefix>.YourMod
{
    public class CompCooldown : ThingComp
    {
        public CompProperties_Cooldown Props => (CompProperties_Cooldown)this.props;

        private int lastUsedTick = -99999;

        public bool IsOnCooldown => Find.TickManager.TicksGame - lastUsedTick < Props.cooldownSeconds * 60;

        public void Trigger()
        {
            lastUsedTick = Find.TickManager.TicksGame;
        }

        // 存档
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref lastUsedTick, "lastUsedTick", -99999);
            base.PostExposeData();
        }
    }
}
```

### 6.3 XML 中使用

```xml
<ThingDef ParentName="BaseWeapon">
  <defName><YourPrefix>_CooldownBlade</defName>
  <comps>
    <li Class="<YourPrefix>.CompProperties_Cooldown">
      <cooldownSeconds>3</cooldownSeconds>
    </li>
  </comps>
</ThingDef>
```

---

## 七、GameComponent / MapComponent / WorldComponent

三种 Component 分别在不同作用域注册，供你挂载全局/地图/世界级别的逻辑。

| Component | 作用域 | 注册时机 | 典型用途 |
|-----------|--------|----------|----------|
| `GameComponent` | 整局游戏（所有地图 + 世界） | 新游戏/加载存档 | 全局事件、跨地图状态 |
| `MapComponent` | 单个地图 | 地图创建/加载 | 该地图专属逻辑、渲染 |
| `WorldComponent` | 世界地图 | 世界创建/加载 | 派系、世界事件 |

### 使用方式

```csharp
// 自定义 GameComponent
public class YourGameComp : GameComponent
{
    public YourGameComp(Game game) { }

    public override void GameComponentTick()
    {
        // 每帧/每 Tick 逻辑
    }
}

// 注册（在 Mod 启动或 StaticConstructorOnStartup 中）
[StaticConstructorOnStartup]
public static class RegisterComps
{
    static RegisterComps()
    {
        // 通过 Harmony 或直接在 XML 注册 GameComponent 也可
    }
}
```

> `GameComponent` 通常通过 `Patch` 游戏的 `Game.InitNewGame` / `Game.LoadGame` 注入，或在 XML 中通过 `gameConditionClass` 等机制注册。最简方式是在 `Mod` 构造时用 Harmony 往 `Game.FinalizeInit` 等方法加 Postfix 来确保组件存在。

获取组件：`Current.Game.GetComponent<YourGameComp>()`、`map.GetComponent<YourMapComp>()`、`Find.World.GetComponent<YourWorldComp>()`。

---

## 八、DefModExtension 自定义 Def 扩展字段

`DefModExtension` 让你在不新建 Def 类型的前提下，给已有 Def（如 ThingDef）附加自定义数据字段。

```csharp
using Verse;

namespace <YourPrefix>.YourMod
{
    public class DefModExtension_Tier : DefModExtension
    {
        public int tier = 1;
        public bool isLegendary = false;
    }
}
```

XML 中挂载：

```xml
<ThingDef ParentName="BaseWeapon">
  <defName><YourPrefix>_TierBlade</defName>
  <modExtensions>
    <li Class="<YourPrefix>.DefModExtension_Tier">
      <tier>3</tier>
      <isLegendary>true</isLegendary>
    </li>
  </modExtensions>
</ThingDef>
```

运行时读取：

```csharp
var ext = def.GetModExtension<DefModExtension_Tier>();
if (ext != null && ext.isLegendary) { /* ... */ }
```

---

## 九、自定义 Def 类型

当现有 Def 类型无法满足时，可继承 `Def` 创建全新类型，游戏会自动扫描并加载。

```csharp
using Verse;

namespace <YourPrefix>.YourMod
{
    public class TierDef : Def
    {
        public int powerLevel;
        public string unlockText;
    }
}
```

XML 中使用（顶层标签为类名去掉 `Def` 后缀，即 `Tier`）：

```xml
<Defs>
  <TierDef>
    <defName><YourPrefix>_TierLegendary</defName>
    <label>legendary tier</label>
    <powerLevel>10</powerLevel>
    <unlockText>You feel immense power.</unlockText>
  </TierDef>
</Defs>
```

查询：`DefDatabase<TierDef>.GetNamed("<YourPrefix>_TierLegendary")`。

---

## 十、跨版本兼容做法

让一个模组同时支持多个游戏版本，常用组合策略：

### 10.1 版本文件夹 + LoadFolders.xml

为不同版本准备专属 Def/Patch/DLL，通过 `LoadFolders.xml` 按版本加载（见 `02-project-structure.md`）。

### 10.2 NuGet 通配版本

`Krafs.Rimworld.Ref` 用 `1.*` 通配，使同一项目可在多版本编译（注意 API 差异需条件编译或运行时判断）。

### 10.3 条件 Patch

用 `PatchOperationFindMod` 检测 DLC，用版本专属 Patch 文件夹处理版本差异。

### 10.4 运行时模组检测

C# 中检测 DLC/模组是否启用，按需启用逻辑：

```csharp
using Verse;

public static class ModDetector
{
    // 检测 DLC 是否安装
    public static bool RoyaltyInstalled => ModsConfig.RoyaltyActive;
    public static bool BiotechInstalled => ModsConfig.BiotechActive;
    public static bool AnomalyInstalled => ModsConfig.AnomalyActive;
    public static bool OdysseyInstalled => ModsConfig.OdysseyActive;

    // 检测指定模组是否启用
    public static bool IsModActive(string packageId)
    {
        return ModsConfig.ActiveModsInLoadOrder.Any(m => m.PackageId == packageId);
    }
}

// 使用
if (ModDetector.BiotechInstalled)
{
    // 仅 Biotech 启用时执行
}
```

### 10.5 运行时版本判断

```csharp
// 当前游戏版本
var ver = VersionControl.CurrentVersion;
if (ver >= new Version(1, 6))
{
    // 1.6+ 专属逻辑
}
```

### 10.6 Prepare 条件 Patch（Harmony）

通过 Harmony 的 `Prepare` 方法决定是否应用补丁，详见 `06-harmony.md`「条件 Patch」。
