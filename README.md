# 就你会消除 — 原版 1:1 Unity 复刻工程（二次开发）

微信小游戏《就你会消除》原版 1:1 复刻的 Unity 源码工程。所有 127 个 UI 面板、
PuzzleCut/PuzzleStart/PuzzleShow 关卡配置、3D 花牌堆场景、Spine 特效、
微信平台层（登录/分享/激励视频/好友榜）均按原版数据与 prefab 坐标一比一实现，
可直接二次开发与打包发布。

## 环境要求

- Unity **2022.3.62f3c1**（LTS，`ProjectSettings/ProjectVersion.txt` 已锁定版本）
- 打包微信小游戏：Unity WebGL 模块 + [微信开发者工具]
- 无需联网下载任何资源，全部资源已随仓库分发（约 750MB）

## 快速开始

1. 用 Unity 2022.3.62f3c1 打开本目录（`c:\xiaoxiaole\game`）
2. 打开场景 `Assets/Scenes/StartScene.unity`（主城）并点击 Play，或 `MainScene.unity` 直接续关进玩法
3. 入口：`Assets/Scripts/Core/GameBootstrap.cs`（挂场景根，按场景名自动路由：Load→加载页→StartScene，MainScene→续关玩法）
4. 编辑器使用 `PlaceholderPlatform` 模拟微信能力；需要直接进指定关卡调试可设 `GameBootstrap.DirectEnter=true` + `StartLevel`

> 编辑器批处理验证（无需开图形界面）：
> `tools/compile_check.ps1`（编译预验证）
> `Xio.EditorTools.SceneSmoke.Run`（3 场景路由冒烟）
> 玩法截图：`Xio.EditorTools.DiagShot.Run`（输出 `C:\xiaoxiaole\diag_shot.png`）

## 场景结构（对齐原版 BuildSettings）

原版微信小游戏为 **3 场景 + Addressables 预制体**（catalog 考古确证），本工程同名复刻：

| 场景 | 原版对应 | 内容 |
|---|---|---|
| `Assets/Scenes/Load.unity` | level0 `Scenes/Load` | 加载页 → 自动切主城 |
| `Assets/Scenes/StartScene.unity` | level1 `Scenes/StartScene` | 主城 MainCityPanel |
| `Assets/Scenes/MainScene.unity` | level2 `Scenes/MainScene` | 玩法（续关进 GameplayPanel） |

另外工程内含 **78 个真实 prefab 资产**（`Assets/Prefabs/UI/*` 46 个面板 + `Assets/Prefabs/Model/*` 32 个 3D 模型），
与原版 Addressables 目录一一对应；游戏运行时面板仍由代码按原版 prefab 坐标动态构建（UIPanel 基类），
prefab 资产用于 Project 窗口结构对齐与二次开发参考。

## 目录结构

```
Assets/
├─ Scripts/
│  ├─ Core/        # 玩法核心：PuzzleGame(三消) / Scene3D(原版3D场景) / GameConfig / ConfigLoader
│  │               # LevelSegmentModel(190关分段解锁) / FairySkillSystem(精灵技能) / TaskTracker(赛季任务)
│  ├─ UI/          # 全部 127 个面板的 1:1 复刻（UIPanel 基类 + 栈式 PanelManager）
│  │  ├─ Game/     #   玩法面板 GameplayPanel / GamePanel / GameFX(特效)
│  │  ├─ MainCity/ #   主城 MainCityPanel / 精力系统
│  │  ├─ Meta/     #   排行榜 RankViewPanel / 赛季 / 图鉴等
│  │  └─ Popups/   #   弹窗：分享 / 视频 / 道具引导 / 精力不足等
│  ├─ Platform/    # 平台抽象层：IPlatform → WeChatPlatform / PlaceholderPlatform
│  ├─ Spine/       # Spine-unity 运行时骨架加载（Resources/Config + Original/ui）
│  └─ System/      # SaveManager / OriginalAssets / AudioManager / SoundAssets / EventCenter
├─ Resources/
│  ├─ Config/      # 原版配置：PuzzleConfig / FairyConfig / ItemConfig / 赛季 + Spine 骨架 JSON
│  ├─ Original/    # 原版资源 3461 个（ui 贴图 / 花牌插画 / 背景）
│  └─ Audio/       # 原版音效：Bundle(96个WAV) + Item(16种花牌专属音效)
├─ Scenes/Demo.unity  # 唯一场景（运行时动态构建全部 UI/3D）
├─ Spine/          # spine-unity 4.x runtime（随仓库分发，无需包管理器下载）
└─ Editor/         # 批处理：BatchRun 冒烟 / DiagShot 截图 / 资源导出工具

minigame/          # 微信小游戏桥接：game.js(game.json) + openDataContext(好友榜)
```

## 数据驱动说明

- **关卡**：`Resources/Config/PuzzleConfig.json`（PuzzleCut/PuzzleStart/PuzzleShow 原版参数）
  - 显示关号 = 段内连续编号：`Id>=1000 ? Id%1000 : Id%100`（101→1 … 1100→100）
  - 关卡按精灵分段：1xx+11xx 同属精灵1，共 190 关
- **花牌**：`Resources/Config/ItemConfig.json`，贴图 `Resources/Original/ui/{花牌名}`，专属音效 `Resources/Audio/Item/{花牌名}.mp3`
- **3D 场景**：`Scene3D.cs` 按原版 level2 dump 复刻（相机 59° 正交俯视、49 格牌位 7×7、7 收集槽、墙与地板）

## 微信小游戏打包

1. 微信开发者工具新建小游戏项目，目录指向工程外的 `game` 构建产物（WebGL）
2. 桥接文件：`minigame/game.js` + `minigame/game.json`（复制到小游戏根目录）
3. 好友榜开放数据域：`minigame/openDataContext/index.js`（配套 `game.json` openDataContext 配置）
4. 代码层已通过 `IPlatform` 抽象，WebGL 构建自动切 `WeChatPlatform`，编辑器用 `PlaceholderPlatform`

## 常见二次开发入口

| 需求 | 位置 |
|---|---|
| 改关卡配置/加入新关卡 | `Resources/Config/PuzzleConfig.json`（配贴图切片） |
| 换 UI / 调坐标 | 各面板 `Assets/Scripts/UI/**`，坐标与 `tools/prefab_dump/*.txt` 一一对应 |
| 新增系统 | 挂 `UIPanel` 子类，用 `PanelManager.Push<T>()` 弹出 |
| 接自有广告/登录 | 实现 `IPlatform`，替换 `PlatformService.Resolve()` |