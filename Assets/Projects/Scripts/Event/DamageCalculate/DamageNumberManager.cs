using UnityEngine;

public class DamageNumberManager : MonoBehaviour
{
    // --- 引用 ---
    public GameObject damageNumberPrefab; // 伤害数字的预制体
    public Canvas uiCanvas;               // UI画布的引用

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    // 在脚本启用时订阅事件
    void OnEnable()
    {
        EnemyState.OnDamageTaken += HandleDamageTaken;
    }

    // 在脚本禁用时取消订阅，防止内存泄漏
    void OnDisable()
    {
        EnemyState.OnDamageTaken -= HandleDamageTaken;
    }

    // 事件处理函数
    private void HandleDamageTaken(float damage, bool isCritical, Vector3 worldPosition)
    {
        if (damageNumberPrefab == null || uiCanvas == null)
        {
            Debug.LogError("DamageNumberManager 缺少预制体或画布的引用!");
            return;
        }

        // 1. 在画布下创建伤害数字的实例
        GameObject numberInstance = Instantiate(damageNumberPrefab, uiCanvas.transform);

        // 2. 将伤害发生的世界坐标转换为UI的屏幕坐标
        // 稍微向上偏移一点，让数字出现在敌人头顶
        Vector3 spawnWorldPosition = worldPosition + Vector3.up * 1.5f;
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(spawnWorldPosition);
        
        // 随机给一个小的水平偏移，防止数字完全重叠
        screenPosition.x += Random.Range(-25f, 25f);
        screenPosition.y += Random.Range(-15f, 15f);

        // 设置UI元素的位置
        numberInstance.transform.position = screenPosition;

        // 3. 调用动画脚本的Setup方法，传递伤害信息
        DamageNumberAnimator animator = numberInstance.GetComponent<DamageNumberAnimator>();
        if (animator != null)
        {
            animator.Setup(damage, isCritical);
        }
    }
}