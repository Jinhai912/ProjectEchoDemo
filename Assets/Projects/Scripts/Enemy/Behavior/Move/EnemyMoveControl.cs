using UnityEngine;

/// <summary>
/// 近战敌人移动与AI控制
/// 描述：负责基础近战单位的索敌、追击以及攻击逻辑。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class EnemyMoveControl : MonoBehaviour
{
    #region 属性配置
    [Header("战斗参数")]
    [SerializeField] private float moveSpeed = 3.5f;        
    [SerializeField] private float chaseRange = 20f;        
    [SerializeField] private float stopChasingRange = 25f;  
    [SerializeField] private float attackRange = 2f;        

    [Header("频率控制")]
    [SerializeField] private float targetFindInterval = 0.5f; 
    [SerializeField] private float attackCooldown = 1.5f;     

    [Header("攻击设定")]
    [SerializeField] private int attackDamage = 10;           
    #endregion

    #region 私有变量
    private enum EnemyState { Idle, Chasing, Attacking }
    private EnemyState _currentState;
    private Rigidbody _rb;
    private Transform _target;             
    private float _findTargetTimer;        
    private float _attackTimer;              
    #endregion

    #region 生命周期
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _currentState = EnemyState.Idle;
        _findTargetTimer = targetFindInterval;
        _attackTimer = 0f;
    }

    void Update()
    {
        UpdateTimers();
        UpdateState();
    }

    void FixedUpdate()
    {
        ExecuteMovement();
    }
    #endregion

    #region 核心逻辑
    private void UpdateTimers()
    {
        _findTargetTimer -= Time.deltaTime;
        if (_findTargetTimer <= 0f)
        {
            FindNearestTarget();
            _findTargetTimer = targetFindInterval;
        }

        if (_attackTimer > 0) _attackTimer -= Time.deltaTime;
    }

    private void UpdateState()
    {
        if (_target == null)
        {
            _currentState = EnemyState.Idle;
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, _target.position);

        switch (_currentState)
        {
            case EnemyState.Idle:
                if (distanceToTarget <= chaseRange) _currentState = EnemyState.Chasing;
                break;

            case EnemyState.Chasing:
                if (distanceToTarget > stopChasingRange)
                {
                    _target = null;
                    _currentState = EnemyState.Idle;
                }
                else if (distanceToTarget <= attackRange)
                {
                    _currentState = EnemyState.Attacking;
                }
                break;

            case EnemyState.Attacking:
                if (distanceToTarget > attackRange)
                {
                    _currentState = EnemyState.Chasing;
                }
                else if (_attackTimer <= 0)
                {
                    PerformAttack();
                }
                break;
        }
    }

    private void ExecuteMovement()
    {
        if (_currentState == EnemyState.Chasing && _target != null)
        {
            Vector3 direction = (_target.position - transform.position).normalized;
            direction.y = 0;
            Vector3 targetVelocity = direction * moveSpeed;
            _rb.velocity = new Vector3(targetVelocity.x, _rb.velocity.y, targetVelocity.z);

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRotation, 0.1f));
            }
        }
        else
        {
            _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
        }
    }

    private void FindNearestTarget()
    {
        Attackable[] targets = FindObjectsOfType<Attackable>();
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Attackable t in targets)
        {
            float dist = Vector3.Distance(transform.position, t.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = t.transform;
            }
        }

        _target = (closest != null && minDistance <= chaseRange) ? closest : null;
    }

    private void PerformAttack()
    {
        _attackTimer = attackCooldown;
        PlayerStates player = _target.GetComponent<PlayerStates>();
        if (player != null)
        {
            player.TakeDamage(attackDamage);
            Debug.Log($"[战斗] {gameObject.name} 命中玩家，造成 {attackDamage} 伤害");
        }
    }
    #endregion
}