// PlayerFeedbackManager.cs (修改后)
using UnityEngine;
using UnityEngine.SceneManagement; // 引入场景管理命名空间

public class PlayerFeedbackManager : MonoBehaviour
{
    // --- 引用 ---
    public GameObject feedbackTextPrefab;
    // uiCanvas 现在是私有的，因为它会动态获取
    private Canvas uiCanvas; 
    private Camera mainCamera;

    void Awake()
    {
        // Awake 在对象第一次被创建时调用，在这里进行初始设置
        FindRequiredComponents();
    }

    void OnEnable()
    {
        OnFeedbackRequested += HandleFeedbackRequest;
        // 监听场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        OnFeedbackRequested -= HandleFeedbackRequest;
        // 取消监听
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    // 当新场景加载完成时调用
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 在新场景中重新寻找组件
        FindRequiredComponents();
    }

    // 一个专门用来寻找所需组件的方法
    private void FindRequiredComponents()
    {
        mainCamera = Camera.main; // Camera.main 会自动寻找带 "MainCamera" 标签的相机
        uiCanvas = FindObjectOfType<Canvas>(); // 寻找当前场景中的第一个 Canvas

        // 添加健壮性检查
        if (mainCamera == null) Debug.LogError("PlayerFeedbackManager: 场景中找不到 MainCamera!");
        if (uiCanvas == null) Debug.LogError("PlayerFeedbackManager: 场景中找不到 Canvas!");
    }

    // 事件处理函数
    private void HandleFeedbackRequest(string message, bool isSpecial, Vector3 worldPosition)
    {
        // 在使用前再次检查，确保万无一失
        if (feedbackTextPrefab == null || uiCanvas == null || mainCamera == null) return;

        // ... 后续的 Instantiate 和 Setup 逻辑完全不变 ...
        GameObject textInstance = Instantiate(feedbackTextPrefab, uiCanvas.transform);
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        screenPosition.x += Random.Range(-20f, 20f);
        screenPosition.y += Random.Range(10f, 30f);
        textInstance.transform.position = screenPosition;
        PickupFeedbackAnimator animator = textInstance.GetComponent<PickupFeedbackAnimator>();
        if (animator != null)
        {
            animator.Setup(message, isSpecial);
        }
    }
    
    // 静态事件定义可以保留，也可以移到更全局的地方，比如一个 EventManager
    public static System.Action<string, bool, Vector3> OnFeedbackRequested;
}