# C# 模组开发参考

## 项目文件配置

RimWorld C# 模组有两种 .csproj 风格。

| 特征 | 旧式（非 SDK-style） | 现代（SDK-style，推荐） |
|------|---------------------|------------------|
| 根元素 | `<Project ToolsVersion xmlns="...msbuild...">` | `<Project Sdk="Microsoft.NET.Sdk">` |
| TargetFramework | `v4.7.2` | `net48` |
| RimWorld API 引用 | 直引本地 DLL 路径 | NuGet 包 `Krafs.Rimworld.Ref` |
| Harmony 引用 | `packages.config` + HintPath | `<PackageReference Include="Lib.Harmony">` |
| 私有成员访问 | 反射 `BindingFlags.NonPublic` | `<Publicize Include="Assembly-CSharp"/>` |
| 文件列表 | 需逐个 `<Compile Include>` | 自动包含所有 .cs 文件 |

### 现代 SDK-style .csproj（推荐）

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <LangVersion>latest</LangVersion>
    <OutputPath>..\1.6\Assemblies\</OutputPath>
    <RootNamespace>MyMod</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Krafs.Rimworld.Ref" Version="1.6.*-*" />
    <PackageReference Include="Lib.Harmony" Version="2.4.2" ExcludeAssets="runtime" />
    <PackageReference Include="Krafs.Publicizer" Version="2.3.0" />
  </ItemGroup>
  <ItemGroup>
    <Publicize Include="Assembly-CSharp"/>
  </ItemGroup>
</Project>
```

- `Krafs.Rimworld.Ref` 包含 RimWorld API + UnityEngine 传递依赖
- `ExcludeAssets="runtime"` 表示编译时引用 Harmony 但不打包其 DLL
- `<Publicize>` 自动将 `Assembly-CSharp.dll` 的 private/internal 成员公开为 public

### 旧式 .csproj（直引本地 DLL）

```xml
<Reference Include="Assembly-CSharp">
  <HintPath>..\..\..\RimWorldWin64_Data\Managed\Assembly-CSharp.dll</HintPath>
  <Private>False</Private>
</Reference>
```

路径解析：.csproj 位于 `Source/` → `..` → 模组目录 → `..` → `Mods/` → `..` → `RimWorld/`。前提：开发时模组必须放在 `RimWorld/Mods/ModName/` 目录下。

## Mod 加载入口模式

### 模式 A：继承 Mod 类（最常见）

```csharp
using HarmonyLib;
using Verse;

namespace MyMod
{
    public class MyMod : Mod
    {
        public static MySettings settings;

        public MyMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<MySettings>();
            var harmony = new Harmony("com.author.modid");
            harmony.PatchAll();
        }

        public override string SettingsCategory() => "My Mod";
        public override void DoSettingsWindowContents(Rect inRect)
            => settings.DoWindow(inRect);
    }
}
```

RimWorld 在加载模组时通过反射扫描程序集中继承 `Verse.Mod` 的类型并自动实例化。

### 模式 B：[StaticConstructorOnStartup]（轻量级，无需 Mod 类）

```csharp
[StaticConstructorOnStartup]
public class MyInitializer
{
    static MyInitializer()
    {
        foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefs)
        {
            // 修改逻辑
        }
    }
}
```

适合不需要设置界面的轻量模组，通过 `DefDatabase<T>.AllDefs` 遍历修改 Def。

### 模式 C：自动 Harmony patch

Mod 类构造函数不调用 `PatchAll()`，依赖 RimWorld 引擎自动扫描 `[HarmonyPatch]` 注解。前提：Assemblies 中需包含 `0Harmony.dll` 或依赖全局 Harmony。

## Harmony Patch 写法

### 属性注解式 Patch

```csharp
// Postfix：修改方法后的行为
[HarmonyPatch(typeof(Projectile), "Launch",
    new Type[] { typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo) })]
class PatchLaunch
{
    static void Postfix(Projectile __instance, ref Vector3 ___destination)
    {
        // __instance = 被 patch 的实例
        // ___destination = 私有字段（三下划线前缀）
    }
}

// Postfix 修改返回值
[HarmonyPatch(typeof(Projectile), "get_StartingTicksToImpact")]
class PatchSpeed
{
    static float Postfix(float value)
    {
        return value / MyMod.settings.projectileSpeed;
    }
}

// Prefix：返回 false 跳过原方法
[HarmonyPatch(typeof(PawnRenderUtility), "DrawEquipmentAiming")]
class PatchDraw
{
    static bool Prefix(ref Thing eq)
    {
        if (!MyMod.settings.enabled) return true;  // true=执行原方法
        // 自定义逻辑
        return false;  // false=跳过原方法
    }
}
```

### Patch 类型对照

| 类型 | 时机 | 关键参数 | 用途 |
|------|------|---------|------|
| `Prefix` | 原方法前 | `__instance`, `ref` 参数, `___字段名` | 拦截/替换；返回 false 跳过原方法 |
| `Postfix` | 原方法后 | `__instance`, `ref __result` | 修改返回值 |
| `Transpiler` | IL 级别 | `IEnumerable<CodeInstruction>` | 用 CodeMatcher 替换 IL |
| `Prepare` | 应用前 | 返回 `bool` | 条件判断是否应用 |

### 特殊参数约定

- `__instance`：被 patch 方法的 this 引用
- `__result`：原方法返回值（Postfix 中可 ref 修改）
- `___fieldName`：访问原类的私有实例字段（三个下划线前缀）
- `__args`：访问原方法所有参数数组
- `ref` 参数：修改原方法参数值

### 条件 Patch（Prepare 方法）

```csharp
[HarmonyPatch(typeof(GameComponentUtility), nameof(GameComponentUtility.StartedNewGame))]
public static class ResetOnStarted
{
    public static bool Prepare() =>
        !ModLister.AnyModActiveNoSuffix(["zetrith.prepatcher"]);  // false=跳过此 patch
    public static void Postfix() => PawnDataUtility.Reset();
}
```

## ModSettings 设置持久化

```csharp
public class MySettings : ModSettings
{
    public bool enabled = true;
    public float speed = 1f;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref enabled, "enabled", true);
        Scribe_Values.Look(ref speed, "speed", 1f);
    }

    public void DoWindow(Rect inRect)
    {
        var listing = new Listing_Standard();
        listing.Begin(inRect);
        listing.CheckboxLabeled("Enabled".Translate(), ref enabled);
        listing.SliderLabeled("Speed".Translate(), ref speed, 0.1f, 10f);
        listing.End();
    }
}
```

`ExposeData()` 在保存和加载时都被调用，`Scribe_Values.Look` 自动根据模式读写。

## 自定义 Def 与 ThingComp

### 自定义 Def

```csharp
public class GunPropDef : Def
{
    public float recoil;
    public bool enableTrails;
}
// XML: <GunPropDef><defName>Gun_Pistol_Prop</defName>...</GunPropDef>
```

### ThingComp

```csharp
public class CompGun : ThingComp
{
    public CompProperties_Gun Props => (CompProperties_Gun)props;

    public override void CompTick()
    {
        base.CompTick();
        // 每帧逻辑
    }
}

public class CompProperties_Gun : CompProperties
{
    public float spread;
    public CompProperties_Gun() { compClass = typeof(CompGun); }
}
// XML: <li Class="MyMod.CompProperties_Gun"><spread>0.1</spread></li>
// 获取: thing.TryGetComp<CompGun>()
```

## 跨版本兼容做法

| 方法 | 说明 |
|------|------|
| 版本文件夹 | `1.4/`、`1.5/`、`1.6/` 各存对应版本 Assemblies/Defs |
| LoadFolders.xml | 自定义版本文件夹加载规则 |
| NuGet 通配版本 | `Version="1.6.*-*"` 自动匹配最新引用包 |
| 条件 patch | `Prepare()` 检测其他 mod，动态决定是否应用 |
| 运行时模组检测 | 遍历 `ModsConfig.ActiveModsInLoadOrder` |
| About.xml 声明 | `modDependencies` + `loadAfter` |

### 运行时跨模组兼容检测

```csharp
public static bool usingGiddyUp = false;

static Core()
{
    foreach (var mod in ModsConfig.ActiveModsInLoadOrder)
    {
        switch (mod.PackageId.ToLower())
        {
            case "owlchemist.giddyup":
            case "memegoddess.giddyup":
                usingGiddyUp = true;
                Log.Message($"[MyMod] Giddy-up! detected");
                break;
        }
    }
}
```
