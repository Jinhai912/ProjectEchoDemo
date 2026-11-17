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

### #7 回归修复：升级选项不显示导致卡关

- **状态**: 已修复，Unity 编译与场景引用验证通过，待游玩确认
- **文件**:
  - `Assets/Projects/Scripts/Global/LevelUpManager.cs`
  - `Assets/Projects/Scripts/Global/UIManager.cs`
- **问题**: `LevelUpPanel` 在场景中初始为激活，`LevelUpManager` 找到后立即隐藏。之后 `UIManager` / `LevelUpManager` 再用 `GameObject.FindWithTag` 查找时，未激活对象找不到，引用变成 `null`。升级时 `UIManager` 没有面板引用，`LevelUpManager` 又只在 `GameManager == null` 时激活面板，导致面板不显示、`timeScale = 0` 卡住。
- **修改**:
  - `LevelUpManager` 增加 `Panel` 属性，并通过 `AbilityCardUI` 反向查找面板，即使面板已隐藏也能拿到引用。
  - `ShowLevelUpOptions` 结尾无条件 `levelUpPanel.SetActive(true)`。
  - `SelectAbility` 先无条件隐藏面板，再切回 `Playing`。
  - `UIManager` 优先使用 `LevelUpManager.Panel`，并用卡片反向查找兜底。
- **验证**:
  - Unity 编译通过（0 errors）。
  - Play Mode 启动战斗场景后，`LevelUpManager.Panel` 指向 `LevelUpPanel`，三张 `AbilityCard` 均存在，Console 0 errors。
  - 实际升级三选一待游玩确认。

### #8 修正商店/事件节点流程：进入遭遇房间，而不是直接弹面板

- **状态**: 已修正，Unity 编译通过，战斗场景启动验证通过，商店交互待验证
- **问题**: #6 把商店/事件节点做成了在地图界面上直接打开面板。项目的原设计是节点进入 `MainFightScene`，由 `RoomController` 激活场景里已有的 `Store_Table_Test` / `Event_Console`，玩家按 E 交互，再从出口回地图。
- **修改**:
  - `MapGenerator` 为 Store/Event 节点创建对应 `encounterType` 的运行时 `EncounterData`。
  - `MapManager` 统一把 Store/Event 交给 `RoomController.StartEncounter`，不再直接开面板。
  - `RoomController` 按遭遇类型激活商店/事件交互物，不刷怪；`OnEncounterComplete` 用于生成出口。
  - `RewardManager` 对非战斗遭遇跳过宝箱，只生成出口。
  - `StoreUI` 在引用为空时运行时构建商品列表、商品按钮和关闭键，修复 `ShopPanel` 空壳。
  - `UIManager` 的面板查找改为包含未激活对象，并避免重复查找把引用覆盖成 `null`。
- **验证**:
  - Unity 编译通过（0 errors）。
  - 战斗遭遇启动后 `Store_Table_Test` / `Event_Console` / `ShopPanel` 均保持关闭，Console 0 errors。
  - 商店交互的实际 UI 待验证（当前 `guide` 模式阻止了自动测试菜单）。

### #9 商店/事件流程验证与中文字体 fallback

- **状态**: 已验证，Unity 编译通过
- **验证结果**:
  - 商店：`RoomController 开始关卡: 商店` → `特殊遭遇已准备完成: Store` → `打开商店界面` → `InStore` → 点击商品（`钱不够！`）→ 关闭 → `Playing` → 出口 → `MapSelection`。
  - 事件：`RoomController 开始关卡: 事件` → `特殊遭遇已准备完成: Event` → `InEvent` → 出口 → `MapSelection`。
  - Console 0 errors；运行时中文缺字警告已定位。
- **修改**: 在 `Assets/TextMesh Pro/Resources/TMP Settings.asset` 的 `m_fallbackFontAssets` 加入 `msyh SDF`（GUID `1fc33eee782440942a13fc2aded163c4`），让默认字体缺字时回退到中文字体。
- **待办**: 玩家初始碎片为 0，商店只能提示“钱不够”。需要决定初始碎片、敌人掉落或事件奖励中的一种来源。

### #10 初始碎片与运行时中文字体

- **状态**: 已实现，Unity 编译通过
- **修改**:
  - `PlayerData` 新增 `startingCurrency`（默认 300），`InitializeForNewRun` 用它初始化 `currentCurrency`。
  - 新增 `RuntimeFontUtility`，运行时创建的 UI 文本直接使用 TMP 全局 fallback 列表里的中文字体（当前为 `msyh SDF`），不再依赖默认 LiberationSans 的逐字回退。
  - `StoreUI` / `EventUI` / `RuntimeEventPanelBuilder` 创建文本时统一调用该工具。
- **验证**: Unity 编译 0 errors；`startingCurrency` 在 Inspector 中为 300。中文显示待再跑一次商店确认。

### #11 打通垂直切片主流程：Boss → 下一层 → 通关结算

- **状态**: 已实现并验证
- **问题**: `GoToNextFloor` 重载 `MainFightScene` 后，`OnSceneLoaded` 会再次启动 `nextEncounter`（上一个 Boss），导致无法进入新一层；`WinTheEntireRun` 只回主菜单，没有结算界面。
- **修改**:
  - `GameManager` 新增 `showMapAfterSceneLoad`：Boss 楼梯触发 `GoToNextFloor` 时清空 `nextEncounter`，重载场景后直接打开新一层地图。
  - 新增 `GameState.Victory`；`WinTheEntireRun` 切换到 Victory，不再直接回主菜单。
  - `UIManager` 复用 `GameOverPanel` 作为结算面板，Victory 时标题改为“通关！”，GameOver 时恢复“GAME OVER”。
  - `GoToMainMenu` / `StartEncounter` / `RestartGame` 清理过渡标记，避免残留状态。
- **验证**:
  - Play Mode 中临时按钮连续触发 `GoToNextFloor` 三次：
    - 第一次后 `currentState = MapSelection`，地图层数 6；
    - 第二次后地图层数 7；
    - 第三次后 `currentState = Victory`，`GameOverPanel` 激活，`Over` 文本为“通关！”。
  - Console 0 errors。

### #12 敌人掉落 10–20 碎片

- **状态**: 已实现并验证
- **修改**:
  - `EnemyState` 新增 `currencyMin = 10` / `currencyMax = 20`，死亡时生成碎片掉落物。
  - 新增 `CurrencyGem`（继承 `Pickup`）：飞向玩家、拾取后增加碎片并弹出 `+X 碎片` 反馈；30 秒未拾取自动销毁，并带 `TemporaryEncounterObject` 以便换场清理。
  - 掉落物先用运行时生成的小光球作为占位表现，后续替换正式模型/特效。
- **验证**: Play Mode 起始敌人被自动击杀后，`currentCurrency` 从 300 变为 316（掉落 16），`CurrencyGem` 已被拾取，Console 0 errors。

### #13 修复死亡后主菜单按钮无响应

- **状态**: 已修复并验证
- **问题**: `GameOverPanel` 和暂停菜单的 “Main Menu” 按钮，场景里的持久化监听器 `m_Target` 为 `{fileID: 0}`，点击不会调用 `UIManager.OnMainMenuButtonPressed`；日志中也没有对应调用记录。
- **修改**: `UIManager.FindAndAssignUI` 在运行时找到两个面板里的 Main Menu 按钮，`RemoveAllListeners` 后重新绑定 `OnMainMenuButtonPressed`；`OnMainMenuButtonPressed` 增加日志。
- **验证**: Play Mode 中打开暂停菜单，触发 `Main Menu` 按钮，Console 输出 `UIManager: 返回主菜单。`，场景切换到 MainMenu。GameOver 面板复用同一绑定逻辑。
- **备注**: 测试时在 MainMenu 未保存场景里留下了一个 `Canvas/__TestKillPlayer` 测试按钮，需要删除或重载场景丢弃，不要保存。

### #14 垂直切片收尾：Boss 强度与楼层成长

- **状态**: 已实现，Unity 编译通过，待游玩验证
- **问题**: `Encounter_Boss_01` 的敌人引用是普通近战怪（15 HP），两枪就死；普通/精英遭遇也不随楼层成长，三层难度几乎一样。
- **修改**:
  - `Encounter_Boss_01.encounterType` 改为 `Boss`。
  - `RoomController.SpawnEnemy` 增加 `ApplyDifficultyScaling`：
    - 每层敌人生命乘以 `1 + 0.5 × (floor - 1)`；
    - Boss 额外乘以 20，并放大 1.8 倍、染红作为占位表现。
- **验证**: Unity 编译 0 errors；`Encounter_Boss_01` 类型确认为 `Boss`。实际战斗强度待游玩验证。

### #15 垂直切片收尾：交互物复用重置与暂停 Resume

- **状态**: 已实现并验证
- **问题**:
  - `Event_Console` 被不同事件房间复用，`hasInteracted` 不重置，第二个事件节点点不开。
  - `Store_Table_Test` 被不同商店房间复用，`isStockGenerated` 不重置，第二家商店沿用上一家库存和已购状态。
  - 暂停菜单 `Resume` 按钮的持久化监听器 `m_Target` 也是 0，点击无响应，与 Main Menu 按钮同一类问题。
- **修改**:
  - `EventInteractable.OnEnable` 重置 `hasInteracted`。
  - `StoreInteractable.OnEnable` 重置 `isStockGenerated` 并清空 `currentStock`。
  - `UIManager` 的按钮绑定改为通用 `WireButton`，同时绑定暂停菜单 `Resume`、`Main Menu` 和 GameOver 的 `MainMenu`。
- **验证**:
  - Unity 编译 0 errors。
  - Play Mode 中暂停后触发 `Resume`，`currentState` 从 `Paused` 回到 `Playing`。

### #16 清理未使用的空壳/调试脚本

- **状态**: 已完成
- **删除**: `EventManager.cs`（空壳，无场景引用）、`SimpleUITester.cs`（调试遗留，无场景引用）。
- **验证**: Unity 编译 0 errors。

### #17 性能收尾：索敌、GC 与对象池

- **状态**: 已实现并验证
- **修改**:
  - `Shootable` 增加全局注册表；`ShootController` 改为 10Hz 重寻，不再每帧 `FindObjectsOfType`。
  - `EnemyMoveControl` 缓存玩家 Transform，移除每个敌人 0.5s 的 `FindObjectsOfType<Attackable>`。
  - `StatusContainer` 用预分配 List 缓冲状态 key，移除每帧 `Keys.ToList()`。
  - `PlayerStates` 移除每帧 Inspector 同步，改为数据变化时同步。
  - `RoomController` 增加静态 Instance；`EnemyState` 优先从 Instance 取，减少生成时的全场景查找。
  - 伤害飘字、拾取反馈、经验球接入 `ObjectPooler`；飘字/反馈用计时器回收，不再 `Instantiate/Destroy`。
  - `Bullet` 用计时器替代每发协程/`WaitForSeconds`。
  - `PlayerInteractor` / `PhysicalReactionManager` 的 `OverlapSphere` 改为 `NonAlloc`。
  - 缓存 CurrencyGem 的共享材质，移除近战命中日志刷屏。
- **验证**: Unity 编译 0 errors；Play Mode 自动击杀起始敌人，碎片 300→316，无 Console errors，掉落物正常拾取。

### #18 性能收尾二：敌方投射物池化 + Windows 构建

- **状态**: 已实现并验证
- **修改**:
  - `Shootable` 缓存 `IsBarrel`，自动索敌不再每次 `GetComponent<MediaBarrel>`。
  - `StatusContainer` 无状态时跳过计时更新。
  - `ObjectPooler` 先设置位姿再激活，保证池化对象 `OnEnable` 读到正确朝向。
  - `EnemyProjectile` 改为池化 + 计时器回收；`ArtilleristAI` 从对象池发射。
- **验证**:
  - Unity 编译 0 errors。
  - Windows 构建成功：`Builds/Windows/ProjectEcho.exe`。
  - 敌方投射物池化待游玩远程敌人确认。

### #19 性能收尾三：碎片光球对象池

- **状态**: 已实现并验证
- **修改**:
  - `EnemyState` 用运行时模板 + `ObjectPooler` 生成碎片光球，材质共享。
  - `CurrencyGem` 改为 30s 计时回收，拾取/超时都回池。
- **验证**: Play Mode 起始敌人击杀，碎片 300→313，Console 0 errors；池中保留未激活碎片实例。

### #20 性能验收快照与 Windows 构建

- **状态**: 已完成
- **Profiler（Play Mode 起始战斗场景）**:
  - frameTime: 6.9–12.0 ms
  - batches / drawCalls: 38 / 38
  - triangles: 15,596
  - totalAllocated: ~300 MB
- **Windows 构建**: `Builds/Windows/ProjectEcho.exe`（StandaloneWindows64，两个场景均已打包）。

### #21 修复物理反应飘字缺字

- **状态**: 已修复
- **问题**: 反应飘字使用 `【】`，LiberationSans 与 msyh SDF 的字形表都没有该字符，显示为方块并产生 TMP Warning。
- **修改**: 改为 ASCII `[传导]` / `[过载]` / `[气化]` / `[爆燃]`，中文继续走 msyh fallback。
- **验证**: Unity 编译 0 errors；待触发物理反应确认。

### #22 里程碑 1：硬件三槽数据层与过热逻辑

- **状态**: 已实现，Unity 编译通过，ScriptableObject API 验证通过
- **新增**:
  - `HardwareData` 基类 + `HardwareSlot`；核心炉/枪管继承，新增 `HardwareHeatSinkSO`。
  - 枪管新增弹形字段：`projectilesPerShot` / `spreadAngle` / `bulletSpeed` / `pierce`。
  - `PlayerData` 新增 `currentHeatSink`、`startingCore/Barrel/HeatSink`、热量/过热状态、热容/散热/冷却延迟/过热恢复、`AddHeat` / `ResetHeat` / `EquipHeatSink`、`OnHeatChanged`。
  - 新开局重置硬件与热量。
  - 样本散热器 `Assets/Data/Hardware/HeatSinks/HW_H_Standard.asset`。
- **验证**:
  - Unity 编译 0 errors。
  - 旧枪管资产字段完整，`Slot=Barrel`，`DisplayName` 正常。
  - ScriptableObject API 创建散热器并批量写入 7 个字段成功；`Slot=HeatSink`，`DisplayName=标准散热器`。

### #23 里程碑 2：枪管弹形 + 热量/过热接入射击

- **状态**: 已实现并验证
- **修改**:
  - `ShootController` 读取当前枪管的弹数/扇形/弹速/穿透/子弹预制体和每发热量，能力卡加成叠加。
  - 射击累积热量，过热时锁定射击；`PlayerData` 负责冷却与恢复，枪管/散热参数生效。
  - `Bullet` 使用 `FinalPierce`。
  - 每个房间开始重置热量。
  - `UIManager` 增加占位热量条/过热红条（正式表现留给用户）。
  - 添加散射管、长枪管、高流散热器、大容量散热器资产；场景起始枪管=标准脉冲管，散热器=标准散热器。
- **验证**:
  - Unity 编译 0 errors。
  - 起始装备生效，热量条出现。
  - 临时加热两次：热量 100、`isOverheated=true`、HeatLabel=`过热！`、`CanFire=false`；3 秒后恢复 `0/false/true`。
  - 运行时切散射管：伤害 6 / 射速 1.5 / 3 弹 / 热 8；高流散热器：热容 70 / 散热 45。
  - 运行时切长枪管：伤害 25 / 射速 0.8 / 穿透 2 / 热 18。
