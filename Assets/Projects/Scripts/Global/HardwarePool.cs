using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 硬件资产池。
/// 描述：按槽位权重随机返回核心炉、枪管或散热器，供商店/升级/事件使用。
/// </summary>
public class HardwarePool : MonoBehaviour
{
    [Header("核心炉")]
    public List<HardwareCoreSO> cores = new List<HardwareCoreSO>();

    [Header("枪管")]
    public List<HardwareBarrelSO> barrels = new List<HardwareBarrelSO>();

    [Header("散热器")]
    public List<HardwareHeatSinkSO> heatSinks = new List<HardwareHeatSinkSO>();

    public HardwareData GetRandomHardware()
    {
        float roll = Random.value;

        if (roll < 0.25f && cores != null && cores.Count > 0)
            return cores[Random.Range(0, cores.Count)];

        if (roll < 0.65f && barrels != null && barrels.Count > 0)
            return barrels[Random.Range(0, barrels.Count)];

        if (heatSinks != null && heatSinks.Count > 0)
            return heatSinks[Random.Range(0, heatSinks.Count)];

        // 槽位池为空时回退到任意非空池
        if (cores != null && cores.Count > 0) return cores[Random.Range(0, cores.Count)];
        if (barrels != null && barrels.Count > 0) return barrels[Random.Range(0, barrels.Count)];
        return null;
    }
}
