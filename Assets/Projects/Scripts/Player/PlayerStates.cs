// PlayerStates.cs (最终、清晰的版本)
using UnityEngine;

public class PlayerStates : MonoBehaviour
{
    // --- 事件定义 ---
    public static event System.Action<int, int> OnHealthChanged;
    public static event System.Action OnPlayerDied;

    // --- 用于在 Inspector 中监视数据的私有变量 ---
    [Header("--- 玩家当前属性 (运行时监视) ---")]
    [Tooltip("这些数值来自全局的 PlayerData，仅用于显示。")]
    [SerializeField] private int maxHealth;
    [SerializeField] private int currentHealth;
    [SerializeField] private int currentExperience;
    [Space(10)]
    [SerializeField] private float baseDamage;
    [SerializeField] private float critRate;
    [SerializeField] private float critDamage;
    [SerializeField] private float totalDamageBonus;
    [Space(10)]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float defense;

    #region Unity生命周期
    void Start()
    {
        if (PlayerData.Instance == null)
        {
            Debug.LogError("致命错误: PlayerData 实例不存在！", this);
            return;
        }
        // --- 核心修改：读取【计算后】的最终属性 ---
        OnHealthChanged?.Invoke(PlayerData.Instance.currentHealth, PlayerData.Instance.MaxHealth); // 使用 MaxHealth 属性
    }

    void Update()
    {
        // 每一帧都从 PlayerData 拉取最新数据来更新 Inspector 的显示
        SyncStatsForInspector();
    }
    #endregion

    #region 公共接口 (作为外部与 PlayerData 沟通的桥梁)

    /// <summary>
    /// 玩家承受伤害。
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        if (PlayerData.Instance == null || PlayerData.Instance.currentHealth <= 0) return;
        
        // --- 核心修改：读取【计算后】的最终属性 ---
        int finalDamage = Mathf.Max(1, damageAmount - PlayerData.Instance.Defense); // 使用 Defense 属性

        PlayerData.Instance.currentHealth -= finalDamage;
        
        // --- 核心修改：读取【计算后】的最终属性 ---
        OnHealthChanged?.Invoke(PlayerData.Instance.currentHealth, PlayerData.Instance.MaxHealth); // 使用 MaxHealth 属性

        if (PlayerData.Instance.currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 计算将要造成的伤害。
    /// </summary>
    public float CalculateFinalDamage(out bool isCritical)
    {
        isCritical = false;
        if (PlayerData.Instance == null) return 0;

        // --- 核心修改：读取【计算后】的最终属性 ---
        // 注意：这些名字的改变取决于你在 PlayerData.cs 中如何命名你的计算属性
        float finalDamage = PlayerData.Instance.FinalAttack; // 假设最终攻击力属性叫这个
        isCritical = Random.value < PlayerData.Instance.FinalCritRate; // 假设最终暴击率属性叫这个

        if (isCritical)
        {
            finalDamage *= PlayerData.Instance.FinalCritDamage; // 假设最终暴伤属性叫这个
        }
        finalDamage *= PlayerData.Instance.FinalDamageBonus; // 假设最终伤害加成属性叫这个
        return finalDamage;
    }

    /// <summary>
    /// 获得经验。
    /// </summary>
    public void AddExperience(int amount)
    {
        if (PlayerData.Instance == null) return;
        PlayerData.Instance.AddExperience(amount);
        PlayerFeedbackManager.OnFeedbackRequested?.Invoke("+" + amount + " XP", false, transform.position);
    }

    /// <summary>
    /// 应用一个能力。
    /// </summary>
    public void ApplyAbility(AbilityData ability)
    {
        if (PlayerData.Instance == null) return;
        PlayerData.Instance.ApplyAbility(ability);
        
        // 应用能力后，特别是加血后，需要手动广播一次血量变化
        if(ability.type == AbilityData.AbilityType.IncreaseMaxHealth)
        {
            OnHealthChanged?.Invoke(PlayerData.Instance.currentHealth, PlayerData.Instance.MaxHealth);
        }
    }

    #endregion

    #region 私有方法
    /// <summary>
    /// 私有的死亡处理方法
    /// </summary>
    private void Die()
    {
        OnPlayerDied?.Invoke();
    }

    /// <summary>
    /// 从 PlayerData 同步数据到 Inspector 以便监视
    /// </summary>
    private void SyncStatsForInspector()
    {
        if (PlayerData.Instance == null) return;

        // --- 核心修改：所有读取操作，都从旧的变量名，改为新的【计算后】的属性名 ---
        maxHealth = PlayerData.Instance.MaxHealth;
        currentHealth = PlayerData.Instance.currentHealth; // currentHealth 是直接存储的，不用改
        currentExperience = PlayerData.Instance.currentExperience; // 同上
        baseDamage = PlayerData.Instance.FinalAttack; // 显示最终攻击力
        critRate = PlayerData.Instance.FinalCritRate; // 显示最终暴击率
        critDamage = PlayerData.Instance.FinalCritDamage; // 显示最终暴伤
        totalDamageBonus = PlayerData.Instance.FinalDamageBonus; // 显示最终伤害加成
        moveSpeed = PlayerData.Instance.MoveSpeed;
        fireRate = PlayerData.Instance.FireRate;
        defense = PlayerData.Instance.Defense;
    }
    #endregion
}