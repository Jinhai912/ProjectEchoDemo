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
        PlayerStates.OnPlayerDied += HandlePlayerDeath;
        // 让 GameManager 在切换场景时不被销毁
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 游戏开始时，默认进入主菜单状态
        // 假设你的初始场景就是主菜单
        UpdateGameState(GameState.MainMenu);
    }

    void OnDestroy()
    {
        PlayerStates.OnPlayerDied -= HandlePlayerDeath;
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
        // 假设你的游戏主场景名为 "MainLevel"
        // SceneManager.LoadScene("MainLevel"); 

        // 为了简单起见，我们先不切换场景，直接改变状态
        UpdateGameState(GameState.Playing);
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
        // SceneManager.LoadScene("MainMenu");
        UpdateGameState(GameState.MainMenu);
    }
    private void HandlePlayerDeath()
    {
        UpdateGameState(GameState.GameOver);
    }
}