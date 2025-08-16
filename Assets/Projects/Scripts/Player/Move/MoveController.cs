using UnityEngine;
using UnityEngine.InputSystem; // 确保引入命名空间

// 脚本名建议与类名一致，这里假设文件名是 MoveControll.cs
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))] // 最好也把PlayerInput加到依赖中
public class MoveController : MonoBehaviour
{
    private Rigidbody rb;
    private Vector2 rawMoveInput; // 使用 Vector2 来存储原始输入
    private bool canMove = true; // 新增一个控制开关

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 在 Inspector 中确保 Rigidbody 的 Freeze Rotation (X, Y, Z) 已勾选
        // 并且 Use Gravity 已取消勾选
    }

    void OnEnable() { GameManager.OnGameStateChanged += HandleGameStateChange; }
    void OnDisable() { GameManager.OnGameStateChanged -= HandleGameStateChange; }
    private void HandleGameStateChange(GameManager.GameState newState)
    {
        // 只有在 Playing 状态下才允许移动
        canMove = (newState == GameManager.GameState.Playing);
    }

    // 由 PlayerInput 组件在输入变化时调用
    private void OnMove(InputValue value)
    {
        // 从输入事件中获取 Vector2 值并存储
        if (!canMove) // 检查开关
        {
            rawMoveInput = Vector2.zero; // 如果不能移动，清空输入
            return;
        }
        rawMoveInput = value.Get<Vector2>();
    }

    // 移除整个 Update() 函数，因为它与新的输入系统冲突

    void FixedUpdate()
    {
        if (!canMove) { rb.velocity = Vector3.zero; return; }

        // --- 核心修改在这里 ---
        // 1. 在执行移动前，从 PlayerData 获取当前应有的移动速度
        float currentMoveSpeed = 5f; // 先给一个默认值，以防 PlayerData 不存在
        if (PlayerData.Instance != null)
        {
            currentMoveSpeed = PlayerData.Instance.MoveSpeed;
        }
        // --- 修改结束 ---

        Vector3 moveDirection = new Vector3(rawMoveInput.x, 0f, rawMoveInput.y);

        // 2. 在计算目标位置时，使用我们刚刚获取的 currentMoveSpeed
        Vector3 targetPosition = rb.position + moveDirection.normalized * currentMoveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(targetPosition);
    }


}