# 01 - 环境搭建

> **适用版本**: RimWorld 1.6 | **最后更新**: 2026-07-28

本文档介绍 RimWorld 模组开发所需的环境配置，包括 IDE 选择、.NET SDK、引用程序集、NuGet 包、反编译工具与辅助工具。

---

## 一、IDE 选择

| IDE | 推荐度 | 说明 |
|-----|--------|------|
| **Visual Studio 2022** | 推荐（首选） | Windows 平台原生支持 .NET Framework，调试体验最佳，社区版免费即可满足需求。安装时勾选「.NET 桌面开发」工作负载。 |
| **JetBrains Rider** | 强烈推荐 | 跨平台，对 C# 与 Harmony 的代码分析极强，内置反编译查看器，适合进阶开发者。需付费（有免费学生/开源授权）。 |
| **Visual Studio Code** | 可用 | 轻量，需配合 C# Dev Kit 扩展与 .NET SDK；对旧式 .csproj 支持稍弱，调试配置略繁琐，适合纯 XML 模组或轻量 C# 开发。 |

> 选择建议：以 C# 开发为主选 VS 2022 或 Rider；仅做 XML/Patch 选任意编辑器（VS Code、Notepad++ 均可）。

---

## 二、.NET Framework 4.7.2 SDK

RimWorld 基于 Unity 的 Mono 运行时，目标框架为 **.NET Framework 4.7.2**。开发 C# 模组时，编译目标必须与之匹配，否则可能出现运行时 `MissingMethodException` 等问题。

安装方式：
- **Visual Studio 2022 安装器**：勾选「.NET Framework 4.7.2 SDK」与「.NET Framework 4.7.2 目标包」（在「单个组件」中搜索）。
- **独立安装包**：从微软官网下载 .NET Framework 4.7.2 Developer Pack。

> 提示：使用 `Krafs.Rimworld.Ref` NuGet 包时，包内已包含正确的程序集引用，可免去手动配置目标框架的麻烦，但本机仍需安装对应 SDK 以支持编译。

---

## 三、引用 DLL 路径表

手动引用本地 DLL 时，所有程序集位于游戏安装目录下的 `RimWorldWin64_Data/Managed/`（Steam 默认安装路径见下）。

默认游戏路径（Steam）：
```
C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed\
```

| DLL 文件 | 作用 | 是否常用 |
|----------|------|----------|
| `Assembly-CSharp.dll` | 游戏主程序集，包含 Verse / RimWorld 全部核心类型 | 必引 |
| `UnityEngine.dll` | Unity 引擎核心类型（GameObject、Transform 等） | 必引 |
| `UnityEngine.CoreModule.dll` | Unity 核心模块 | 常引 |
| `UnityEngine.IMGUIModule.dll` | 即时模式 GUI（IMGUI），自定义窗口绘制 | 常引 |
| `UnityEngine.TextRenderingModule.dll` | 文本渲染 | 视需要 |
| `UnityEngine.PhysicsModule.dll` | 物理系统 | 视需要 |
| `Mono.Cecil.dll` | 部分框架运行时使用 | 视需要 |
| `0Harmony.dll` | Harmony 补丁库（通常通过 NuGet 引入） | 常引 |

> 推荐做法：优先使用 NuGet 包（见下节）替代手动引用本地 DLL，可避免游戏更新后路径失效、版本不一致等问题。

---

## 四、NuGet 包推荐

### 4.1 Krafs.Rimworld.Ref

社区维护的 RimWorld 游戏引用包，封装了 `Assembly-CSharp.dll` 与 `UnityEngine.*.dll`，按游戏版本提供对应的引用程序集。

```xml
<ItemGroup>
  <PackageReference Include="Krafs.Rimworld.Ref" Version="1.6.*" />
</ItemGroup>
```

- 使用通配版本号（如 `1.6.*`）可自动跟进该大版本内的修订。
- 优点：无需手动指向本地游戏目录，CI 构建友好。

### 4.2 Lib.Harmony

Harmony 运行时补丁库的 NuGet 发行版，版本与 RimWorld 内置的 Harmony 保持兼容。

```xml
<PackageReference Include="Lib.Harmony" Version="2.*" />
```

### 4.3 Lib.Harmony.Thin

仅含 Harmony 接口与特性的「瘦」包，不含实现。适用于：你的模组只声明 Harmony 补丁，实际运行依赖游戏内已加载的 Harmony，避免重复携带 DLL。

```xml
<PackageReference Include="Lib.Harmony.Thin" Version="2.*" />
```

### 4.4 Krafs.Publicizer

允许在编译时「公开」`Assembly-CSharp.dll` 中的 `private`/`internal` 成员，使其可在模组代码中直接访问，无需反射。

```xml
<ItemGroup>
  <PackageReference Include="Krafs.Publicizer" Version="2.*" PrivateAssets="all" />
</ItemGroup>

<ItemGroup>
  <Publicize Include="Assembly-CSharp" />
</ItemGroup>
```

> 详见 `05-csharp-basics.md` 中「项目文件配置」一节。

---

## 五、dnSpy 反编译工具使用

**dnSpy**（或其维护分支 dnSpyEx）是 .NET 调试与反编译利器，用于查看游戏源码、定位需要 Harmony 补丁的方法。

### 基本流程

1. 下载 dnSpyEx（dnSpy 原版已停止维护，推荐使用 dnSpyEx 分支）。
2. 打开 `File → Open`，加载 `RimWorldWin64_Data/Managed/Assembly-CSharp.dll`。
3. 在左侧「程序集资源管理器」中按命名空间浏览（如 `RimWorld`、`Verse`）。
4. 双击类名查看反编译后的 C# 源码，定位目标方法的签名与可见性。
5. 右键方法 → 「编辑 IL」可查看中间语言（Transpiler 调试用）。

### 实用技巧

- **搜索类型/方法**：`Ctrl + Shift + K` 全局搜索，或 `Ctrl + M` 搜索成员。
- **附加调试**：`Debug → Start Executable` 选择 RimWorld 启动程序，可断点调试游戏运行流程（配合 PDB 时体验更佳）。
- **导出项目**：右键程序集 →「导出到项目」，可生成可编译的 C# 项目供离线参考。

> 注意：反编译结果仅供学习与定位 API，切勿直接复制粘贴游戏私有代码到模组中发布，存在版权风险。

---

## 六、辅助工具

### RimPy

老牌模组管理器，用于模组排序、配置管理与本地模组整理。可自动生成 `ModSync.xml`、检测依赖关系。Windows 平台使用。

### RimSort

开源跨平台（Windows / macOS / Linux）模组管理器，提供智能排序、冲突检测、Steam 创意工坊集成、本地与外部数据库比对等功能，是 RimPy 的现代替代方案。

| 工具 | 平台 | 主要用途 |
|------|------|----------|
| RimSort | 全平台 | 模组排序、冲突检测、工坊集成（推荐） |
| RimPy | Windows | 模组排序、配置管理、ModSync 生成 |
| dnSpyEx | Windows | 反编译、调试 |
| Notepad++ / VS Code | 全平台 | XML / Patch 快速编辑 |

---

## 七、快速检查清单

完成环境搭建后，逐项核对：

- [ ] 已安装 Visual Studio 2022（含 .NET 桌面开发工作负载）或 Rider。
- [ ] 已安装 .NET Framework 4.7.2 SDK 与目标包。
- [ ] 已确认 RimWorld 游戏安装路径，能定位到 `Managed/Assembly-CSharp.dll`。
- [ ] 已创建 .csproj 并通过 `Krafs.Rimworld.Ref` 引用游戏程序集（或手动引用本地 DLL）。
- [ ] 已通过 `Lib.Harmony` 引入 Harmony（如需 C# 补丁）。
- [ ] 已安装 dnSpyEx 并能打开 `Assembly-CSharp.dll` 查看源码。
- [ ] 已安装 RimSort 或 RimPy 用于本地测试与排序。
- [ ] 能成功编译一个空的 `[StaticConstructorOnStartup]` 入口类并加载进游戏（无红字）。
