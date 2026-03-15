using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 伤害飘字管理器
/// 描述：监听敌人受击事件，并负责在对应屏幕坐标生成伤害数字。
/// </summary>
public class DamageNumberManager : MonoBehaviour
{
    [Header("资源配置")]
    public GameObject damageNumberPrefab;

    private Canvas _uiCanvas;
    private Camera _mainCamera;

    #region 生命周期
    void Awake()
    {
        FindRequiredComponents();
        EnemyState.OnDamageTaken += HandleDamageTaken;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        EnemyState.OnDamageTaken -= HandleDamageTaken;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    #endregion

    #region 核心逻辑
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => FindRequiredComponents();

    private void FindRequiredComponents()
    {
        _mainCamera = Camera.main;
        _uiCanvas = FindObjectOfType<Canvas>();
    }

    private void HandleDamageTaken(float damage, bool isCritical, Vector3 worldPosition)
    {
        if (damageNumberPrefab == null || _uiCanvas == null || _mainCamera == null) return;

        // 1. 实例化与坐标转换
        GameObject numberInstance = Instantiate(damageNumberPrefab, _uiCanvas.transform);
        Vector3 spawnWorldPos = worldPosition + Vector3.up * 1.5f;
        Vector2 screenPos = _mainCamera.WorldToScreenPoint(spawnWorldPos);

        // 2. 增加随机扰动，防止文字重叠
        screenPos.x += Random.Range(-25f, 25f);
        screenPos.y += Random.Range(-15f, 15f);
        numberInstance.transform.position = screenPos;

        // 3. 启动动画
        if (numberInstance.TryGetComponent<DamageNumberAnimator>(out var animator))
        {
            animator.Setup(damage, isCritical);
        }
    }
    #endregion
}