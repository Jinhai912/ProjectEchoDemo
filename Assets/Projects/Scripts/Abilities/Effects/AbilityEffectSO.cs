using UnityEngine;

/// <summary>
/// 能力效果抽象基类
/// 描述：所有具体逻辑零件（数值加成、机制注入、反应联动）的父类。
/// </summary>
public abstract class AbilityEffectSO : ScriptableObject
{
    [TextArea(3, 5)]
    [Tooltip("该效果的逻辑描述，可用于UI显示")]
    public string effectDescription;

    /// <summary>
    /// 当协议被安装或固件被应用时触发
    /// </summary>
    /// <param name="playerData">玩家全局数据引用</param>
    public abstract void OnEquip(PlayerData playerData);

    /// <summary>
    /// 当协议被卸载或效果失效时触发（撤销逻辑）
    /// </summary>
    /// <param name="playerData">玩家全局数据引用</param>
    public virtual void OnRemove(PlayerData playerData) { }
}