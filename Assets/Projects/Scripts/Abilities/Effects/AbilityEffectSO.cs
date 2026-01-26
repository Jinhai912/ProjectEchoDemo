using UnityEngine;

public abstract class AbilityEffectSO : ScriptableObject
{
    // 每个效果都可以有自己的描述，用于 UI 拼接
    [TextArea] public string description;

    // 核心方法：当这个效果被应用到玩家身上时发生什么
    // 我们把 PlayerData 传进去，让 Effect 自己决定改什么数据
    public abstract void OnEquip(PlayerData playerData);

    // 可选：当效果被移除时（备用，比如临时 Buff）
    public virtual void OnRemove(PlayerData playerData) { }
}