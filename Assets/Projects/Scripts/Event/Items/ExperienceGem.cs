using UnityEngine;

// ExperienceGem 继承自 Pickup，拥有其所有行为（如飞向玩家）
public class ExperienceGem : Pickup
{
    public int experienceAmount = 10; // 每个经验球提供的经验值

    // 重写基类的 OnPickedUp 方法，添加自己独特的效果
    protected override void OnPickedUp(GameObject picker)
    {
        // 尝试从拾取者（玩家）身上获取玩家经验脚本
        PlayerStates playerExp = picker.GetComponent<PlayerStates>();
        
        if (playerExp != null)
        {
            // 调用增加经验的方法
            playerExp.AddExperience(experienceAmount);
        }

        // 在执行完自己的逻辑后，调用基类的方法来完成销毁等共同行为
        base.OnPickedUp(picker); 
    }
}