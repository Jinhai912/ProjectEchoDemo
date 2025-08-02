using System.Collections.Generic;
using UnityEngine;

public class AbilityPool : MonoBehaviour
{
    // 在 Inspector 中，我们将把所有创建的能力 .asset 文件都拖到这个列表里
    [Header("能力数据库")]
    public List<AbilityData> allAbilities;

    /// <summary>
    /// [Beta]从能力池中随机、可重复地抽取指定数量的能力。
    /// </summary>
    /// <param name="count">要抽取的数量</param>
    /// <returns>一个包含随机能力的新列表</returns>
    public List<AbilityData> GetRandomAbilities(int count)
    {
        // 创建一个临时的列表，复制所有能力，避免直接修改原始列表
        List<AbilityData> tempPool = new List<AbilityData>(allAbilities);
        List<AbilityData> chosenAbilities = new List<AbilityData>();

        // 确保抽取的数量不会超过池中能力的总数
        int drawCount = Mathf.Min(count, tempPool.Count);

        for (int i = 0; i < drawCount; i++)
        {
            // 随机选择一个索引
            int randomIndex = Random.Range(0, tempPool.Count);
            
            // 将选中的能力添加到结果列表中
            chosenAbilities.Add(tempPool[randomIndex]);
            
            // 从临时池中移除已选中的能力，确保下次不会再抽到它
            tempPool.RemoveAt(randomIndex);
        }

        return chosenAbilities;
    }
}