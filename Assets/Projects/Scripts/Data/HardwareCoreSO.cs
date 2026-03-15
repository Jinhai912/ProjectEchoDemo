using UnityEngine;

/// <summary>
/// 硬件核心炉数据定义 (ScriptableObject)
/// 描述：定义机器人的动力源，决定基础内存容量 (MB) 与生存属性。
/// </summary>
[CreateAssetMenu(fileName = "HW_C_", menuName = "Game/Hardware/Core")]
public class HardwareCoreSO : ScriptableObject
{
    #region 核心标识
    [Header("系统标识")]
    [Tooltip("唯一硬件ID，如 HW_C_001")]
    public string hardwareID;

    [Tooltip("核心炉型号名称")]
    public string hardwareName;
    #endregion

    #region 性能参数
    [Header("性能指标")]
    [Tooltip("该核心提供的基础内存总容量 (单位: MB)")]
    public int memoryCapacity = 1024; 

    [Tooltip("装备该核心时获得的最大生命值加成")]
    public int healthBonus = 0;       

    [Tooltip("机体基础能量恢复速率")]
    public float energyRegen = 10f;   
    #endregion
}