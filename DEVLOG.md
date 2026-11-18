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

### #24 热量数值反馈与构建更新

- **状态**: 已实现，Windows 构建数据已更新
- **问题**: 旧构建的数据早于里程碑 2（exe 外壳时间不变，但 `ProjectEcho_Data` 还是 16:49），且热量条只有细线；日志里没有过热记录，玩家难以感知。
- **修改**: 热量标签改为 `热量 42/100`，按整数变化刷新；过热时显示“过热！”并变红。
- **验证**:
  - Unity 编译 0 errors。
  - Windows 构建数据的 `Assembly-CSharp.dll` / `globalgamemanagers` 时间更新到 17:47。

### #25 里程碑 3：商店/升级/事件的硬件获取与替换

- **状态**: 已实现并验证
- **修改**:
  - 新增 `RewardChoice`，统一能力/硬件的展示、内存标签、价格和品质色。
  - 新增 `HardwarePool`（核心/枪管/散热），挂在 GameManager；创建 2 个核心炉资产（标准/装甲）。
  - `LevelUpManager` 三选一可混合能力与硬件（`hardwareChance=0.35`）；`AbilityCardUI` 支持 `DisplayChoice`，硬件卡用金色强调。
  - `StoreInteractable` 库存混合能力/硬件（`hardwareChance=0.35`），跳过已装备/重复硬件；`StoreUI` 支持硬件购买与装备，装备失败退款。
  - `EventData.EventOption` 新增 `rewardHardware`，`EventUI` 支持装备硬件。
  - `PlayerData` 新增 `EquipHardware` / `IsHardwareEquipped`；核心换装会夹紧生命并刷新 HUD。
- **验证**:
  - Unity 编译 0 errors。
  - `hardwarePool` 正确指向 GameManager，三组列表已序列化到 MainMenu。
  - 升级三选一（临时硬件概率 1）出现长枪管/装甲核心炉 + 能力卡混排；点击长枪管后 `currentBarrel=HW_B_Rail`，伤害 25 / 射速 0.8 / 穿透 2，面板回到 Playing。
  - 商店库存/购买路径编译通过，待游玩商店确认。

### #26 修复新枪管子弹碰撞后残留

- **状态**: 已修复并验证
- **问题**: 新枪管弹速更高（长枪 30），子弹预制体是 Discrete 碰撞，薄碰撞体可能被穿透；遭遇结束后仍在飞的子弹会保留到 3 秒生命周期，在地图上可见。
- **修改**:
  - `Bullet.Awake` 设置 `ContinuousDynamic` 碰撞检测。
  - `RoomController` 在遭遇完成/换场时 `ClearProjectiles()`，回收所有玩家子弹和敌方投射物。
  - `ObjectPooler` 对缺失 `PooledObject` 的实例补组件并 `MarkOutOfPool()`，避免 `IsInPool` 残留导致不回收。
- **验证**:
  - Unity 编译 0 errors。
  - Play Mode 中子弹 `collisionDetectionMode=ContinuousDynamic`；遭遇结束后 `BulletTest(Clone)` 的 `isActive=false`（已回池）。
  - 长枪的穿透仍会让子弹在命中前 2 个敌人后继续飞，这是设计行为；地图/换场时会被清理。

### #27 里程碑 4：事件硬件样例、商店验收与热量平衡

- **状态**: 已实现并验证；按要求未构建
- **事件硬件样例**:
  - `Event_FirstContact` 新增第三个选项“拆下散热模块 (-5 HP)”，奖励 `HW_H_HighFlow` 高流散热器。
  - 实测选择后生命 100→95，当前散热器变为高流，热容 70。
- **商店路径验收**:
  - 将库存临时设为全硬件、20 件，首次进货覆盖全部 5 件未装备硬件；价格严格为核心 300 / 枪管 200 / 散热 150，无重复、无已装备硬件。
  - 购买长枪管：碎片 1000→800，枪管立即装备，按钮禁用且价格文本变为“已购买”。
  - 二次进货后已购买硬件不再出现，剩余硬件仍无重复、无已装备项。
- **热量平衡**:
  - 发现问题：标准脉冲管 5 热/发 + 高流散热器时，0.3s 冷却延迟会在 0.5s 射击间隔内把每发热量全部散掉，连续射击永远不会过热，等于绕过热量机制；且高流反而比标准散热器更能持续输出，定位相反。
  - 调整：标准脉冲管 `heatPerShot 5 → 8`；高流散热器 `coolingDelay 0.3 → 0.5s`。高流现在是“低热容先过热、但恢复最快”的短爆发散热器。
- **三套 Build 实测（4 倍时间缩放采样，热量/射速同比例）**:
  - 标准脉冲 + 高流：DPS≈20，过热 5.24s（理论 4.38s），恢复 1.21s。
  - 散射 + 标准：DPS≈27，过热 3.18s（理论 3.36s），恢复 2.01s。
  - 长枪 + 大容量：DPS≈20（穿透 2），过热 14.52s（理论 13.89s），恢复 3.00s。
- **验证**:
  - Unity 编译 0 errors；自动验证脚本全部通过后已删除，未留在工程中。
  - 事件、商店、三套 Build 均在 Play Mode 中实际触发；本轮未重复完整三层试玩，改动未触及楼层/Boss 流程。

### #28 Boss TTK 数据核对

- **状态**: 已核对，不需要改 Boss 血量
- **计算**:
  - `EnemyTest` 基础生命 15；第三层敌方倍率 `1 + 0.5 × (3 - 1) = 2`；Boss 再乘 20，最终 600 HP。
  - 标准脉冲 + 高流：爆发 20 DPS，5.24s 爆发 + 1.21s 恢复，有效 DPS≈16.2，TTK≈37s。
  - 散射 + 标准：爆发 27 DPS，3.18s + 2.01s，有效 DPS≈16.6，TTK≈36s。
  - 长枪 + 大容量：爆发 20 DPS，14.52s + 3.00s，有效 DPS≈16.5，TTK≈36s。
- **结论**: 三套 Build 对三层 Boss 的击杀时间都在 36–37 秒，差距小于 1 秒；当前 Boss 血量与热量数值匹配，暂不调整。碎片掉落仍为 10–20，商店价格已在 #27 验证。

### #29 热量平衡最终值（回退脉冲管热量）

- **状态**: 已实现并验证
- **调整**:
  - #27 中把标准脉冲管 `heatPerShot` 从 5 提到 8 属于中间试验；为保留已验收的起始手感，最终回退到 5。
  - 保留高流散热器 `coolingDelay 0.3 → 0.5s`。这样连续射击时不会在 0.5s 弹间歇里把热量全部散掉，高流重新有“低热容、短爆发、快恢复”的定位。
- **实测（4 倍时间缩放，热量/射速同比例）**:
  - 标准脉冲 + 高流：9.32s 过热，1.21s 恢复。
  - 标准脉冲 + 标准散热：11.84s 过热，2.02s 恢复。
  - 高流爆发更短但恢复快约 0.8s；标准更适合长窗口，二者有效 DPS 相差约 3.5%。
- **未变化**:
  - 散射 + 标准仍为 3.18s 过热 / 2.01s 恢复。
  - 长枪 + 大容量仍为 14.52s 过热 / 3.00s 恢复。
  - 按 #28 的 600 HP 计算，三套 Build 对三层 Boss 的有效 DPS 仍为 16.5–17.7，TTK 约 34–36 秒，Boss 血量不需要再调。

### #30 主场景黑白印刷风格覆盖（测试版）

- **状态**: 已接入并验证；仅风格测试，未构建
- **新增**:
  - `StyleSceneApplier`：运行时把 `Ground/Floor` 替换为 `PaperGrid`，玩家/敌人按原材质明度替换为黑白墨线；其余环境保留原材质，由全局 Volume 去饱和。
  - `PaperGrid` Shader：纸白底、细黑网格、5 格粗线、轻微棋盘明度差。
  - `STYLE OVERRIDE` 根对象：`StyleSceneApplier` + `Global Volume` + 无碰撞装饰（风扇、立柱、围挡、箱体、网点人群）。
  - F8 开关风格/原始材质；`Tools > ECHO > Remove Style From MainFightScene` 可整体移除。
- **发现并修正**:
  - 主场景环境几乎全压在一块 `Ground` 网格上，最初整体换成纸白后地面和空间参照消失；改为专用 `PaperGrid` 材质。
  - 主战斗 HUD 的 CanvasScaler 仍是 800×600 参考分辨率，在 1080p 下被放大到占屏；测试版临时改为 1920×1080，并把 HUD 文字改为墨黑。
  - 纯纸白背景 + 白色玩家会“白上加白”；因此环境保留原材质并整体去饱和，只把玩家/敌人换成黑白剪影。
  - Volume 最初没有真正生效：Profile 的组件未写成子资产，且场景里赋的是 `profile` 而不是 `sharedProfile`。已改为 `AddObjectToAsset` + `sharedProfile`，去饱和/对比度/暗角/颗粒现在实际参与渲染。
- **验证**:
  - Unity 编译 0 errors。
  - 从主菜单进入主战斗场景，StyleSceneApplier 日志正常，地面/角色/HUD/后处理均按预期切换。
  - 主场景动态预览：`Assets/Art/StyleTest/MainScenePreview.png`。
- **注意**:
  - 每 0.5s 扫描 Renderer 仅用于原型验证，正式版应在生成时挂接风格组件，避免全场景扫描。
  - 品红/彩色 VFX 是占位特效，后续替换为符合元素调色板的颜色。
  - HUD 仍是占位布局，正式 UI Kit 接入时重做。

### #31 黑白印刷风基础 UI Kit

- **状态**: 已实现并逐屏验证；未构建
- **架构**:
  - 新增 `UISkin`（`DefaultExecutionOrder(-1000)`）：在场景加载时重建当前场景所有可见面板的视觉层级，不替换玩法控制器。
  - 新增 `InkUI`：统一生成纸白 9-slice 卡片、按钮、Pill、文本、血条、热量条和硬阴影。
  - 新增 `InkButtonFeedback`、`InkPanelAnimator`、`HudView`、`GameOverView`、`MapHeaderView`。
  - `UIManager` 改为支持嵌套查找面板/按钮，并把热量 HUD 委托给 `HudView`。
  - `LevelUpManager` 在显示升级前自检并重新绑定卡片，避免 UI 重建后缓存旧引用。
  - `MapView`/`MapManager` 适配方形纸卡节点、透明墨线图标和红/黑/灰三态。
- **已覆盖的可见 UI**:
  - 主菜单：标题、副标题、开始/继续按钮、技术编号块、底部版本与提示。
  - 战斗 HUD：生命条与数值、RAM 内存条、碎片、热量条与过热状态、三个硬件槽（CORE/BARREL/HEAT SINK）。
  - 暂停：遮罩、暂停卡片、继续/主菜单按钮、ESC 提示。
  - 失败/通关结算：标题、本局摘要（层数/碎片/三槽硬件）、返回主菜单。
  - 地图：标题、层数/碎片、节点图、连线、当前/可选/锁定图例。
  - 升级三选一：标题、说明、三张能力/硬件卡、类型标签、名称、描述、安装提示。
  - 商店：标题、碎片、商品行（名称/类型/价格）、购买后“已购买”禁用态、离开按钮、装备提示。
  - 事件：标题、正文、事件图占位、分支按钮。
- **效果**:
  - 纸白圆角卡片 + 2px 墨黑描边 + 右下硬阴影。
  - 按钮悬停/键盘焦点反色；按下缩放；禁用变浅灰；鼠标/键盘焦点统一。
  - 面板/卡片入场淡入 + 0.98→1 缩放，使用 Unscaled Time；升级卡三张错峰出现。
  - 地图节点当前红色、可选墨黑、锁定浅灰；连线墨黑。
  - HUD 碎片和三个硬件槽实时刷新；热量过热变红。
  - 文本统一清洗中文标点/缺字字符，避免 TMP 方块。
- **修复/配套**:
  - `msyh SDF` 改为 Dynamic + 多图集，并补 msyh.ttc 源字体。
  - 四个地图图标 PNG 处理为透明底黑线图标。
  - `SW_Mag_B` 名称“贪婪漩涡”改为“贪婪涡流”，避开静态图集缺字。
  - 主战斗 HUD CanvasScaler 统一为 1920×1080。
  - 移除临时 `UITestDriver`；保留 `FontCoverageAudit` 字体审计菜单和 `UIWorkbench` 场景切换菜单。
- **验证**:
  - Unity 编译 0 errors。
  - 1920×1080 下逐屏截图验证：主菜单/HUD/暂停/升级/商店/事件/地图/失败/通关。
  - 商店购买：结构强化 50 碎片，碎片 300→250（实际还拾取了额外碎片），行禁用并显示“已购买”，商店保持 InStore。
  - 事件分支按钮可点击；地图节点生成、连线与三态正常。
  - UI 预览拼图：`Assets/Art/UI/InkUI_Preview.png`。
- **当前限制**:
  - UI 仍由运行时代码生成，尚未拆成正式 Prefab；美术替换 UI 时应迁移到 Prefab/Variant。
  - 不包含音效、正式图标和战斗特效。
  - 面板按 1920×1080 设计，超宽屏/低分辨率还需要单独验收。

## 2026-11-18

### #32 里程碑 5：冲锋兵与护盾兵

- **状态**: 已实现，Unity 编译与自动战斗验收通过；未构建
- **问题**:
  - 战斗遭遇缺少能主动逼迫玩家走位的冲锋敌人，以及需要方向处理的护盾敌人。
  - 护盾方向判定最初只覆盖子弹命中点；元素反应、持续区域等旧伤害接口会把世界原点误当成攻击来源。
  - 长枪弹拥有穿透次数，但物理碰撞响应会阻挡子弹，导致首个目标之后的敌人无法结算伤害。
- **原因**:
  - 旧近战 AI 只有追击和停止距离，没有蓄力、锁定冲锋、硬直与冲锋冷却。
  - `EnemyState` 的两参数兼容接口使用 `Vector3.zero`，不能表达真实伤害来源。
  - `Bullet` 只保留穿透次数，没有越过当前碰撞体并恢复直线飞行速度。
- **修改**:
  - 新增冲锋兵 `ChargerAI`：`Idle → Chase → Windup → Charge → Recover`；蓄力显示 `ChargeTelegraph`，冲锋方向锁定，单次冲锋最多伤害一次。
  - 新增护盾兵 `FrontShield`：按 `frontDot = 0.25` 判断正面，正面伤害乘 `0.2`，最低保留 1 点，并显示“格挡”反馈。
  - `EnemyState.TakeDamage` 增加带 `hitPoint` 的重载；旧重载在没有命中点时按非正面伤害处理。
  - `PhysicalReactionManager` 和 `TemporaryZoneLogic` 将伤害来源/区域坐标传给护盾方向判定。
  - `Bullet` 传入实际碰撞点；仍有穿透次数时越过当前碰撞体、恢复原飞行向量，继续攻击后方目标。
  - 新增 `Enemy_Charger.prefab`、`Enemy_Shield.prefab` 及占位材质；`StyleSceneApplier` 只增加 `telegraph` 名称过滤，避免蓄力预警被黑白材质扫描覆盖。
- **占位数值**:
  - 冲锋兵：25 HP，追击 2.8/20，触发距离 9，蓄力 0.65 秒，冲锋 14/0.45 秒，硬直 0.9 秒，冷却 2.5 秒，伤害 15，掉落 12–20。
  - 护盾兵：45 HP，移动 2.1/20，停止距离 26，攻击距离 2.2、间隔 2.2 秒、伤害 8，正面倍率 0.2，掉落 15–25。
- **遭遇接入**:
  - `Encounter_Start`：1 近战 + 1 冲锋 + 1 护盾。
  - `Encounter_Normal_01`：3 近战 + 2 冲锋 + 2 护盾。
  - `Encounter_Normal_02`：2 远程 + 2 冲锋 + 2 护盾。
  - `Encounter_Elite_001`：8 近战 + 2 远程 + 3 冲锋 + 2 护盾。
- **验证**:
  - Unity 强制重编译 0 errors、0 warnings；Prefab 扫描无 Missing Script，场景扫描无 Missing Reference。
  - 冲锋实测：蓄力到冲锋约 0.65 秒，预警按时出现并在冲锋时关闭；命中一次扣除 15 HP；硬直约 0.9 秒；冷却 2.5 秒后再次蓄力。
  - 冲锋撞墙：停止在墙体前，冲锋计时结束进入硬直，玩家未受伤。
  - 护盾直接伤害：正面 10→2、侧面 10→10、背面 10→10、正面 1→1；正面出现“格挡”反馈。
  - 真实子弹：标准脉冲正面 2、背面 10；长枪正面 5 并继续命中后方目标 25，共 30；穿透两个普通目标共 50；散射管生成 3 发。
  - 导电反应按来源方向命中正面时 10→2；旧无命中点接口保持全额 10，不再误用世界原点。
  - 掉落与拾取：冲锋兵实测 18 碎片，护盾兵实测 20 碎片，均成功拾取。
  - 起始遭遇完成并生成出口；普通 01、普通 02、精英 01 均完成清怪与关卡完成事件，运行时 Console 0 errors。
- **未覆盖**:
  - Boss、商店、事件、下一层完整流程未在本轮重新游玩；本次伤害与敌人改动未触及这些系统的控制流。
  - 两种新敌人同屏时的最终战斗压力、预警时长与占位可读性仍需人工手感验收。

### #33 护盾兵追击过快导致无法有效绕后

- **状态**: 已调整并完成运行时绕圈验证
- **问题**: 护盾兵移动速度为 2.1，且转向使用每物理帧固定 `Slerp(..., 0.1)`，会快速持续面朝玩家，使侧后方窗口过窄。
- **原因**:
  - 固定插值在 50 Hz 下约 1 秒可消除 99.5% 的角度差，实际转向接近瞬时。
  - 护盾兵原有移动速度虽低于玩家，但配合持续追踪仍会快速压缩绕后空间。
- **修改**:
  - `EnemyMoveControl` 新增 `turnSpeedDegreesPerSecond`；配置为正值时使用固定角速度 `Quaternion.RotateTowards`，未配置时继续使用原 `Slerp(..., 0.1)`，不改变既有普通近战敌人的手感。
  - `Enemy_Shield.prefab` 移动速度 `2.1 → 1.7`，转向速度设为 `60°/秒`。
  - `EnemyContentBuilder` 同步更新，避免以后重建护盾兵时恢复旧参数。
- **验证**:
  - 运行时按玩家移动速度 4 绕圈：
    - 半径 2.15 时，约 0.70 秒到达侧面，约 1.36 秒到达背面。
    - 半径 2.80 时，约 2.01 秒到达侧面，最大角度约 141°。
  - 运行时读取 Prefab 参数为移动速度 1.7、转向速度 60。
  - 验证完成后删除临时探针；最终强制编译 0 errors、0 warnings，Console 0 errors。

### #34 遭遇组合扩充

- **状态**: 已实现并通过运行时生成与完成流程验证
- **问题**: 普通遭遇只有两套，且已有精英 02 仍是“20 近战 + 10 远程”的旧怪海配置，没有利用新加入的冲锋兵和护盾兵形成战术差异。
- **修改**:
  - 新增 `Encounter_Normal_03`“机动封锁”：3 冲锋 → 3 护盾 → 2 近战，生成 2–3 个介质桶。
  - 新增 `Encounter_Normal_04`“火力掩护”：2 远程 → 2 护盾 → 3 冲锋，生成 2–4 个介质桶。
  - 将 `Encounter_Elite_002` 改为“重装突袭”：6 近战 → 2 远程 → 4 冲锋 → 3 护盾，生成 4–5 个介质桶，遭遇类型修正为 `EliteCombat`。
  - 新增 `EncounterVarietyBuilder`，可重复生成并注册以上配置，不触碰现有冲锋兵、护盾兵 Prefab。
  - 将普通 03、普通 04 加入 `MainMenu` 的普通遭遇池，并确认精英 02 已在精英遭遇池。
- **验证**:
  - 三个资产中的敌人 Prefab GUID 均可解析，地图池包含普通遭遇 4 套、精英遭遇 2 套。
  - 运行时逐一启动普通 03、普通 04、精英 02，三者均完成全部波次、触发遭遇完成并生成出口。
  - 运行时观察到三种配置分别包含冲锋兵/护盾兵组合；精英 02 包含近战、远程、冲锋兵、护盾兵四类敌人。
  - 临时验证探针已删除，Console 0 errors。

### #35 混合波次生成与完整流程回归

- **状态**: 已实现并通过混合波次、特殊遭遇与楼层流程验证
- **问题**: 原有 `RoomController.Wave` 每波只能配置一种敌人，所谓“组合遭遇”实际仍是按兵种顺序逐波刷新，无法形成同时存在的夹击压力。
- **修改**:
  - `RoomController.Wave` 新增 `mixedEnemies` 混编条目；每个条目可独立配置敌人 Prefab、数量和刷新间隔。
  - 新增 `SpawnMixedWave`：各兵种第一只敌人在同一帧开始生成，后续按各自间隔补充，全部刷完后等待整波清空。
  - 保留原 `enemyPrefab/enemyCount/spawnInterval` 路径；没有混编条目的旧遭遇继续使用原逻辑。
  - 普通 03“钳形攻势”改为两波混编：
    - 试探：1 冲锋 + 1 护盾 + 1 近战同波生成。
    - 增援：2 冲锋 + 2 护盾 + 1 近战同波生成。
  - `EncounterVarietyBuilder` 同步支持混编波次。
- **混合波次验证**:
  - `Encounter_Normal_03` 首帧同时出现 3 种敌人。
  - 两波共预期 8 个敌人，实际生成 8 个：3 冲锋、3 护盾、2 近战。
  - 清怪、遭遇完成事件和出口生成均正常。
  - `Encounter_Normal_04` 作为旧格式兼容用例，首帧仅出现一种敌人；实际生成 7 个并正常完成。
- **完整流程回归**:
  - 商店遭遇准备成功，商店交互物与普通出口正常生成。
  - 事件遭遇准备成功，事件交互物与普通出口正常生成。
  - Boss 遭遇完成，Boss 专用楼梯 `StairsToNextFloor` 正常生成。
  - 下一层升至第 2 层并打开地图；再下一层升至第 3 层并打开地图。
  - 越过第三层后进入 `Victory` 状态。
  - 本轮运行时 Console 0 errors。

### #36 构筑与装配系统设计留档

- **状态**: 设计决策已留档，尚未进入代码实现
- **问题**: 后续会持续增加物品、效果和装备组合，但现有效果接口只有装备/卸下回调，缺少触发上下文、叠加上限和空间装配能力；项目也没有背包和装备槽容量模型。
- **文档**: 新增 `BUILD_SYSTEM_DESIGN.md`，并在 `README.md` 增加入口。
- **已确认决策**:
  - 背包板是唯一装配空间，核心、枪管、散热器都放在同一板上。
  - 一个装备锚区可以安装多个组件，后期可通过扩容效果增加容量。
  - 相同组件的位置、相邻关系、方向和顺序可以改变触发效果。
- **设计内容**:
  - 物品定义、物品实例、背包状态、装备锚区、装配图和协同规则。
  - 统一效果定义、效果上下文、运行时效果和属性修改器账本。
  - 装配事务、扩容收敛、UI 边界和旧系统迁移原则。
  - 第一版 `8×8` 背包纵向切片及实施顺序。
