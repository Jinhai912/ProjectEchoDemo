using UnityEngine;

[CreateAssetMenu(fileName = "NewShootEffect", menuName = "Abilities/Effects/Shoot Modifier")]
public class ShootModifierEffectSO : AbilityEffectSO
{
    public ShootModType modifierType;
    public int value; // 增加的数量 (比如 +1 发，+1 次穿透)

    public enum ShootModType
    {
        AddProjectileCount, // 增加子弹数
        AddPierceCount      // 增加穿透数
    }

    public override void OnEquip(PlayerData data)
    {
        switch (modifierType)
        {
            case ShootModType.AddProjectileCount:
                data.projectileCount += value;
                Debug.Log($"机制生效: 子弹数 +{value}, 当前: {data.projectileCount}");
                break;

            case ShootModType.AddPierceCount:
                data.piercingCount += value;
                Debug.Log($"机制生效: 穿透数 +{value}, 当前: {data.piercingCount}");
                break;
        }
    }
}