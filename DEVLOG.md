# Project ECHO 开发记录

> 每个确定性问题单独记录、单独提交，便于回溯。
> 记录格式：问题 → 原因 → 修改 → 验证。

## 2026-11-17

### #1 AbilityPool 抽卡死循环

- **状态**: 已修复，Unity 编译通过，待运行验证
- **文件**: `Assets/Projects/Scripts/Global/AbilityPool.cs`
- **问题**: `GetRandomAbilities()` 使用 `i--` 处理重复抽取。当某个品质池只剩 1 张，或可用能力总数少于请求数量时，循环无法前进，会导致 Unity 主线程卡死。
- **原因**: 用“抽到重复就重试”代替“从剩余集合中抽取”，且没有终止条件。
- **修改**:
  - 每轮只从尚未选中的能力中抽取。
  - 目标品质抽干后，按白 → 蓝 → 紫 → 红顺序回退到仍有存货的池。
  - 所有池都抽空时提前返回，允许返回数量少于请求数量。
  - 分拣时跳过空数据槽，避免 `NullReferenceException`。
- **验证**: 代码审查通过；Unity 编译通过（无 `error CS`），运行验证待完成。

### #2 游戏状态事件未触发 + 升级面板绕过状态机

- **状态**: 已修复，Unity 编译通过，待运行验证
- **文件**:
  - `Assets/Projects/Scripts/Global/GameManager.cs`
  - `Assets/Projects/Scripts/Global/UIManager.cs`
  - `Assets/Projects/Scripts/Global/LevelUpManager.cs`
- **问题**:
  1. `GameManager.OnGameStateChanged` 只有订阅、从未 `Invoke`，导致 `MoveController`、`ShootController`、`RoomController` 的 `canMove/canShoot/canOperate` 永远不更新，暂停时相关逻辑并没有真正停住。
  2. `LevelUpManager` 直接修改 `Time.timeScale` 和面板显隐，绕过 `GameManager`；升级面板打开时 `currentState` 仍是 `Playing`，按 ESC 会进入 `Paused`，造成状态错乱。
- **修改**:
  - `GameManager.UpdateGameState` 统一触发 `OnGameStateChanged`，移除对 `UIManager` 的直接调用，避免重复处理。
  - 新增 `GameState.InLevelUp`，由 `GameManager` 统一处理暂停与 UI 显示。
  - `LevelUpManager` 改用 `UpdateGameState(InLevelUp/Playing)`，并保留没有 `GameManager` 时的兜底逻辑。
  - 升级面板显示时保留 HUD。
- **验证**: 代码审查通过；Unity 编译通过（无 `error CS`），运行验证待完成。

### #3 ObjectPooler 以 GameObject.name 分池

- **状态**: 已修复，Unity 编译通过，待运行验证
- **文件**:
  - `Assets/Projects/Scripts/Global/ObjectPooler.cs`
  - `Assets/Projects/Scripts/Global/PooledObject.cs`（新增）
- **问题**: 池 key 使用 `prefab.name`，回收时再用 `obj.name` 找回池子。实例改名、预制体同名或对象被场景销毁后，可能回收到错误池；找不到 key 时会直接 `Destroy`。
- **修改**:
  - 新增 `PooledObject` 组件，在实例上记录来源预制体引用和是否在池中。
  - `ObjectPooler` 改为 `Dictionary<GameObject, Queue<GameObject>>`，按预制体引用分池。
  - 回收时通过 `PooledObject.SourcePrefab` 找回池子，重复回收会被 `IsInPool` 拦截。
  - 取出时跳过已被场景销毁的旧实例。
- **验证**: Unity 编译通过（无 `error CS`），运行验证待完成。

### #4 MapManager 未订阅的死代码

- **状态**: 已修复，Unity 编译通过
- **文件**: `Assets/Projects/Scripts/Map/MapManager.cs`
- **问题**: `OnSceneLoaded` 从未被 `SceneManager.sceneLoaded` 订阅，`PrepareAndDrawMapRoutine` 只被这个死方法调用；同时留下了无用的 `System.Collections` / `UnityEngine.SceneManagement` 引用。
- **修改**: 删除这两个方法及无用 using。地图绘制统一走 `UIManager.ShowMapPanel → MapManager.PrepareAndDrawMap` 的现有路径。
- **验证**: Unity 编译通过（0 errors）。删除的路径本来就不会执行，因此行为不变。
- **备注**: 如果后续发现地图面板刚打开时节点挤在一起，需要单独处理“等待一帧布局完成”的问题，而不是恢复这段死代码。

### #5 事件选项 healthCost 未生效

- **状态**: 已修复，Unity 编译通过，待运行验证
- **文件**:
  - `Assets/Projects/Scripts/Player/PlayerStates.cs`
  - `Assets/Projects/Scripts/Global/EventUI.cs`
- **问题**: `EventData.EventOption.healthCost` 只有配置和注释，选择选项时从未扣除生命值；如果代价致死，`CloseEvent` 还会把状态恢复成 `Playing`，覆盖 `GameOver`。
- **修改**:
  - 新增 `PlayerStates.ApplyHealthCost(int)`：直接扣血、不走防御减免，并统一广播血量变化/死亡事件。
  - `EventUI.SelectOption` 在 `healthCost > 0` 时调用。
  - `EventUI.CloseEvent` 在玩家已死亡时不再调用 `ResumeGame`。
- **验证**: Unity 编译通过（0 errors），运行验证待完成。

### #6 商店/事件地图节点接通

- **状态**: 已实现，Unity 编译与场景启动验证通过，完整流程待游玩验证
- **文件**:
  - `Assets/Projects/Scripts/Map/MapGenerator.cs`
  - `Assets/Projects/Scripts/Map/MapManager.cs`
  - `Assets/Projects/Scripts/Map/MapNode.cs`
  - `Assets/Projects/Scripts/Global/UIManager.cs`
  - `Assets/Projects/Scripts/Global/StoreUI.cs`
  - `Assets/Projects/Scripts/Global/EventUI.cs`
  - `Assets/Projects/Scripts/Global/RuntimeEventPanelBuilder.cs`（新增）
  - `Assets/Resources/Events/Event_FirstContact.asset`（新增）
- **功能**:
  - 地图生成时第 1 层保证一个商店、第 2 层保证一个事件，其余层按概率生成。
  - 商店节点直接打开现有商店面板，关闭后回到地图。
  - 事件节点打开事件面板，关闭后回到地图。
  - 事件池留空时自动从 `Resources/Events` 加载。
  - 场景缺少 `UIPanel_Event` 时，运行时自动创建一个基础事件面板。
- **验证**:
  - Unity 编译通过（0 errors）。
  - Play Mode 启动 `MainFightScene` 后，`Canvas/EventPanel` 自动创建并带 `UIPanel_Event` 标签。
  - Console 显示 `MapManager: 从 Resources/Events 加载了 1 个事件。`
  - 完整节点点击、商店购买、事件选项流程待手动游玩验证。
