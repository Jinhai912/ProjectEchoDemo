// ArtilleristAI.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArtilleristAI : MonoBehaviour
{
    [Header("战斗参数")]
    public float attackRange = 15f;     // 在这个距离开始攻击
    public float stopChasingRange = 20f;  // 超过这个距离就放弃
    public float rotationSpeed = 5f;
    public float attackCooldown = 2f;

    [Header("组件引用")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    private Transform player;
    private Rigidbody rb;
    private float attackTimer;

    private enum State { Idle, Attacking } // 简化的状态机
    private State currentState;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) player = playerGO.transform;
        currentState = State.Idle;
    }

    void Update()
    {
        if (player == null) return;
        
        if (attackTimer > 0) { attackTimer -= Time.deltaTime; }

        UpdateState();
        ExecuteStateAction();
    }

    void UpdateState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            currentState = State.Attacking;
        }
        else if (distanceToPlayer > stopChasingRange)
        {
            currentState = State.Idle;
        }
        else // 在攻击范围和放弃范围之间，可以添加一个追击状态
        {
            currentState = State.Idle; // 为简化，我们先让它停在原地
        }
    }

    void ExecuteStateAction()
    {
        // 任何状态下，只要能看到玩家，都应该朝向玩家
        if (currentState != State.Idle)
        {
            LookAtPlayer();
        }
        
        rb.velocity = Vector3.zero; // 远程怪不移动

        if (currentState == State.Attacking && attackTimer <= 0)
        {
            Attack();
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    void Attack()
    {
        attackTimer = attackCooldown;

        if (projectilePrefab == null || firePoint == null) return;
        
        // 我们需要确保发射点也朝向玩家
        firePoint.LookAt(player);
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }
}