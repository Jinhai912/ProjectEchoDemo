using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 玩家反馈信息管理器
/// 描述：负责处理除了伤害数值以外的所有文字反馈（如XP获得、材料获取、物理反应提示）。
/// </summary>
public class PlayerFeedbackManager : MonoBehaviour
{
    public static System.Action<string, bool, Vector3> OnFeedbackRequested;

    [Header("资源配置")]
    public GameObject feedbackTextPrefab;

    private Canvas _uiCanvas; 
    private Camera _mainCamera;

    #region 生命周期
    void Awake()
    {
        FindRequiredComponents();
        OnFeedbackRequested += HandleFeedbackRequest;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        OnFeedbackRequested -= HandleFeedbackRequest;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    #endregion

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => FindRequiredComponents();

    private void FindRequiredComponents()
    {
        _mainCamera = Camera.main;
        _uiCanvas = FindObjectOfType<Canvas>();
    }

    private void HandleFeedbackRequest(string message, bool isSpecial, Vector3 worldPosition)
    {
        if (feedbackTextPrefab == null || _uiCanvas == null || _mainCamera == null) return;

        GameObject textInstance = Instantiate(feedbackTextPrefab, _uiCanvas.transform);
        Vector2 screenPos = _mainCamera.WorldToScreenPoint(worldPosition);
        
        // 稍微向上和左右偏移
        screenPos.x += Random.Range(-20f, 20f);
        screenPos.y += Random.Range(10f, 30f);
        textInstance.transform.position = screenPos;

        if (textInstance.TryGetComponent<PickupFeedbackAnimator>(out var animator))
        {
            animator.Setup(message, isSpecial);
        }
    }
}