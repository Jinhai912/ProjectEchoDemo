// PlayerFeedbackManager.cs
using UnityEngine;

public class PlayerFeedbackManager : MonoBehaviour
{
    // --- 静态事件 ---
    // 定义一个事件，让其他脚本可以请求显示反馈
    // 参数: 消息内容, 是否特殊, 发生的世界坐标
    public static System.Action<string, bool, Vector3> OnFeedbackRequested;

    // --- 引用 ---
    public GameObject feedbackTextPrefab; // 拾取反馈的预制体
    public Canvas uiCanvas;               // UI画布的引用

    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
        // 如果引用没设置，尝试自动寻找
        if (uiCanvas == null) { uiCanvas = FindObjectOfType<Canvas>(); }
    }

    // 订阅事件
    void OnEnable() { OnFeedbackRequested += HandleFeedbackRequest; }
    // 取消订阅
    void OnDisable() { OnFeedbackRequested -= HandleFeedbackRequest; }

    // 事件处理函数
    private void HandleFeedbackRequest(string message, bool isSpecial, Vector3 worldPosition)
    {
        if (feedbackTextPrefab == null || uiCanvas == null) return;

        // 1. 在画布下创建实例
        GameObject textInstance = Instantiate(feedbackTextPrefab, uiCanvas.transform);

        // 2. 坐标转换 + 随机偏移
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        screenPosition.x += Random.Range(-20f, 20f);
        screenPosition.y += Random.Range(10f, 30f); // 向上偏移多一点
        textInstance.transform.position = screenPosition;

        // 3. 调用动画脚本的Setup
        PickupFeedbackAnimator animator = textInstance.GetComponent<PickupFeedbackAnimator>();
        if (animator != null)
        {
            animator.Setup(message, isSpecial);
        }
    }
}