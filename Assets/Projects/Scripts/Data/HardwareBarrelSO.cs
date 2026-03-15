using UnityEngine;

/// <summary>
/// 硬件枪管单元数据定义 (ScriptableObject)
/// 描述：决定机器人的攻击形态、基础火力数值以及热量产生系数。
/// </summary>
[CreateAssetMenu(fileName = "HW_B_", menuName = "Game/Hardware/Barrel")]
public class HardwareBarrelSO : ScriptableObject
{
    #region 核心标识
    [Header("系统标识")]
    [Tooltip("唯一硬件ID，如 HW_B_001")]
    public string hardwareID;

    [Tooltip("枪管型号名称")]
    public string hardwareName;
    #endregion

    #region 战斗参数
    [Header("输出配置")]
    [Tooltip("该枪管发射的基础子弹预制体")]
    public GameObject bulletPrefab;   

    [Tooltip("每一枚弹丸的基础伤害值")]
    public float baseDamage = 10f;    

    [Tooltip("基础射速 (每秒发射次数)")]
    public float baseFireRate = 2f;   
    #endregion

    #region 物理反馈
    [Header("热能特性")]
    [Tooltip("每发射一枚弹丸产生的基础热量值 (用于过载构筑)")]
    public float heatPerShot = 5f;    
    #endregion
}