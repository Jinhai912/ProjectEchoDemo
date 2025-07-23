using UnityEngine;

// 确保敌人对象有 Rigidbody 组件
[RequireComponent(typeof(Rigidbody))]
public class EnemyMoveControl : MonoBehaviour
{
    [Header("追击参数")]
    public float moveSpeed = 3.5f;        // 敌人的移动速度
    public float chaseRange = 20f;        // 进入追击状态的范围
    public float stopChasingRange = 25f;  // 放弃追击的范围 (比chaseRange大，防止在边缘反复横跳)
    public float attackRange = 2f;        // 停止移动并发起攻击的范围

    [Header("索敌参数")]
    public float targetFindInterval = 0.5f; // 寻找目标的频率（0.5秒一次，避免每帧都找）

    private Rigidbody rb;
    private Transform target;             // 当前追击的目标
    private float findTargetTimer;        // 寻找目标的计时器

    // 敌人的状态枚举
    private enum EnemyState { Idle, Chasing, Attacking }
    private EnemyState currentState;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 推荐在 Inspector 中冻结旋转，防止敌人翻滚
        // rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        
        currentState = EnemyState.Idle; // 初始状态为空闲
        findTargetTimer = targetFindInterval;
    }

    void Update()
    {
        // 使用计时器来控制索敌频率
        findTargetTimer -= Time.deltaTime;
        if (findTargetTimer <= 0f)
        {
            FindTargetPlayer();
            findTargetTimer = targetFindInterval;
        }

        // 根据当前状态执行不同的逻辑
        UpdateState();
    }

    void FixedUpdate()
    {
        // 在 FixedUpdate 中执行移动，因为涉及到物理
        if (currentState == EnemyState.Chasing && target != null)
        {
            MoveTowardsTarget();
        }
        else
        {
            // 如果不是追击状态，则停止移动
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    /// <summary>
    /// 更新敌人的状态机
    /// </summary>
    void UpdateState()
    {
        if (target == null)
        {
            // 如果没有目标，始终处于空闲状态
            currentState = EnemyState.Idle;
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                // 如果在空闲状态下，目标进入了追击范围，则开始追击
                if (distanceToTarget <= chaseRange)
                {
                    currentState = EnemyState.Chasing;
                }
                break;

            case EnemyState.Chasing:
                // 如果在追击状态下，目标超出了放弃追击的范围，则返回空闲
                if (distanceToTarget > stopChasingRange)
                {
                    target = null; // 丢失目标
                    currentState = EnemyState.Idle;
                }
                // 如果进入了攻击范围，则切换到攻击状态
                else if (distanceToTarget <= attackRange)
                {
                    currentState = EnemyState.Attacking;
                }
                break;

            case EnemyState.Attacking:
                // 如果在攻击状态下，目标又跑出了攻击范围，则继续追击
                if (distanceToTarget > attackRange)
                {
                    currentState = EnemyState.Chasing;
                }
                // 在这里可以执行攻击逻辑
                // Debug.Log("正在攻击 " + target.name);
                break;
        }
    }

    /// <summary>
    /// 寻找并设置最近的可攻击目标
    /// </summary>
    void FindTargetPlayer()
    {
        // 你的索敌逻辑很棒，我们直接用
        Attackable[] attackables = FindObjectsOfType<Attackable>();
        Transform nearestTarget = null;
        float minDistance = Mathf.Infinity;

        foreach (Attackable attackable in attackables)
        {
            float distance = Vector3.Distance(transform.position, attackable.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestTarget = attackable.transform;
            }
        }

        // 即使找到了最近的，也要判断是否在追击范围内
        if (nearestTarget != null && minDistance <= chaseRange)
        {
            target = nearestTarget;
        }
        else
        {
            // 如果没有在范围内的目标，则清空目标
            target = null;
        }
    }

    /// <summary>
    /// 朝着目标移动
    /// </summary>
    void MoveTowardsTarget()
    {
        // 计算朝向目标的方向向量
        Vector3 direction = (target.position - transform.position).normalized;

        direction.y = 0;

        // 计算目标速度
        Vector3 targetVelocity = direction * moveSpeed;

        // 应用速度到 Rigidbody (只改变XZ平面的速度，保留Y轴速度以应对重力等)
        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);

        // 让敌人朝向移动方向
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            // 平滑转向，避免瞬间转身
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 0.1f));
        }
    }
}