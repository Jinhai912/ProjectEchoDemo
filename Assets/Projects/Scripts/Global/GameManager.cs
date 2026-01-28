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
    // 1. 修改：添加了 InStore 状态
    public enum GameState { MainMenu, Playing, Paused, GameOver, MapSelection, InEvent, InStore }
    public GameState currentState;
    public int currentFloor { get; private set; } = 1;
    public int totalFloors = 3;//总共层数

    // --- 事件 ---
    public static event System.Action<GameState> OnGameStateChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerStates.OnPlayerDied += HandlePlayerDeath;
        PlayerStates.OnHealthChanged += HandleHealthChange; 
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
            // 2. 修改：允许在商店或事件中按 ESC 关闭界面回到游戏
            else if (currentState == GameState.InStore || currentState == GameState.InEvent)
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
                Time.timeScale = 1f; 
                break;
            case GameState.Playing:
                Time.timeScale = 1f; 
                break;
            case GameState.Paused:
                Time.timeScale = 0f; 
                break;
            case GameState.GameOver:
                Time.timeScale = 0f; 
                break;
            case GameState.MapSelection:
                Time.timeScale = 0f;
                break;
            
            // 3. 修改：处理商店和事件状态的时间流速
            case GameState.InEvent:
            case GameState.InStore:
                Time.timeScale = 0f; // 打开商店/事件时，游戏暂停
                break;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HandleGameStateChange(newState); 
        }
        
        Debug.Log("Game state changed to: " + newState);
    }

    // --- 提供一些公共方法给 UI 按钮或其他脚本调用 ---

    public void StartGame()
    {
        currentFloor = 1;
        Debug.Log("新游戏开始，进入第 1 层。");

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnSceneLoaded(scene, mode);
        }

        if (scene.name == "MainFightScene")
        {
            RoomController roomController = FindObjectOfType<RoomController>();

            if (roomController != null)
            {
                if (nextEncounter != null)
                {
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
        // 这里的逻辑适用于从 Paused, InStore, InEvent 返回游戏
        UpdateGameState(GameState.Playing);
    }

    public void GoToMainMenu()
    {
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.InitializeForNewRun(); 
        }
        if (MapManager.Instance != null)
        {
            MapManager.Instance.ClearMap();
        }
        
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void HandlePlayerDeath()
    {
        UpdateGameState(GameState.GameOver);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

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

    private void HandleHealthChange(int current, int max)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealthUI(current, max);
        }
    }

    public void GoToNextFloor()
    {
        currentFloor++;
        Debug.Log("--- 准备进入第 " + currentFloor + " 层！ ---");

        if (currentFloor > totalFloors)
        {
            WinTheEntireRun();
            return; 
        }

        if (MapManager.Instance != null)
        {
            MapManager.Instance.ClearMap();
        }
        else
        {
            Debug.LogWarning("GoToNextFloor: 找不到 MapManager 实例来清空地图。");
        }

        SceneManager.LoadScene("MainFightScene");
    }

    private void WinTheEntireRun()
    {
        Debug.Log("恭喜！你已通关整个周目！正在返回主菜单...");
        GoToMainMenu();
    }
}