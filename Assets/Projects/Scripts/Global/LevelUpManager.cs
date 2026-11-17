// LevelUpManager.cs (回归本源，绝对能工作的版本)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelUpManager : MonoBehaviour
{
    // --- 单例模式 ---
    public static LevelUpManager Instance { get; private set; }

    [Header("组件引用")]
    public AbilityPool abilityPool;
    public HardwarePool hardwarePool;
    [Range(0f, 1f)] public float hardwareChance = 0.35f;

    // --- UI 引用 ---
    private GameObject levelUpPanel; 
    private List<AbilityCardUI> abilityCards = new List<AbilityCardUI>();                      
    
    private Transform player;

    public GameObject Panel => levelUpPanel;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (abilityPool == null) abilityPool = GetComponent<AbilityPool>();
        if (hardwarePool == null) hardwarePool = GetComponent<HardwarePool>();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 每次加载场景，都重新寻找玩家和UI
        if (scene.name == "MainFightScene")
        {
            FindLevelUpUI();
            FindPlayer();
        }
    }

    private void FindPlayer()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) player = playerGO.transform;
    }

    public void RefreshUIReferences()
    {
        FindLevelUpUI();
    }

    private void FindLevelUpUI()
    {
        GameObject panelGO = GameObject.FindWithTag("UIPanel_LevelUp");

        // 面板在场景加载过程中可能已经被隐藏，FindWithTag 找不到未激活对象
        if (panelGO == null)
        {
            AbilityCardUI[] cards = FindObjectsOfType<AbilityCardUI>(true);
            foreach (AbilityCardUI card in cards)
            {
                if (card == null || !card.gameObject.scene.IsValid()) continue;

                Transform current = card.transform;
                while (current != null)
                {
                    if (current.CompareTag("UIPanel_LevelUp"))
                    {
                        panelGO = current.gameObject;
                        break;
                    }
                    current = current.parent;
                }

                if (panelGO != null) break;
            }
        }

        if (panelGO == null)
        {
            Debug.LogError("LevelUpManager: 找不到 UIPanel_LevelUp！");
            return;
        }

        levelUpPanel = panelGO;
        abilityCards.Clear();
        abilityCards.AddRange(levelUpPanel.GetComponentsInChildren<AbilityCardUI>(true)); // true: 包括非激活的子对象

        // 找到后，就立刻隐藏它，自己管理自己
        levelUpPanel.SetActive(false);
    }

    /// <summary>
    /// 入口方法：显示升级选项
    /// </summary>
    public void ShowLevelUpOptions()
    {
        if (levelUpPanel == null || abilityCards.Count == 0 ||
            abilityCards.Exists(card => card == null))
        {
            FindLevelUpUI();
        }

        // 健壮性检查
        if (levelUpPanel == null || abilityCards.Count == 0)
        {
            Debug.LogError("LevelUpManager 无法显示升级选项：UI 引用未找到！");
            return;
        }

        if (abilityPool == null)
        {
            Debug.LogError("LevelUpManager 无法显示升级选项：AbilityPool 未设置！");
            return;
        }

        // 1. 切换到升级状态，由 GameManager 统一处理暂停与 UI 显示
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateGameState(GameManager.GameState.InLevelUp);
        }
        else
        {
            Time.timeScale = 0f;
            levelUpPanel.SetActive(true);
        }

        // 2. 抽取能力/硬件并更新卡片
        List<RewardChoice> chosenChoices = BuildChoices(3);
        for (int i = 0; i < abilityCards.Count; i++)
        {
            if (i < chosenChoices.Count)
            {
                abilityCards[i].gameObject.SetActive(true);
                abilityCards[i].DisplayChoice(chosenChoices[i]);
            }
            else
            {
                abilityCards[i].gameObject.SetActive(false);
            }
        }

        // 3. 无论 UIManager 是否持有引用，都确保升级面板可见
        levelUpPanel.SetActive(true);

        Debug.Log($"LevelUpManager 已显示升级选项，共 {chosenChoices.Count} 张卡。");
    }

    private List<RewardChoice> BuildChoices(int count)
    {
        List<RewardChoice> result = new List<RewardChoice>();
        HashSet<AbilityData> usedAbilities = new HashSet<AbilityData>();
        HashSet<HardwareData> usedHardware = new HashSet<HardwareData>();

        int attempts = 0;
        int maxAttempts = Mathf.Max(1, count * 20);

        while (result.Count < count && attempts < maxAttempts)
        {
            attempts++;

            if (hardwarePool != null && Random.value < hardwareChance)
            {
                HardwareData hardware = hardwarePool.GetRandomHardware();
                bool equipped = PlayerData.Instance != null && PlayerData.Instance.IsHardwareEquipped(hardware);
                if (hardware != null && !equipped && usedHardware.Add(hardware))
                {
                    result.Add(RewardChoice.FromHardware(hardware));
                    continue;
                }
            }

            List<AbilityData> abilities = abilityPool.GetRandomAbilities(1);
            if (abilities.Count > 0 && usedAbilities.Add(abilities[0]))
            {
                result.Add(RewardChoice.FromAbility(abilities[0]));
            }
        }

        if (result.Count < count)
        {
            List<AbilityData> fill = abilityPool.GetRandomAbilities(count - result.Count);
            foreach (var ability in fill)
            {
                if (usedAbilities.Add(ability)) result.Add(RewardChoice.FromAbility(ability));
            }
        }

        return result;
    }

    /// <summary>
    /// 选择统一奖励（能力或硬件）。
    /// </summary>
    public void SelectChoice(RewardChoice choice)
    {
        if (choice.IsHardware)
        {
            if (PlayerData.Instance != null)
            {
                bool equipped = PlayerData.Instance.EquipHardware(choice.hardware);
                Debug.Log(equipped
                    ? $"获得硬件: {choice.DisplayName}"
                    : $"硬件已装备或无效: {choice.DisplayName}");
            }
        }
        else if (choice.ability != null && player != null)
        {
            player.GetComponent<PlayerStates>()?.ApplyAbility(choice.ability);
            Debug.Log($"获得协议: {choice.DisplayName}");
        }

        CloseLevelUpPanel();
    }

    /// <summary>
    /// 出口方法：选择能力
    /// </summary>
    public void SelectAbility(AbilityData selectedAbility)
    {
        SelectChoice(RewardChoice.FromAbility(selectedAbility));
    }

    private void CloseLevelUpPanel()
    {
        // 1. 先隐藏自己持有的面板，避免 UIManager 没拿到引用时面板残留
        if (levelUpPanel != null) levelUpPanel.SetActive(false);

        // 2. 回到游戏状态，由 GameManager 统一恢复 timeScale 和 HUD
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateGameState(GameManager.GameState.Playing);
        }
        else
        {
            Time.timeScale = 1f;
        }

        Debug.Log("LevelUpManager 已关闭升级面板。");
    }
}
