using UnityEngine;
using UnityEngine.SceneManagement; // 用于场景管理

public class GameManager : MonoBehaviour
{
    [Header("关卡流程")]
    public EncounterData startingEncounter; // 在 Inspector 中拖入你的第一个关卡数据
    public EncounterData nextEncounter { get; private set; }

    // --- 单例模式 ---
    public static GameManager Instance { get; private set; }

    // --- 游戏状态 ---
    public enum GameState { MainMenu, Playing, Paused, GameOver, MapSelection}
    public GameState currentState;

    // --- 事件 ---
    // 定义事件，以便其他脚本可以响应状态变化
    public static event System.Action<GameState> OnGameStateChanged;

    void Awake()
    {
        // 标准的单例模式实现，确保全局只有一个 GameManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);



        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerStates.OnPlayerDied += HandlePlayerDeath;
        PlayerStates.OnHealthChanged += HandleHealthChange; // GameManager 也需要监听血量变化
    }

    void OnDestroy()
    {
        PlayerStates.OnPlayerDied -= HandlePlayerDeath;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerStates.OnHealthChanged -= HandleHealthChange;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }
    }

    // --- 核心方法：更新游戏状态 ---
    public void UpdateGameState(GameState newState)
    {
        currentState = newState;

        // 根据新状态执行相应的操作
        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f; // 确保时间流速正常
                // 在这里可以处理显示主菜单UI的逻辑
                break;
            case GameState.Playing:
                Time.timeScale = 1f; // 游戏开始，时间正常流动
                break;
            case GameState.Paused:
                Time.timeScale = 0f; // 游戏暂停，时间静止
                break;
            case GameState.GameOver:
                Time.timeScale = 0f; // 游戏结束，时间静止
                // 在这里可以处理显示游戏结束UI的逻辑
                break;
            case GameState.MapSelection:
                Time.timeScale = 0f;
                break;
        }

        if (UIManager.Instance != null)
        {
            // 直接命令 UIManager 更新它的显示状态
            UIManager.Instance.HandleGameStateChange(newState); // 假设方法名叫这个
        }
        // 广播状态变化事件，通知所有监听者
        //OnGameStateChanged?.Invoke(newState);
        Debug.Log("Game state changed to: " + newState);
    }

    // --- 提供一些公共方法给 UI 按钮或其他脚本调用 ---

    public void StartGame()
    {
        // --- 在开始新游戏时，重置玩家数据 ---
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.InitializeForNewRun();
        }
        else
        {
            Debug.LogError("找不到 PlayerData 实例！无法初始化玩家数据。");
        }
        
        StartEncounter(startingEncounter);
    }

    // 场景加载完成后的回调函数
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. 命令 UIManager 重新寻找UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnSceneLoaded(scene, mode);
        }

        // 2. 决定新场景的状态
        // 根据场景名决定初始状态
        if (scene.name == "MainFightScene")
        {
            // --- 核心修复：在这里启动第一场战斗 ---

            // 1. 找到当前场景的 RoomController
            RoomController roomController = FindObjectOfType<RoomController>();
            
            if (roomController != null)
            {
                // 2. 检查 GameManager 手里是否攥着下一关的数据
                if (nextEncounter != null)
                {
                    // 3. 命令 RoomController 用这份数据开始战斗！
                    roomController.StartEncounter(nextEncounter);
                }
                else
                {
                    Debug.LogError("GameManager: 场景已加载，但没有找到 nextEncounter 数据来开始战斗！");
                }
            }
            else
            {
                Debug.LogError("GameManager: 在 MainFightScene 中找不到 RoomController！无法开始战斗。");
            }

            // 4. 最后，将游戏状态设置为 Playing
            UpdateGameState(GameState.Playing);
        }
        else if (scene.name == "MainMenu")
        {
            UpdateGameState(GameState.MainMenu);
        }
    }

    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            UpdateGameState(GameState.Paused);
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            UpdateGameState(GameState.Playing);
        }
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void HandlePlayerDeath()
    {
        UpdateGameState(GameState.GameOver);
    }

    public void RestartGame()
    {
        // 恢复时间流速，以防是从暂停或地图界面过来的
        Time.timeScale = 1f;

        // 重新加载当前的战斗场景
        // GetActiveScene().name 会获取到 "MainFightScene"
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // OnSceneLoaded 事件会自动处理后续的状态切换 (切换到 Playing)
    }

    // --- 修改：创建一个新的方法来启动指定的关卡 ---
    public void StartEncounter(EncounterData encounterData)
    {
        if (encounterData == null)
        {
            Debug.LogError("尝试开始一个空的 Encounter! 检查 GameManager 的 startingEncounter 是否设置。");
            return;
        }

        nextEncounter = encounterData;
        SceneManager.LoadScene("MainFightScene");
    }

    // GameManager 负责监听血量变化，然后转告 UIManager
    private void HandleHealthChange(int current, int max)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealthUI(current, max);
        }
    }
    
}