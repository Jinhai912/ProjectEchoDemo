using UnityEngine;
using UnityEngine.InputSystem; // 确保引入命名空间

// 脚本名建议与类名一致，这里假设文件名是 MoveControll.cs
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))] // 最好也把PlayerInput加到依赖中
public class MoveController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector2 rawMoveInput; // 使用 Vector2 来存储原始输入

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 在 Inspector 中确保 Rigidbody 的 Freeze Rotation (X, Y, Z) 已勾选
        // 并且 Use Gravity 已取消勾选
    }

    // 由 PlayerInput 组件在输入变化时调用
    private void OnMove(InputValue value)
    {
        // 从输入事件中获取 Vector2 值并存储
        rawMoveInput = value.Get<Vector2>();
    }

    // 移除整个 Update() 函数，因为它与新的输入系统冲突

    void FixedUpdate()
    {
        // 1. 将二维输入转换为三维世界移动方向
        Vector3 moveDirection = new Vector3(rawMoveInput.x, 0f, rawMoveInput.y);

        // 2. 计算本物理帧的目标位置
        // moveDirection.normalized 确保斜向移动速度不会过快
        Vector3 targetPosition = rb.position + moveDirection.normalized * moveSpeed * Time.fixedDeltaTime;

        // 3. 使用 MovePosition 执行移动
        rb.MovePosition(targetPosition);
    }
}