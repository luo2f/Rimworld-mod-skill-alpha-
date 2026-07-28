# 09 - Steam Workshop 发布

> **适用版本**: RimWorld 1.6 | **最后更新**: 2026-07-28

本文档讲解模组发布到 Steam 创意工坊的完整流程：发布前检查、多语言支持、上传步骤、版本管理、描述最佳实践、兼容性声明与更新注意事项。

---

## 一、发布前检查清单

逐项核对后再发布：

- [ ] `About/About.xml` 完整：`packageId`（唯一且最终定稿）、`name`、`author`、`description`、`supportedVersions`。
- [ ] `About/Preview.png`：640×360，PNG 格式，清晰美观（工坊展示图）。
- [ ] 法律自查：未包含游戏私有代码、未使用未授权的第三方资产（贴图、音效、代码）。
- [ ] 目录结构清理：无 `.vs/`、`bin/`、`obj/`、调试日志、临时文件。
- [ ] Def 测试：所有自定义 Def 可正常生成、无红字。
- [ ] 无红字日志：隔离测试（Core + 必要依赖 + 你的模组）启动无报错。
- [ ] 兼容性测试：与常见依赖框架共存无冲突。
- [ ] 版本号：`About.xml` 中 `supportedVersions` 与实际测试版本一致。
- [ ] 翻译：至少提供 `English/Keyed/`，建议附带中文。
- [ ] 描述：工坊描述完整，含功能、依赖、兼容性说明。

---

## 二、多语言支持

- **English 必选**：`Languages/English/Keyed/` 必须存在，作为兜底语言。
- **其他语言可选**：`ChineseSimplified`、`ChineseTraditional`、`French`、`German`、`Japanese`、`Russian` 等。
- 翻译结构详见 `07-assets.md`「翻译系统」。

```
Languages/
├── English/
│   ├── Keyed/
│   └── DefInjected/
└── ChineseSimplified/
    ├── Keyed/
    └── DefInjected/
```

> 即使只做英文，也建议预留 `Languages/English/Keyed/` 占位，方便后续汉化与社区贡献。

---

## 三、Steam Workshop 上传步骤

### 3.1 游戏内直接上传（推荐）

最简单的方式，适合绝大多数作者：

1. 将模组文件夹放入本地 Mod 目录：`Steam/steamapps/common/RimWorld/Mods/`。
2. 启动 RimWorld → 主菜单 → 选项 → 模组 → 找到「Upload mod」或使用开发模式中的上传入口。
3. 选择你的模组，填写工坊标题与描述。
4. 点击上传，等待 Steam 处理完成。
5. 上传成功后，游戏会在模组 `About/` 下生成 `PublishedFileId.txt`（记录工坊 ID，**勿删**）。

### 3.2 手动上传（高级）

通过 Steam Workshop 工具或第三方上传器（如 SteamCMD）上传，适合需要 CI 自动化发布的作者。需配置 `PublishedFileId.txt` 与 appid 294100。

> 一般作者用游戏内上传即可；手动上传涉及命令行与配置，仅在需要批量/自动化时使用。

---

## 四、版本管理

### 4.1 语义化版本

模组版本建议遵循语义化版本号 `major.minor.patch`：

| 版本段 | 含义 | 示例 |
|--------|------|------|
| `major` | 不兼容的大改动（破坏存档） | 1.0.0 → 2.0.0 |
| `minor` | 向后兼容的新功能 | 1.0.0 → 1.1.0 |
| `patch` | 修复/小调整 | 1.0.0 → 1.0.1 |

### 4.2 ModSync.xml

`ModSync.xml`（放在模组根目录）供 RimPy 等管理器识别版本与同步信息：

```xml
<?xml version="1.0" encoding="utf-8"?>
<ModSync>
  <Version>1.2.0</Version>
  <Manifest>
    <identifier><YourPrefix>.yourmodname</identifier>
    <version>1.2.0</version>
    <downloadUri>https://github.com/YourName/YourMod/releases/latest</downloadUri>
  </Manifest>
</ModSync>
```

---

## 五、Mod 描述最佳实践

工坊描述应结构清晰，建议模板：

```
[模组名称]

简要一句话介绍模组核心功能。

== 功能特性 ==
- 特性 1
- 特性 2
- 特性 3

== 依赖 ==
- 必需：Harmony、某框架（列出 packageId 或名称）
- 可选：某 DLC（注明哪些功能需要）

== 兼容性 ==
- 兼容：列举已测试兼容的模组类型
- 已知冲突：列举已知冲突及原因
- 加载顺序：应置于 XXX 之后

== 翻译 ==
- English（自带）
- 简体中文（自带/欢迎贡献）

== 更新日志 ==
v1.2.0 (2026-07-28)
- 新增 XXX
- 修复 XXX

== 致谢 ==
- 感谢 XXX

== 反馈 ==
- Bug 反馈：GitHub Issues / 讨论区链接
```

---

## 六、兼容性声明示例

在描述与 README 中明确声明兼容性，减少玩家提问：

```
兼容性声明：
- 本模组通过 XML Patch 与 Harmony 实现功能，未修改原版核心 DLL。
- 推荐加载顺序：Harmony → 本模组依赖的框架 → 本模组 → 其他内容模组。
- 已测试兼容：原版全部 DLC；与常见存储、战斗、UI 类模组无冲突。
- 已知不兼容：无。
- 如遇兼容问题，请附 Player.log 到讨论区反馈。
```

---

## 七、更新 Mod 注意事项

### 7.1 XML 更新

- XML Def/Patch 改动通常**兼容旧存档**（新增字段、调整数值）。
- 删除已使用的 Def 可能导致旧存档引用失效，谨慎操作。

### 7.2 C# 更新

- C# 逻辑改动**可能破坏存档**，尤其是修改了存档数据结构（ExposeData 字段）。
- 升级前在描述中标注「可能需要新存档」，并保留旧字段迁移逻辑。

### 7.3 删除 Def

若必须删除某个 Def，用 `[Obsolete]` 思想渐进式移除：

- 先在描述中声明该 Def 将弃用。
- 在 C# 中用 `[DefOf]` 或运行时检测，对旧存档中的引用做兼容处理。
- 避免直接删除导致旧存档加载崩溃。

### 7.4 版本号与工坊更新

- 每次更新前递增 `ModSync.xml` 与内部版本号。
- 工坊更新后，Steam 会自动推送更新给订阅者。
- 重要更新在描述顶部置顶「更新日志」。

---

## 八、常见问题 FAQ

**Q：上传后 `PublishedFileId.txt` 能删吗？**
A：不能。它记录工坊 ID，删除后游戏无法识别该模组已上传，会视为新模组重复上传。

**Q：上传后能改 `packageId` 吗？**
A：不能。`packageId` 一旦发布即固定，改了等于换了一个模组，玩家存档与配置会断联。

**Q：更新后玩家看不到新内容？**
A：让玩家取消订阅再重新订阅，或重启 Steam，确保订阅文件夹被刷新。也可在工坊页面提示。

**Q：模组太大上传慢/失败？**
A：检查是否误打包了 `bin/`、`obj/`、源码、大体积贴图。精简体积，必要时分拆可选内容为独立模组。

**Q：可以同时发布到工坊和 GitHub 吗？**
A：可以，且推荐。工坊供 Steam 玩家订阅，GitHub 供源码托管与离线下载，二者用同一 `packageId`。
