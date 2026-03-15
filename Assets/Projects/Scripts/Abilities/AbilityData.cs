using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 协议/能力数据定义 (ScriptableObject)
/// 描述：定义局内获得的软件协议（Software）或固件升级（Firmware）的核心属性。
/// </summary>
[CreateAssetMenu(fileName = "SW_P_", menuName = "Game/Abilities/Ability Data")]
public class AbilityData : ScriptableObject
{
    #region 核心标识
    [Header("系统标识")]
    [Tooltip("唯一索引ID，如 SW_P_101")]
    public string id;           
    
    [Tooltip("玩家看到的名称")]
    public string abilityName;  
    #endregion

    #region 构筑参数
    [Header("构筑规格")]
    [Tooltip("逻辑层级：L1微指令, L2驱动, L3逻辑, L4内核")]
    public AbilityTier tier;    
    
    [Tooltip("是否为内存绑定协议。勾选则占用MB空间并可卸载，不勾选则为永久生效的固件。")]
    public bool isMemoryBound; 

    [Tooltip("内存占用量 (单位: MB)。仅在勾选 isMemoryBound 时有效。")]
    public int memoryCost;      
    #endregion

    #region 显示信息
    [Header("本地化描述")]
    [TextArea(3, 5)] 
    public string description;
    
    [Tooltip("卡牌或UI显示的图标")]
    public Sprite icon;
    #endregion

    #region 逻辑内核
    [Header("效果配置")]
    [Tooltip("该协议包含的逻辑效果零件列表")]
    public List<AbilityEffectSO> effects = new List<AbilityEffectSO>();
    #endregion

    #region 枚举定义
    /// <summary>
    /// 协议的逻辑层级定义
    /// </summary>
    public enum AbilityTier
    {
        /// <summary> 白色：微指令 </summary>
        L1_Micro = 0,   
        /// <summary> 蓝色：驱动程序 </summary>
        L2_Driver = 1,  
        /// <summary> 紫色：功能逻辑库 </summary>
        L3_Logic = 2,   
        /// <summary> 红色：全系统内核 </summary>
        L4_Core = 3     
    }
    #endregion
}