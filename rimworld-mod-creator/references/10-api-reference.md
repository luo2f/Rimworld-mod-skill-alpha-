# 10 - API 速查表

> **适用版本**: RimWorld 1.6 | **最后更新**: 2026-07-28

本文档汇总 RimWorld 模组开发常用 API：命名空间、高频类、高频方法与学习资源，供编码时快速查阅。

---

## 一、常用命名空间表

| 命名空间 | 内容 |
|----------|------|
| `Verse` | 引擎核心：Thing、Pawn、Map、Find、GenSpawn、GenRecipe、Log、Scribe 等 |
| `RimWorld` | 游戏逻辑：Faction、Quest、Recipe、Hediff、Thought、ResearchProject、QualityUtility 等 |
| `RimWorld.Planet` | 世界地图：WorldObject、Caravan、Settlement、WorldComponent 等 |
| `HarmonyLib` | Harmony 补丁：HarmonyPatch、Prefix、Postfix、Transpiler 等 |
| `UnityEngine` | Unity 引擎：Vector3、Rect、Color、GUI、GameObject 等 |

---

## 二、高频类速查

### 2.1 Verse.Thing

所有「东西」的基类（物品、建筑、Pawn、投射物）。

| 成员 | 类型 | 说明 |
|------|------|------|
| `def` | `ThingDef` | 该 Thing 的定义 |
| `Position` | `IntVec3` | 所在格子坐标 |
| `Map` | `Map` | 所在地图（null 表示未生成） |
| `Spawned` | `bool` | 是否已生成到地图 |
| `HitPoints` | `int` | 当前耐久 |
| `MaxHitPoints` | `int` | 最大耐久 |
| `Rotation` | `Rot4` | 朝向 |
| `Destroy(DestroyMode)` | `void` | 销毁 |
| `GetComp<T>()` | `T` | 获取指定类型的 Comp |
| `TakeDamage(DamageInfo)` | `void` | 受到伤害 |
| `SpawnSetup(Map, bool)` | `void` | 生成初始化 |

```csharp
// 获取自定义 Comp
var comp = thing.GetComp<CompCooldown>();
if (comp != null && comp.IsOnCooldown) { /* ... */ }
```

### 2.2 Verse.Pawn

生物（小人、动物、机械体），继承自 Thing。

| 成员 | 类型 | 说明 |
|------|------|------|
| `health` | `Pawn_HealthTracker` | 健康（Hediff、伤势、死亡） |
| `needs` | `Pawn_NeedsTracker` | 需求（食物、心情、休息） |
| `story` | `Pawn_StoryTracker` | 背景、特征（人类） |
| `equipment` | `Pawn_EquipmentTracker` | 装备（手持武器） |
| `apparel` | `Pawn_ApparelTracker` | 穿戴衣物 |
| `inventory` | `Pawn_InventoryTracker` | 随身物品栏 |
| `mindState` | `Pawn_MindState` | 心理状态 |
| `jobs` | `Pawn_JobTracker` | 工作（Job）队列 |
| `def.defName` | `string` | 种类 defName |
| `Faction` | `Faction` | 所属派系 |
| `Name` | `Name` | 姓名 |
| `IsColonist` | `bool` | 是否为殖民者 |

```csharp
if (pawn.health.hediffSet.HasHediff(someHediffDef))
{
    pawn.needs.mood.thoughts.memories.TryGainMemory(someThoughtDef);
}
```

### 2.3 Verse.Map

单张地图。

| 成员 | 类型 | 说明 |
|------|------|------|
| `thingGrid` | `ThingGrid` | 按格子查 Thing |
| `listerThings` | `ListerThings` | 按条件列 Thing |
| `weatherManager` | `WeatherManager` | 天气 |
| `mapPawns` | `MapPawns` | 地图上所有 Pawn |
| `areaManager` | `AreaManager` | 区域（存储区等） |
| `gameConditionManager` | `GameConditionManager` | 游戏状态（如日食） |
| `GetComponent<T>()` | `T` | 获取 MapComponent |

```csharp
// 查找地图上所有殖民者
foreach (Pawn p in map.mapPawns.FreeColonists) { /* ... */ }

// 按格子取 Thing
Thing t = map.thingGrid.ThingAt(cell, ThingCategory.Item);
```

### 2.4 Verse.Find

全局访问器，获取游戏当前状态。

| 成员 | 类型 | 说明 |
|------|------|------|
| `CurrentMap` | `Map` | 当前玩家正在查看的地图 |
| `Maps` | `List<Map>` | 所有地图 |
| `World` | `World` | 世界 |
| `FactionManager` | `FactionManager` | 派系管理 |
| `ResearchManager` | `ResearchManager` | 研究 |
| `TickManager` | `TickManager` | 时间（TicksGame） |
| `WorldObjects` | `WorldObjectsHolder` | 世界对象 |
| `AnyPlayerHomeMap` | `bool` | 是否有玩家基地 |
| `AliveFreeColonists` | `IEnumerable<Pawn>` | 所有存活殖民者 |

```csharp
// 全局搜索某类物品
Thing any = Find.AnyThing<ThingDefOf.Steel>();
int tick = Find.TickManager.TicksGame;
```

### 2.5 Verse.GenSpawn

生成 Thing 到地图的静态工具。

```csharp
// 在指定位置生成物品
GenSpawn.Spawn(thing, cell, map);

// 带朝向生成
GenSpawn.Spawn(thing, cell, map, rot);
```

### 2.6 Verse.GenRecipe

配方产出处理。

```csharp
// 处理配方产出（PostProcessProduct 在制作完成时调用）
// 常用于 Harmony Postfix 扩展产出
```

### 2.7 RimWorld.QualityUtility

品质（Awful~Legendary）工具。

```csharp
// 生成随机品质
QualityCategory quality = QualityUtility.GenerateQualityGeneratingPawnDefaults();

// 给物品赋予品质
var compQ = thing.TryGetComp<CompQuality>();
if (compQ != null) compQ.SetQuality(quality, ArtGenerationContext.Outsourced);
```

---

## 三、高频方法速查

### 3.1 生成与放置

| 方法 | 作用 |
|------|------|
| `GenSpawn.Spawn(Thing, IntVec3, Map)` | 生成 Thing 到地图 |
| `ThingMaker.MakeThing(ThingDef, ThingDef)` | 创建 Thing 实例（可指定材料） |
| `GenPlace.TryPlaceThing(Thing, IntVec3, Map, PlaceMode)` | 尝试放置（含落地处理） |
| `GenThing.TryDropAndForbid(...)` | 丢弃并标记禁止 |

```csharp
Thing item = ThingMaker.MakeThing(ThingDefOf.Steel, null);
item.stackCount = 50;
GenSpawn.Spawn(item, cell, map);
```

### 3.2 查询

| 方法 | 作用 |
|------|------|
| `DefDatabase<T>.GetNamed(string)` | 按 defName 查 Def（找不到抛异常） |
| `DefDatabase<T>.GetNamedSilentFail(string)` | 按 defName 查 Def（找不到返回 null） |
| `DefDatabase<T>.AllDefs` | 遍历所有 Def |
| `map.thingGrid.ThingAt<T>(cell)` | 格子上某类型 Thing |
| `map.listerThings.ThingsOfDef(def)` | 地图上某 Def 的全部 Thing |
| `Find.AnyThing<T>()` | 全局任一指定类型 Thing |

```csharp
var def = DefDatabase<ThingDef>.GetNamedSilentFail("<YourPrefix>_IronSword");
foreach (Thing t in map.listerThings.ThingsOfDef(def)) { /* ... */ }
```

### 3.3 伤害与战斗

| 方法 | 作用 |
|------|------|
| `thing.TakeDamage(DamageInfo)` | 造成伤害 |
| `pawn.Kill(DamageInfo?, Hediff?)` | 杀死 Pawn |
| `DamageInfo` 构造 | 构造伤害信息（伤害类型、数值、来源） |
| `pawn.health.AddHediff(hediff)` | 添加健康状态 |

```csharp
var dinfo = new DamageInfo(DamageDefOf.Cut, 10, instigator: pawn);
targetThing.TakeDamage(dinfo);
```

### 3.4 日志

| 方法 | 作用 |
|------|------|
| `Log.Message(string)` | 普通信息 |
| `Log.Warning(string)` | 警告 |
| `Log.Error(string)` | 错误（不中断） |
| `Log.ErrorOnce(string, int)` | 同 ID 只报一次 |

### 3.5 翻译

```csharp
// 翻译键带参数
"YourPrefix_Greeting".Translate(pawn.NameStringShort);
// 翻译 Def 字段
def.label.Translate();
```

### 3.6 组件获取

```csharp
// Thing 上的 Comp
var comp = thing.GetComp<CompCooldown>();
// 地图 MapComponent
var mapComp = map.GetComponent<YourMapComp>();
// 游戏 GameComponent
var gameComp = Current.Game.GetComponent<YourGameComp>();
// 世界 WorldComponent
var worldComp = Find.World.GetComponent<YourWorldComp>();
```

---

## 四、学习资源链接

| 资源 | 说明 |
|------|------|
| RimWorld 官方 Wiki | https://www.rimworldwiki.com/wiki/Modding_Tutorials 模组制作教程 |
| RimWorld Wiki - Modding | https://www.rimworldwiki.com/wiki/Modding 总览 |
| Harmony 文档 | https://harmony.pardeike.net/ Harmony 官方文档 |
| dnSpyEx | https://github.com/dnSpyEx/dnSpy 反编译调试工具 |
| RimSort | https://github.com/RimSort/RimSort 模组管理器 |
| Krafs.Rimworld.Ref | https://www.nuget.org/packages/Krafs.Rimworld.Ref 游戏引用 NuGet 包 |

> 提示：API 细节以游戏当前版本反编译结果为准，Wiki 可能滞后。用 dnSpyEx 查看 `Assembly-CSharp.dll` 是最权威的 API 来源。
