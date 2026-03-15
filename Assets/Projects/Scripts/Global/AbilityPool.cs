using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 能力与协议分级资产池
/// 描述：负责在Awake阶段自动对AbilityData进行品质分拣，并实现基于权重的随机抽取算法。
/// </summary>
public class AbilityPool : MonoBehaviour
{
    [Header("全能力数据仓库")]
    public List<AbilityData> allAbilities;

    #region 私有池子
    private List<AbilityData> _whitePool = new List<AbilityData>();
    private List<AbilityData> _bluePool = new List<AbilityData>();
    private List<AbilityData> _purplePool = new List<AbilityData>();
    private List<AbilityData> _redPool = new List<AbilityData>();
    #endregion

    private void Awake()
    {
        InitializeTiers();
    }

    /// <summary>
    /// 根据 Tier 标签自动分拣能力
    /// </summary>
    private void InitializeTiers()
    {
        _whitePool.Clear(); _bluePool.Clear(); _purplePool.Clear(); _redPool.Clear();
        foreach (var ability in allAbilities)
        {
            switch (ability.tier)
            {
                case AbilityData.AbilityTier.L1_Micro: _whitePool.Add(ability); break;
                case AbilityData.AbilityTier.L2_Driver: _bluePool.Add(ability); break;
                case AbilityData.AbilityTier.L3_Logic: _purplePool.Add(ability); break;
                case AbilityData.AbilityTier.L4_Core: _redPool.Add(ability); break;
            }
        }
    }

    /// <summary>
    /// 执行基于概率的加权随机抽取
    /// </summary>
    public List<AbilityData> GetRandomAbilities(int count)
    {
        List<AbilityData> result = new List<AbilityData>();
        
        for (int i = 0; i < count; i++)
        {
            // 设定出货率：白(60%)、蓝(25%)、紫(10%)、红(5%)
            int roll = Random.Range(0, 100);
            List<AbilityData> targetPool = roll switch {
                < 60 => _whitePool,
                < 85 => _bluePool,
                < 95 => _purplePool,
                _ => _redPool
            };

            // 池子回退逻辑（防止某种品质能力被抽干）
            if (targetPool.Count == 0) targetPool = _whitePool;
            
            if (targetPool.Count > 0)
            {
                AbilityData selected = targetPool[Random.Range(0, targetPool.Count)];
                if (!result.Contains(selected)) result.Add(selected);
                else i--; // 简单防重抽
            }
        }
        return result;
    }
}