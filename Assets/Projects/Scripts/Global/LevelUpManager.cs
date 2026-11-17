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

        // 3. 兜底：没有 GameManager 时仍然确保面板可见
        if (GameManager.Instance == null) levelUpPanel.SetActive(true);

        Debug.Log("LevelUpManager 已显示升级选项。");
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

        // 1. 回到游戏状态，由 GameManager 统一恢复 timeScale 并隐藏面板
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateGameState(GameManager.GameState.Playing);
        }
        else
        {
            if (levelUpPanel != null) levelUpPanel.SetActive(false);
            Time.timeScale = 1f;
        }

        Debug.Log("LevelUpManager 已关闭升级面板。");
    }
}
