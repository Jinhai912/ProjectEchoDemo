using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI 面板")]
    public GameObject mainMenuPanel;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;
    public GameObject inGameHUD;
    public GameObject mapPanel;
    private GameObject levelUpPanel; // 如果你需要它，记得Find
    // --- 新增面板 ---
    private GameObject storePanel;
    private GameObject eventPanel;

    [Header("HUD 元素")]
    private Slider healthSlider;
    private TextMeshProUGUI healthText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameManager.OnGameStateChanged += HandleGameStateChange;
        PlayerStates.OnHealthChanged += UpdateHealthUI;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameManager.OnGameStateChanged -= HandleGameStateChange;
        PlayerStates.OnHealthChanged -= UpdateHealthUI;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAndAssignUI();
    }

    public void FindAndAssignUI()
    {
        try
        {
            mainMenuPanel = GameObject.FindWithTag("UIPanel_MainMenu");
            pauseMenuPanel = GameObject.FindWithTag("UIPanel_Pause");
            gameOverPanel = GameObject.FindWithTag("UIPanel_GameOver");
            inGameHUD = GameObject.FindWithTag("UIPanel_HUD");
            mapPanel = GameObject.FindWithTag("UIPanel_Map");
            levelUpPanel = GameObject.FindWithTag("UIPanel_LevelUp");
            
            // --- 新增：寻找商店和事件面板 ---
            // 记得去 Unity 里给 Panel 设置 Tag！
            storePanel = GameObject.FindWithTag("UIPanel_Store");
            eventPanel = GameObject.FindWithTag("UIPanel_Event");
        }
        catch (UnityException e)
        {
            Debug.LogWarning("UI 查找警告: " + e.Message);
        }

        GameObject healthBarGO = GameObject.FindWithTag("UI_HealthBar");
        if (healthBarGO != null) healthSlider = healthBarGO.GetComponent<Slider>();

        GameObject healthTextGO = GameObject.FindWithTag("UI_HealthText");
        if (healthTextGO != null) healthText = healthTextGO.GetComponent<TextMeshProUGUI>();
        
        // 初始关闭所有不需要的面板
        if (storePanel) storePanel.SetActive(false);
        if (eventPanel) eventPanel.SetActive(false);
    }

    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthSlider != null) healthSlider.value = (float)currentHealth / maxHealth;
        if (healthText != null) healthText.text = currentHealth + " / " + maxHealth;
    }

    public void HandleGameStateChange(GameManager.GameState newState)
    {
        // 1. 隐藏所有
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (inGameHUD) inGameHUD.SetActive(false);
        if (mapPanel) mapPanel.SetActive(false);
        if (levelUpPanel) levelUpPanel.SetActive(false);
        // --- 新增隐藏 ---
        if (storePanel) storePanel.SetActive(false);
        if (eventPanel) eventPanel.SetActive(false);

        // 2. 激活特定
        switch (newState)
        {
            case GameManager.GameState.MainMenu:
                if (mainMenuPanel) mainMenuPanel.SetActive(true);
                break;
            case GameManager.GameState.Playing:
                if (inGameHUD) inGameHUD.SetActive(true);
                break;
            case GameManager.GameState.Paused:
                if (pauseMenuPanel) pauseMenuPanel.SetActive(true);
                break;
            case GameManager.GameState.GameOver:
                if (gameOverPanel) gameOverPanel.SetActive(true);
                break;
            case GameManager.GameState.MapSelection:
                if (mapPanel) mapPanel.SetActive(true);
                break;
            // --- 新增状态处理 ---
            case GameManager.GameState.InStore:
                if (storePanel) storePanel.SetActive(true);
                break;
            case GameManager.GameState.InEvent:
                if (eventPanel) eventPanel.SetActive(true);
                break;
        }
    }

    // --- 公共方法：打开商店 ---
    public void OpenStorePanel(List<AbilityData> items)
    {
        if (storePanel == null) return;

        // 获取 StoreUI 脚本并初始化
        StoreUI storeUI = storePanel.GetComponent<StoreUI>();
        if (storeUI != null)
        {
            storeUI.SetupStore(items);
        }

        GameManager.Instance.UpdateGameState(GameManager.GameState.InStore);
    }

    // --- 公共方法：打开事件 ---
    public void OpenEventPanel(EventData data, EventInteractable source)
    {
        if (eventPanel == null) return;

        EventUI eventUI = eventPanel.GetComponent<EventUI>();
        if (eventUI != null)
        {
            eventUI.SetupEvent(data, source);
        }

        GameManager.Instance.UpdateGameState(GameManager.GameState.InEvent);
    }

    // ... (保留你原来的 OnStartGameButtonPressed 等方法) ...
    public void OnStartGameButtonPressed() { if (GameManager.Instance != null) GameManager.Instance.StartGame(); }
    public void OnPauseButtonPressed() { if (GameManager.Instance != null) GameManager.Instance.PauseGame(); }
    public void OnResumeButtonPressed() { if (GameManager.Instance != null) GameManager.Instance.ResumeGame(); }
    public void OnMainMenuButtonPressed() { if (GameManager.Instance != null) GameManager.Instance.GoToMainMenu(); }
    public void ShowMapPanel() { if (MapManager.Instance != null) MapManager.Instance.PrepareAndDrawMap(); GameManager.Instance.UpdateGameState(GameManager.GameState.MapSelection); }
    public void HideMapPanel() { if (mapPanel != null) mapPanel.SetActive(false); }
}