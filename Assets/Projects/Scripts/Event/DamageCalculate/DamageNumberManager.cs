// DamageNumberManager.cs (修改后)
using UnityEngine;
using UnityEngine.SceneManagement; // 引入场景管理命名空间

public class DamageNumberManager : MonoBehaviour
{
    // --- 引用 ---
    public GameObject damageNumberPrefab;
    private Canvas uiCanvas; // 变为 private
    private Camera mainCamera;

    // 不再需要 Start，使用 Awake
    void Awake()
    {
        FindRequiredComponents();
    }
    
    void OnEnable()
    {
        // 这里的 EnemyState.OnDamageTaken 需要确保 EnemyState 脚本也存在
        // 或者将事件定义为 public static，就像 PlayerFeedbackManager 那样
        // 假设 EnemyState.cs 中有 public static event System.Action<...> OnDamageTaken;
        EnemyState.OnDamageTaken += HandleDamageTaken;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        EnemyState.OnDamageTaken -= HandleDamageTaken;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindRequiredComponents();
    }

    private void FindRequiredComponents()
    {
        mainCamera = Camera.main;
        uiCanvas = FindObjectOfType<Canvas>();
        if (mainCamera == null) Debug.LogError("DamageNumberManager: 场景中找不到 MainCamera!");
        if (uiCanvas == null) Debug.LogError("DamageNumberManager: 场景中找不到 Canvas!");
    }

    // 事件处理函数
    private void HandleDamageTaken(float damage, bool isCritical, Vector3 worldPosition)
    {
        if (damageNumberPrefab == null || uiCanvas == null || mainCamera == null)
        {
            // Debug.LogError("DamageNumberManager 缺少预制体或画布或相机的引用!");
            return;
        }

        // ... 后续的 Instantiate 和 Setup 逻辑完全不变 ...
        GameObject numberInstance = Instantiate(damageNumberPrefab, uiCanvas.transform);
        Vector3 spawnWorldPosition = worldPosition + Vector3.up * 1.5f;
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(spawnWorldPosition);
        screenPosition.x += Random.Range(-25f, 25f);
        screenPosition.y += Random.Range(-15f, 15f);
        numberInstance.transform.position = screenPosition;
        DamageNumberAnimator animator = numberInstance.GetComponent<DamageNumberAnimator>();
        if (animator != null)
        {
            animator.Setup(damage, isCritical);
        }
    }
}