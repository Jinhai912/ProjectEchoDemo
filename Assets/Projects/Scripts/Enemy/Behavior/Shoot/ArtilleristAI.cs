using UnityEngine;

/// <summary>
/// 远程敌人AI控制器
/// 描述：控制远程单位的站桩射击逻辑、索敌朝向及开火频率。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ArtilleristAI : MonoBehaviour
{
    #region 属性配置
    [Header("战斗参数")]
    public float attackRange = 15f;     
    public float stopChasingRange = 20f;  
    public float rotationSpeed = 5f;
    public float attackCooldown = 2f;

    [Header("组件引用")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    #endregion

    #region 私有变量
    private enum State { Idle, Attacking }
    private State _currentState;
    private Transform _player;
    private Rigidbody _rb;
    private float _attackTimer;
    #endregion

    #region 生命周期
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) _player = playerGO.transform;
        _currentState = State.Idle;
    }

    void Update()
    {
        if (_player == null) return;
        
        if (_attackTimer > 0) _attackTimer -= Time.deltaTime;

        UpdateAIState();
        ExecuteAILogic();
    }
    #endregion

    #region 核心逻辑
    private void UpdateAIState()
    {
        float distance = Vector3.Distance(transform.position, _player.position);

        if (distance <= attackRange)
            _currentState = State.Attacking;
        else if (distance > stopChasingRange)
            _currentState = State.Idle;
    }

    private void ExecuteAILogic()
    {
        if (_currentState != State.Idle)
        {
            LookAtTarget();
            if (_currentState == State.Attacking && _attackTimer <= 0)
            {
                PerformRangedAttack();
            }
        }
        _rb.velocity = Vector3.zero; 
    }

    private void LookAtTarget()
    {
        Vector3 dir = (_player.position - transform.position).normalized;
        dir.y = 0;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
    }

    private void PerformRangedAttack()
    {
        _attackTimer = attackCooldown;
        if (projectilePrefab && firePoint)
        {
            firePoint.LookAt(_player);
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        }
    }
    #endregion
}