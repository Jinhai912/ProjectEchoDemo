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
            if (ability == null) continue;

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
    /// 保证不会因为重复抽取而陷入死循环，并在能力总数不足时提前结束。
    /// </summary>
    public List<AbilityData> GetRandomAbilities(int count)
    {
        List<AbilityData> result = new List<AbilityData>();
        if (count <= 0) return result;

        List<AbilityData>[] pools = { _whitePool, _bluePool, _purplePool, _redPool };

        while (result.Count < count)
        {
            // 设定出货率：白(60%)、蓝(25%)、紫(10%)、红(5%)
            int roll = Random.Range(0, 100);
            List<AbilityData> targetPool = roll switch {
                < 60 => _whitePool,
                < 85 => _bluePool,
                < 95 => _purplePool,
                _ => _redPool
            };

            AbilityData selected = PickUnused(targetPool, result);

            // 目标品质已经被抽干，就按品质顺序回退到其他还有存货的池子
            if (selected == null)
            {
                foreach (var pool in pools)
                {
                    selected = PickUnused(pool, result);
                    if (selected != null) break;
                }
            }

            if (selected == null) break; // 所有池子都抽空，提前结束

            result.Add(selected);
        }

        return result;
    }

    /// <summary>
    /// 从指定池中随机取一个尚未被选中的能力；没有可用项时返回 null。
    /// </summary>
    private static AbilityData PickUnused(List<AbilityData> pool, List<AbilityData> used)
    {
        int availableCount = 0;
        foreach (var ability in pool)
        {
            if (ability != null && !used.Contains(ability)) availableCount++;
        }

        if (availableCount == 0) return null;

        int pickIndex = Random.Range(0, availableCount);
        foreach (var ability in pool)
        {
            if (ability == null || used.Contains(ability)) continue;
            if (pickIndex == 0) return ability;
            pickIndex--;
        }

        return null;
    }
}
