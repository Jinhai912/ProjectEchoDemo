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

    #region Unity生命周期
    void Start()
    {
        if (PlayerData.Instance == null)
        {
            Debug.LogError("致命错误: PlayerData 实例不存在！", this);
            return;
        }
        // 游戏开始时，广播一次初始血量，以启动UI的更新
        OnHealthChanged?.Invoke(PlayerData.Instance.currentHealth, PlayerData.Instance.maxHealth);
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

         // 从 PlayerData 读取防御力
        int defense = PlayerData.Instance.defense;
        // 使用我们之前设计的“减法防御”公式
        int finalDamage = Mathf.Max(1, damageAmount - defense);

        PlayerData.Instance.currentHealth -= finalDamage;

        // 广播更新后的血量，以更新UI
        OnHealthChanged?.Invoke(PlayerData.Instance.currentHealth, PlayerData.Instance.maxHealth);

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

        float finalDamage = PlayerData.Instance.baseDamage;
        isCritical = Random.value < PlayerData.Instance.critRate;

        if (isCritical)
        {
            finalDamage *= PlayerData.Instance.critDamage;
        }
        finalDamage *= PlayerData.Instance.totalDamageBonus;
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
            OnHealthChanged?.Invoke(PlayerData.Instance.currentHealth, PlayerData.Instance.maxHealth);
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

        maxHealth = PlayerData.Instance.maxHealth;
        currentHealth = PlayerData.Instance.currentHealth;
        currentExperience = PlayerData.Instance.currentExperience;
        baseDamage = PlayerData.Instance.baseDamage;
        critRate = PlayerData.Instance.critRate;
        critDamage = PlayerData.Instance.critDamage;
        totalDamageBonus = PlayerData.Instance.totalDamageBonus;
        moveSpeed = PlayerData.Instance.moveSpeed;
        fireRate = PlayerData.Instance.fireRate;
    }
    #endregion
}