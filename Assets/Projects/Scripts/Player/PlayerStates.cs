using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStates : MonoBehaviour
{
    // --- 核心属性 ---
    [Header("基础属性")]
    public float baseDamage = 10f;
    public float critRate = 0.1f; // 10% 暴击率
    public float critDamage = 1.5f; // 150% 暴击伤害

    [Header("伤害加成 (乘区)")]
    public float totalDamageBonus = 1.0f; // 1.0f 表示没有加成

    // --- 计算伤害的公共方法 ---
    // isCritical: out 参数，用于告诉外部这次攻击是否暴击
    public float CalculateFinalDamage(out bool isCritical)
    {
        float finalDamage = baseDamage;
        isCritical = false;

        // 1. 判定暴击
        if (Random.value < critRate) // Random.value 返回 0.0 到 1.0 之间的一个随机数
        {
            isCritical = true;
            finalDamage *= critDamage;
        }

        // 2. 应用伤害加成
        finalDamage *= totalDamageBonus;

        // 3. (未来) 应用其他特殊道具加成...

        return finalDamage;
    }
}
