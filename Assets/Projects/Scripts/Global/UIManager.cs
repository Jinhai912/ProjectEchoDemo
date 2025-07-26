using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 用于重新加载场景

public class UIManager : MonoBehaviour
{
    [Header("UI 面板")]
    public GameObject mainMenuPanel;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;
    public GameObject inGameHUD; // 游戏中的平视显示器 (如血条、分数)

    void OnEnable()
    {
        // 订阅 GameManager 的状态变化事件
        GameManager.OnGameStateChanged += HandleGameStateChange;
        // --- 新增：监听场景加载事件 ---
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // 取消订阅
        GameManager.OnGameStateChanged -= HandleGameStateChange;
        // --- 新增：取消监听 ---
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 当新场景加载完成时调用
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("UIManager 感知到新场景加载: " + scene.name);
        // 在新场景中重新寻找 UI 面板
        FindAndAssignUI();
    }

    // 一个专门用来寻找和分配 UI 元素的方法
    private void FindAndAssignUI()
    {
        // 使用标签来寻找 UI 面板是一种可靠的方式
        // 你需要为你的 UI 面板在 Inspector 中设置对应的标签
        try
        {
            mainMenuPanel = GameObject.FindWithTag("UIPanel_MainMenu");
            pauseMenuPanel = GameObject.FindWithTag("UIPanel_Pause");
            gameOverPanel = GameObject.FindWithTag("UIPanel_GameOver");
            inGameHUD = GameObject.FindWithTag("UIPanel_HUD");
        }
        catch (UnityException e)
        {
            Debug.LogWarning("在当前场景中寻找 UI 面板时出错 (某些面板可能不存在): " + e.Message);
        }

        
    }
    // 事件处理函数
    private void HandleGameStateChange(GameManager.GameState newState)
    {
        // 在操作前，先检查引用是否有效
        if (mainMenuPanel != null) mainMenuPanel.SetActive(newState == GameManager.GameState.MainMenu);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(newState == GameManager.GameState.Paused);
        if (gameOverPanel != null) gameOverPanel.SetActive(newState == GameManager.GameState.GameOver);
        if (inGameHUD != null) inGameHUD.SetActive(newState == GameManager.GameState.Playing);
        
    }

    // --- 公共方法，给 UI 按钮调用 ---

    public void OnStartGameButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }

    public void OnPauseButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
    }

    public void OnResumeButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }

    public void OnMainMenuButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMainMenu();
        }
    }
}