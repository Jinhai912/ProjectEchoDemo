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
