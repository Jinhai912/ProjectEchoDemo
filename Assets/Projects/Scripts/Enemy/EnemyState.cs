using System;
using UnityEngine;

/// <summary>
/// 敌人状态与生命周期管理器
/// 描述：负责血量维护、受伤判定、死亡事件分发及奖励掉落。
/// </summary>
public class EnemyState : MonoBehaviour
{
    #region 事件定义
    public static event Action<float, bool, Vector3> OnDamageTaken;
    public event Action OnDeath;
    #endregion

    #region 配置项
    [Header("生命值配置")]
    public float maxHealth = 10f;
    [SerializeField] private float _currentHealth;

    [Header("掉落配置")]
    public GameObject expGemPrefab;
    public int currencyMin = 10;
    public int currencyMax = 20;
    #endregion

    private RoomController _roomController;
    private static Material _currencyMaterial;

    #region 生命周期
    void Start()
    {
        _currentHealth = maxHealth;
        _roomController = RoomController.Instance;
        if (_roomController == null) _roomController = FindObjectOfType<RoomController>();
        if (_roomController == null) Debug.LogError("[系统] 场景中找不到 RoomController!");
    }
    #endregion

    #region 公共接口
    public void TakeDamage(float damage, bool isCritical)
    {
        if (_currentHealth <= 0) return;

        _currentHealth -= damage;
        OnDamageTaken?.Invoke(damage, isCritical, transform.position);

        if (_currentHealth <= 0) Die();
    }
    #endregion

    #region 私有方法
    private void Die()
    {
        OnDeath?.Invoke();
        if (_roomController != null) _roomController.OnEnemyDied();

        if (expGemPrefab)
        {
            if (ObjectPooler.Instance != null)
            {
                ObjectPooler.Instance.SpawnFromPool(expGemPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                Instantiate(expGemPrefab, transform.position, Quaternion.identity);
            }
        }

        SpawnCurrencyDrop();

        Destroy(gameObject);
    }

    private void SpawnCurrencyDrop()
    {
        int amount = UnityEngine.Random.Range(currencyMin, currencyMax + 1);
        if (amount <= 0) return;

        // 占位表现：一个小光球；后续可以替换成正式的碎片模型/特效
        GameObject gem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gem.name = "CurrencyGem";
        gem.transform.position = transform.position + Vector3.up * 0.5f;
        gem.transform.localScale = Vector3.one * 0.3f;

        Collider gemCollider = gem.GetComponent<Collider>();
        if (gemCollider != null) gemCollider.isTrigger = true;

        Renderer gemRenderer = gem.GetComponent<Renderer>();
        if (gemRenderer != null)
        {
            if (_currencyMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");

                if (shader != null)
                {
                    _currencyMaterial = new Material(shader);
                    _currencyMaterial.color = new Color(0.2f, 0.8f, 1f);
                }
            }

            if (_currencyMaterial != null)
            {
                gemRenderer.sharedMaterial = _currencyMaterial;
            }
        }

        CurrencyGem currencyGem = gem.AddComponent<CurrencyGem>();
        currencyGem.currencyAmount = amount;
        gem.AddComponent<TemporaryEncounterObject>();
    }
    #endregion
}
