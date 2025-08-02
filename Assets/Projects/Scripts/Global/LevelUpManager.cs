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

    // --- UI 引用 ---
    private GameObject levelUpPanel; 
    private List<AbilityCardUI> abilityCards = new List<AbilityCardUI>();                      
    
    private Transform player;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (abilityPool == null) abilityPool = GetComponent<AbilityPool>();
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

    private void FindLevelUpUI()
    {
        GameObject panelGO = GameObject.FindWithTag("UIPanel_LevelUp");
        if (panelGO != null)
        {
            levelUpPanel = panelGO;
            abilityCards.Clear();
            abilityCards.AddRange(levelUpPanel.GetComponentsInChildren<AbilityCardUI>(true)); // true: 包括非激活的子对象
            
            // 找到后，就立刻隐藏它，自己管理自己
            levelUpPanel.SetActive(false); 
        }
    }

    /// <summary>
    /// 入口方法：显示升级选项
    /// </summary>
    public void ShowLevelUpOptions()
    {
        // 健壮性检查
        if (levelUpPanel == null || abilityCards.Count == 0)
        {
            Debug.LogError("LevelUpManager 无法显示升级选项：UI 引用未找到！");
            return;
        }

        // 1. 暂停游戏
        Time.timeScale = 0f;

        // 2. 抽取能力并更新卡片 (这部分逻辑不变)
        List<AbilityData> chosenAbilities = abilityPool.GetRandomAbilities(3);
        for (int i = 0; i < abilityCards.Count; i++)
        {
            if (i < chosenAbilities.Count)
            {
                abilityCards[i].gameObject.SetActive(true);
                abilityCards[i].DisplayAbility(chosenAbilities[i]);
            }
            else
            {
                abilityCards[i].gameObject.SetActive(false);
            }
        }

        // 3. 直接、粗暴地激活面板！
        levelUpPanel.SetActive(true);
        Debug.Log("LevelUpManager 已手动激活 LevelUpPanel。");
    }

    /// <summary>
    /// 出口方法：选择能力
    /// </summary>
    public void SelectAbility(AbilityData selectedAbility)
    {
        // 应用能力 (逻辑不变)
        if (player != null)
        {
            player.GetComponent<PlayerStates>()?.ApplyAbility(selectedAbility);
        }

        // 1. 直接、粗暴地隐藏面板！
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
            Debug.Log("LevelUpManager 已手动隐藏 LevelUpPanel。");
        }

        // 2. 恢复游戏
        Time.timeScale = 1f;
    }
}