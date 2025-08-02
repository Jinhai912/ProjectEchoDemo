using UnityEngine;

// [CreateAssetMenu] 特性是这里的核心魔法。
// 它会在 Unity 的 "Assets/Create" 菜单中添加一个新的选项，
// 让我们能够像创建材质球或动画控制器一样，直接创建这个脚本的实例文件。
[CreateAssetMenu(fileName = "New Ability", menuName = "Game/Ability Data")]
public class AbilityData : ScriptableObject
{
    [Header("基本信息")]
    public string abilityName;
    [TextArea(3, 5)] // 让描述框在 Inspector 里可以输入多行
    public string description;
    public Sprite icon;

    [Header("能力效果")]
    // 我们用一个枚举来定义这个能力具体是做什么的
    public AbilityType type;
    public float value; // 效果的数值 (例如: 0.1 代表 10%, 5 代表 +5 点)

    // 定义所有可能的能力类型
    public enum AbilityType
    {
        // 基础数值提升
        IncreaseMaxHealth,
        IncreaseDefense,
        IncreaseAttack,
        IncreaseMoveSpeed,
        IncreaseAttackSpeed,
        IncreaseKnockback,
        IncreasePickupRadius,
        
        // 衍生/乘区属性提升
        IncreaseDamageBonus,
        IncreaseCritRate,
        IncreaseCritDamage,
        
        // 质变型能力 (未来扩展)
        AddProjectile,
        EnablePierce,
        EnableRicochet
    }
}