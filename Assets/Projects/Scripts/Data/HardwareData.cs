using UnityEngine;

/// <summary>
/// 硬件槽位类型。
/// </summary>
public enum HardwareSlot
{
    Core,
    Barrel,
    HeatSink
}

/// <summary>
/// 所有可挂载硬件的公共基类。
/// 描述：让商店、升级和事件可以用同一套奖励/UI 逻辑处理核心炉、枪管和散热器。
/// </summary>
public abstract class HardwareData : ScriptableObject
{
    public abstract HardwareSlot Slot { get; }
    public abstract string DisplayName { get; }
}
