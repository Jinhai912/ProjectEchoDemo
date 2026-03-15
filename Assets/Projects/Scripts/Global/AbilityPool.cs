using System.Collections.Generic;
using UnityEngine;

public class AbilityPool : MonoBehaviour
{
    [Header("全能力总库 (把所有 .asset 丢进来)")]
    public List<AbilityData> allAbilities;

    // 自动分拣的子池子
    private List<AbilityData> whitePool = new List<AbilityData>();
    private List<AbilityData> bluePool = new List<AbilityData>();
    private List<AbilityData> purplePool = new List<AbilityData>();
    private List<AbilityData> redPool = new List<AbilityData>();

    void Awake()
    {
        SortAbilities();
    }

    // 自动按品质分类
    private void SortAbilities()
    {
        whitePool.Clear(); bluePool.Clear(); purplePool.Clear(); redPool.Clear();
        foreach (var ability in allAbilities)
        {
            switch (ability.tier)
            {
                case AbilityData.AbilityTier.L1_Micro: whitePool.Add(ability); break;
                case AbilityData.AbilityTier.L2_Driver: bluePool.Add(ability); break;
                case AbilityData.AbilityTier.L3_Logic: purplePool.Add(ability); break;
                case AbilityData.AbilityTier.L4_Core: redPool.Add(ability); break;
            }
        }
    }

    public List<AbilityData> GetRandomAbilities(int count)
    {
        List<AbilityData> chosenAbilities = new List<AbilityData>();
        
        for (int i = 0; i < count; i++)
        {
            // --- 决战之夜：Demo 权重逻辑 ---
            // 白(60%)、蓝(25%)、紫(10%)、红(5%)
            int roll = Random.Range(0, 100);
            List<AbilityData> selectedPool;

            if (roll < 60) selectedPool = whitePool;
            else if (roll < 85) selectedPool = bluePool;
            else if (roll < 95) selectedPool = purplePool;
            else selectedPool = redPool;

            // 兜底：如果选中的池子是空的，回退到白池子
            if (selectedPool.Count == 0) selectedPool = whitePool;
            
            if (selectedPool.Count > 0)
            {
                AbilityData randomAbility = selectedPool[Random.Range(0, selectedPool.Count)];
                // 防重复：如果已经选了这张，就再抽一次（简单处理）
                if (!chosenAbilities.Contains(randomAbility))
                    chosenAbilities.Add(randomAbility);
                else
                    i--; // 重新抽
            }
        }
        return chosenAbilities;
    }
}