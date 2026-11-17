using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
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
    private TextMeshProUGUI gameOverTitleText;
    private Image heatBarFill;
    private TextMeshProUGUI heatLabel;
    private int _lastHeatDisplay = -1;
    private bool _lastOverheatDisplay;
    public TextMeshProUGUI memoryText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameManager.OnGameStateChanged += HandleGameStateChange;
        PlayerStates.OnHealthChanged += UpdateHealthUI;
        PlayerData.OnMemoryChanged += UpdateMemoryUI;
        PlayerData.OnHeatChanged += UpdateHeatUI;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameManager.OnGameStateChanged -= HandleGameStateChange;
        PlayerStates.OnHealthChanged -= UpdateHealthUI;
        PlayerData.OnMemoryChanged -= UpdateMemoryUI;
        PlayerData.OnHeatChanged -= UpdateHeatUI;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAndAssignUI();
    }

    public void FindAndAssignUI()
    {
        try
        {
            AssignIfFound(ref mainMenuPanel, GameObject.FindWithTag("UIPanel_MainMenu"));
            AssignIfFound(ref pauseMenuPanel, GameObject.FindWithTag("UIPanel_Pause"));
            AssignIfFound(ref gameOverPanel, GameObject.FindWithTag("UIPanel_GameOver"));
            AssignIfFound(ref inGameHUD, GameObject.FindWithTag("UIPanel_HUD"));

            if (gameOverPanel != null)
            {
                Transform overTransform = gameOverPanel.transform.Find("Over");
                if (overTransform != null)
                {
                    gameOverTitleText = overTransform.GetComponent<TextMeshProUGUI>();
                }
            }

            WireButton(pauseMenuPanel, "Resume", OnResumeButtonPressed);
            WireButton(pauseMenuPanel, "Main Menu", OnMainMenuButtonPressed);
            WireButton(gameOverPanel, "MainMenu", OnMainMenuButtonPressed);

            AssignIfFound(ref mapPanel, FindPanelByTagOrComponent<MapView>("UIPanel_Map"));
            AssignIfFound(ref levelUpPanel, FindPanelByTagOrComponent<AbilityCardUI>("UIPanel_LevelUp"));
            AssignIfFound(ref storePanel, FindPanelByTagOrComponent<StoreUI>("UIPanel_Store"));
            AssignIfFound(ref eventPanel, FindPanelByTagOrComponent<EventUI>("UIPanel_Event"));
            if (inGameHUD != null)
            {
                // transform.Find 是在子物体中按名字查找（注意名字要和 Hierarchy 里完全一致）
                Transform memTransform = inGameHUD.transform.Find("MemoryText");
                
                if (memTransform != null)
                {
                    memoryText = memTransform.GetComponent<TextMeshProUGUI>();
                    Debug.Log("UIManager: 已在 InGameHUD 中找到并关联 MemoryText");
                }
                else
                {
                    Debug.LogWarning("UIManager: 在 InGameHUD 下找不到名为 'MemoryText' 的物体！");
                }
            }
        }
        catch (UnityException e)
        {
            Debug.LogWarning("UI 查找警告: " + e.Message);
        }

        GameObject healthBarGO = GameObject.FindWithTag("UI_HealthBar");
        if (healthBarGO != null) healthSlider = healthBarGO.GetComponent<Slider>();

        GameObject healthTextGO = GameObject.FindWithTag("UI_HealthText");
        if (healthTextGO != null) healthText = healthTextGO.GetComponent<TextMeshProUGUI>();

        // 场景里没有事件面板时，创建一个运行时兜底面板，保证事件节点可用
        if (eventPanel == null && storePanel != null)
        {
            Canvas canvas = inGameHUD != null ? inGameHUD.GetComponentInParent<Canvas>() : null;
            if (canvas != null)
            {
                eventPanel = RuntimeEventPanelBuilder.Build(canvas.transform);
                eventPanel.tag = "UIPanel_Event";
                Debug.Log("UIManager: 场景缺少 EventPanel，已创建运行时事件面板。");
            }
        }

        if (storePanel != null && heatBarFill == null)
        {
            CreateHeatUI();
        }
        
        // 初始关闭所有不需要的面板
        if (storePanel) storePanel.SetActive(false);
        if (eventPanel) eventPanel.SetActive(false);
        if (memoryText != null && PlayerData.Instance != null)
        {
            UpdateMemoryUI(PlayerData.Instance.currentUsedMemoryMB, PlayerData.Instance.maxMemoryMB);
        }
    }

    private static void AssignIfFound(ref GameObject target, GameObject found)
    {
        if (found != null) target = found;
    }

    private GameObject FindPanelByTagOrComponent<T>(string tag) where T : Component
    {
        GameObject taggedPanel = GameObject.FindWithTag(tag);
        if (taggedPanel != null) return taggedPanel;

        // 面板在加载时可能已被隐藏，FindWithTag 找不到未激活对象
        T[] components = FindObjectsOfType<T>(true);
        foreach (T component in components)
        {
            if (component == null || !component.gameObject.scene.IsValid()) continue;

            Transform current = component.transform;
            while (current != null)
            {
                if (current.CompareTag(tag)) return current.gameObject;
                current = current.parent;
            }
        }

        return null;
    }

    private void CreateHeatUI()
    {
        if (inGameHUD == null) return;

        GameObject labelObj = new GameObject("HeatLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObj.transform.SetParent(inGameHUD.transform, false);
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.77f, 0.11f);
        labelRect.anchorMax = new Vector2(0.97f, 0.15f);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        heatLabel = labelObj.GetComponent<TextMeshProUGUI>();
        heatLabel.text = "热量 0/100";
        heatLabel.fontSize = 20f;
        heatLabel.alignment = TextAlignmentOptions.Right;
        heatLabel.color = Color.white;
        heatLabel.raycastTarget = false;

        TMP_FontAsset font = RuntimeFontUtility.GetFont();
        if (font != null) heatLabel.font = font;

        GameObject barObj = new GameObject("HeatBar", typeof(RectTransform), typeof(Image));
        barObj.transform.SetParent(inGameHUD.transform, false);
        RectTransform barRect = barObj.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.77f, 0.06f);
        barRect.anchorMax = new Vector2(0.97f, 0.10f);
        barRect.offsetMin = Vector2.zero;
        barRect.offsetMax = Vector2.zero;

        Image barBackground = barObj.GetComponent<Image>();
        barBackground.color = new Color(0.08f, 0.08f, 0.1f, 0.85f);
        barBackground.raycastTarget = false;

        GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(barObj.transform, false);
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);

        heatBarFill = fillObj.GetComponent<Image>();
        heatBarFill.type = Image.Type.Filled;
        heatBarFill.fillMethod = Image.FillMethod.Horizontal;
        heatBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        heatBarFill.fillAmount = 0f;
        heatBarFill.color = new Color(0.2f, 0.8f, 1f);
        heatBarFill.raycastTarget = false;

        if (PlayerData.Instance != null)
        {
            UpdateHeatUI(PlayerData.Instance.currentHeat, PlayerData.Instance.HeatCapacity, PlayerData.Instance.IsOverheated);
        }
    }

    private void UpdateHeatUI(float current, float max, bool overheated)
    {
        if (heatBarFill != null)
        {
            heatBarFill.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            heatBarFill.color = overheated
                ? new Color(1f, 0.3f, 0.2f)
                : new Color(0.2f, 0.8f, 1f);
        }

        if (heatLabel != null)
        {
            int displayHeat = Mathf.RoundToInt(current);
            int displayMax = Mathf.RoundToInt(max);

            if (displayHeat != _lastHeatDisplay || overheated != _lastOverheatDisplay)
            {
                heatLabel.text = overheated ? "过热！" : $"热量 {displayHeat}/{displayMax}";
                heatLabel.color = overheated
                    ? new Color(1f, 0.35f, 0.25f)
                    : Color.white;

                _lastHeatDisplay = displayHeat;
                _lastOverheatDisplay = overheated;
            }
        }
    }

    private void WireButton(GameObject panel, string buttonName, UnityAction action)
    {
        if (panel == null) return;

        Transform buttonTransform = panel.transform.Find(buttonName);
        if (buttonTransform == null) return;

        Button button = buttonTransform.GetComponent<Button>();
        if (button == null) return;

        // 场景里的持久化监听器目标丢失（m_Target: 0），这里在运行时重新绑定
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
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
                if (gameOverTitleText) gameOverTitleText.text = "GAME OVER";
                break;
            case GameManager.GameState.Victory:
                if (gameOverPanel) gameOverPanel.SetActive(true);
                if (gameOverTitleText)
                {
                    TMP_FontAsset victoryFont = RuntimeFontUtility.GetFont();
                    if (victoryFont != null) gameOverTitleText.font = victoryFont;
                    gameOverTitleText.text = "通关！";
                }
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
            case GameManager.GameState.InLevelUp:
                if (inGameHUD) inGameHUD.SetActive(true);
                if (levelUpPanel) levelUpPanel.SetActive(true);
                break;
        }
    }

    // --- 公共方法：打开商店 ---
    public void OpenStorePanel(List<RewardChoice> items, bool returnToMap = false)
    {
        if (storePanel == null)
        {
            Debug.LogError("UIManager: 找不到 UIPanel_Store，无法打开商店。");
            return;
        }

        // 获取 StoreUI 脚本并初始化
        StoreUI storeUI = storePanel.GetComponent<StoreUI>();
        if (storeUI != null)
        {
            storeUI.SetupStore(items, returnToMap);
        }

        GameManager.Instance.UpdateGameState(GameManager.GameState.InStore);
    }

    // --- 公共方法：打开事件 ---
    public void OpenEventPanel(EventData data, EventInteractable source, bool returnToMap = false)
    {
        if (eventPanel == null)
        {
            Debug.LogError("UIManager: 找不到 UIPanel_Event，无法打开事件。");
            return;
        }

        EventUI eventUI = eventPanel.GetComponent<EventUI>();
        if (eventUI != null)
        {
            eventUI.SetupEvent(data, source, returnToMap);
        }

        GameManager.Instance.UpdateGameState(GameManager.GameState.InEvent);
    }

    private void UpdateMemoryUI(int used, int max)
    {
        if (memoryText != null)
        {
            // 显示格式：RAM: 256 / 1024 MB
            memoryText.text = $"RAM: {used} / {max} MB";

            // 视觉反馈：如果超频(用量 > 上限)，变红；否则白色
            if (used > max) 
                memoryText.color = Color.red;
            else 
                memoryText.color = Color.white;
        }
    }

    // ... (保留你原来的 OnStartGameButtonPressed 等方法) ...
    public void OnStartGameButtonPressed() { if (GameManager.Instance != null) GameManager.Instance.StartGame(); }
    public void OnPauseButtonPressed() { if (GameManager.Instance != null) GameManager.Instance.PauseGame(); }
    public void OnResumeButtonPressed() { if (GameManager.Instance != null) GameManager.Instance.ResumeGame(); }
    public void OnMainMenuButtonPressed()
    {
        Debug.Log("UIManager: 返回主菜单。");
        if (GameManager.Instance != null) GameManager.Instance.GoToMainMenu();
    }
    public void ShowMapPanel() { if (MapManager.Instance != null) MapManager.Instance.PrepareAndDrawMap(); GameManager.Instance.UpdateGameState(GameManager.GameState.MapSelection); }
    public void HideMapPanel() { if (mapPanel != null) mapPanel.SetActive(false); }
}
