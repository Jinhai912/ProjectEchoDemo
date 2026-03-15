using UnityEngine;

/// <summary>
/// 经验球实体
/// </summary>
public class ExperienceGem : Pickup
{
    public int experienceAmount = 10;

    protected override void OnPickedUp(GameObject picker)
    {
        if (picker.TryGetComponent<PlayerStates>(out var player))
        {
            player.AddExperience(experienceAmount);
        }
        base.OnPickedUp(picker); 
    }
}