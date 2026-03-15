/// <summary>
/// 物理介质类型定义
/// 描述：定义了游戏中所有可附着、可交互的物理属性标签。
/// </summary>
public enum PhysicalMediaType
{
    /// <summary> 无物理属性 </summary>
    None,
    
    /// <summary> 电荷：高导电性，支持传导反应 </summary>
    Electric,
    
    /// <summary> 热能：高能量副产物，支持过载与爆燃反应 </summary>
    Heat,
    
    /// <summary> 液体(水)：良好的导电介质，具备降温特性 </summary>
    Liquid_Water,
    
    /// <summary> 液体(油)：不导电，极易燃介质 </summary>
    Liquid_Oil
}