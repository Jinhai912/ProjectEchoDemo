using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 用于重新加载场景
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [Header("UI 面板")]
    public GameObject mainMenuPanel;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;
    public GameObject inGameHUD; // 游戏中的平视显示器 (如血条、分数)
    public GameObject mapPanel;
    // --- 新增 HUD 元素引用 ---
    private Slider healthSlider;
    private TextMeshProUGUI healthText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SceneManager.sceneLoaded += OnSceneLoaded;
        GameManager.OnGameStateChanged += HandleGameStateChange;
        PlayerStates.OnHealthChanged += UpdateHealthUI;
    }

    // void OnEnable()
    // {
    //     // 订阅 GameManager 的状态变化事件
    //     GameManager.OnGameStateChanged += HandleGameStateChange;
    //     // --- 新增：监听场景加载事件 ---
    //     SceneManager.sceneLoaded += OnSceneLoaded;
    //     //订阅生命之变化事件
    //     PlayerStates.OnHealthChanged += UpdateHealthUI;
    // }

    // void OnDisable()
    // {
    //     // 取消订阅
    //     GameManager.OnGameStateChanged -= HandleGameStateChange;
    //     // --- 新增：取消监听 ---
    //     SceneManager.sceneLoaded -= OnSceneLoaded;
    //     PlayerStates.OnHealthChanged -= UpdateHealthUI;
    // }
    void OnDestroy()
    {
        // 确保在 UIManager 被销毁时（例如关闭游戏），取消所有订阅
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameManager.OnGameStateChanged -= HandleGameStateChange;
        PlayerStates.OnHealthChanged -= UpdateHealthUI;
    }

    // 当新场景加载完成时调用
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("UIManager 感知到新场景加载: " + scene.name);
        // 在新场景中重新寻找 UI 面板
        FindAndAssignUI();
    }

    // 一个专门用来寻找和分配 UI 元素的方法
    public void FindAndAssignUI()
    {
        // 使用标签来寻找 UI 面板是一种可靠的方式
        // 你需要为你的 UI 面板在 Inspector 中设置对应的标签
        try
        {
            mainMenuPanel = GameObject.FindWithTag("UIPanel_MainMenu");
            pauseMenuPanel = GameObject.FindWithTag("UIPanel_Pause");
            gameOverPanel = GameObject.FindWithTag("UIPanel_GameOver");
            inGameHUD = GameObject.FindWithTag("UIPanel_HUD");
            mapPanel = GameObject.FindWithTag("UIPanel_Map");
        }
        catch (UnityException e)
        {
            Debug.LogWarning("在当前场景中寻找 UI 面板时出错 (某些面板可能不存在): " + e.Message);
        }
        // --- 新增：寻找 HUD 元素 ---
        GameObject healthBarGO = GameObject.FindWithTag("UI_HealthBar");
        if (healthBarGO != null) healthSlider = healthBarGO.GetComponent<Slider>();

        GameObject healthTextGO = GameObject.FindWithTag("UI_HealthText");
        if (healthTextGO != null) healthText = healthTextGO.GetComponent<TextMeshProUGUI>();


    }
    // --- 新增：更新血条 UI 的方法 ---
    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        // 更新 Slider
        if (healthSlider != null)
        {
            // 计算比例 (0到1之间)
            healthSlider.value = (float)currentHealth / maxHealth;
        }

        // 更新文本
        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth;
        }
    }
    // 事件处理函数
    public void HandleGameStateChange(GameManager.GameState newState)
    {
        // 在操作前，先检查引用是否有效
        if (mainMenuPanel != null) mainMenuPanel.SetActive(newState == GameManager.GameState.MainMenu);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(newState == GameManager.GameState.Paused);
        if (gameOverPanel != null) gameOverPanel.SetActive(newState == GameManager.GameState.GameOver);
        if (inGameHUD != null) inGameHUD.SetActive(newState == GameManager.GameState.Playing);
        if (mapPanel != null) mapPanel.SetActive(newState == GameManager.GameState.MapSelection);
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

    /// <summary>
    /// 显示地图
    /// </summary>
    public void ShowMapPanel()
    {
        Debug.Log("--- ShowMapPanel 方法被调用 ---");
    
        if (mapPanel != null)
        {
            Debug.Log("mapPanel 引用正常，准备激活！当前激活状态: " + mapPanel.activeSelf);
            mapPanel.SetActive(true);
            Debug.Log("SetActive(true) 已执行。当前激活状态: " + mapPanel.activeSelf);

            // // (可选) 暂时隐藏游戏内的 HUD，避免重叠
            // if(inGameHUD != null) 
            // {
            //     inGameHUD.SetActive(false);
            //     Debug.Log("已隐藏 InGameHUD。");
            // }
        }
        else
        {
            // 如果执行到这里，就说明 FindWithTag 失败了
            Debug.LogError("ShowMapPanel 失败：mapPanel 变量是 null！请检查 'UIPanel_Map' 标签是否已正确设置在 MapPanel 对象上！");
        }
    }
    
    /// <summary>
    /// 隐藏地图
    /// </summary>
    public void HideMapPanel()
    {
        if (mapPanel != null) mapPanel.SetActive(false);
        // 恢复游戏时间的操作将由 GameManager 在加载新关卡时处理
    }
}