using UnityEngine;

[CreateAssetMenu(fileName = "NewMediaEffect", menuName = "Abilities/Effects/Media Injector")]
public class MediaEffectSO : AbilityEffectSO
{
    public PhysicalMediaType mediaToInject;

    public override void OnEquip(PlayerData data)
    {
        // 简单处理：直接覆盖当前子弹属性
        // 以后如果你想做“双元素子弹”，可以在这里改成列表或位运算
        data.currentBulletMedia = mediaToInject;
        Debug.Log($"<color=magenta>[注入] 子弹现在携带 {mediaToInject} 属性</color>");
    }

    public override void OnRemove(PlayerData data)
    {
        if (data.currentBulletMedia == mediaToInject)
        {
            data.currentBulletMedia = PhysicalMediaType.None;
        }
    }
}