using UnityEngine;
using UnityEngine.SceneManagement; // 用于场景管理

public class GameManager : MonoBehaviour
{
    // --- 单例模式 ---
    public static GameManager Instance { get; private set; }

    // --- 游戏状态 ---
    public enum GameState { MainMenu, Playing, Paused, GameOver }
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
    }

    void Start()
    {
        // 游戏开始时，默认进入主菜单状态
        //SceneManager.LoadScene("MainFightScene");
        
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
    void OnEnable()
    {
        PlayerStates.OnPlayerDied += HandlePlayerDeath;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable()
    {
        PlayerStates.OnPlayerDied -= HandlePlayerDeath;
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
        }

        // 广播状态变化事件，通知所有监听者
        OnGameStateChanged?.Invoke(newState);
        Debug.Log("Game state changed to: " + newState);
    }

    // --- 提供一些公共方法给 UI 按钮或其他脚本调用 ---

    public void StartGame()
    {
        Debug.LogWarning("StartGame() 方法被调用了！即将加载 MainFightScene。", this.gameObject);
        //Debug.Break(); // 这是一个非常强大的调试工具！

        // 如果游戏当前不是在 MainMenu 状态，就阻止切换，以防万一
        if (currentState != GameState.MainMenu)
        {
            Debug.LogWarning("尝试在非 MainMenu 状态下启动游戏，已阻止。当前状态: " + currentState);
            return;
        }

        // 加载你的主战斗场景
        SceneManager.LoadScene("MainFightScene");
    
        // 我们需要在场景加载完成后再更新状态
        // 这里我们用一个简单的监听
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    // 场景加载完成后的回调函数
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 检查加载的是否是我们的主战斗场景
        if (scene.name == "MainFightScene")
        {
            // 场景加载完毕，现在可以安全地将状态切换到 Playing
            UpdateGameState(GameState.Playing);
        
            // （重要）取消订阅，避免重复调用
            SceneManager.sceneLoaded -= OnSceneLoaded;
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}