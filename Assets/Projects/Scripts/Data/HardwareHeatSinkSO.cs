using UnityEngine;

/// <summary>
/// 硬件散热器数据定义。
/// 描述：决定热量上限、散热速度、冷却延迟和过热恢复时间。
/// </summary>
[CreateAssetMenu(fileName = "HW_H_", menuName = "Game/Hardware/HeatSink")]
public class HardwareHeatSinkSO : HardwareData
{
    #region 系统标识
    [Header("系统标识")]
    [Tooltip("唯一硬件ID，如 HW_H_001")]
    public string hardwareID;

    [Tooltip("散热器型号名称")]
    public string hardwareName;

    [TextArea(2, 4)]
    public string description;

    public Sprite icon;
    #endregion

    #region 散热性能
    [Header("散热性能")]
    [Tooltip("热量上限")]
    public float heatCapacity = 100f;

    [Tooltip("每秒散热量")]
    public float coolingRate = 25f;

    [Tooltip("停火后多久开始散热")]
    public float coolingDelay = 0.5f;

    [Tooltip("过热后强制停火的时间")]
    public float overheatRecoveryTime = 2f;
    #endregion

    public override HardwareSlot Slot => HardwareSlot.HeatSink;
    public override string DisplayName => hardwareName;
}
