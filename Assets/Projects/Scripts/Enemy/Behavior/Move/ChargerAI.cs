using UnityEngine;

/// <summary>
/// 冲锋兵：追击 → 蓄力预警 → 直线冲锋 → 硬直恢复。
/// 描述：冲锋期间接触玩家造成一次伤害，蓄力阶段会显示占位预警条。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ChargerAI : MonoBehaviour
{
    [Header("追击")]
    public float chaseSpeed = 2.8f;
    public float chaseRange = 20f;

    [Header("冲锋")]
    public float chargeRange = 9f;
    public float windupTime = 0.65f;
    public float chargeSpeed = 14f;
    public float chargeDuration = 0.45f;
    public float recoverTime = 0.9f;
    public float chargeCooldown = 2.5f;
    public int chargeDamage = 15;

    [Header("表现占位")]
    public GameObject telegraph;

    private enum State { Idle, Chase, Windup, Charge, Recover }

    private State _state;
    private Rigidbody _rb;
    private Transform _player;
    private float _stateTimer;
    private float _cooldownTimer;
    private Vector3 _chargeDirection;
    private bool _hitThisCharge;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _player = player.transform;

        if (telegraph != null) telegraph.SetActive(false);
        _state = State.Idle;
    }

    private void Update()
    {
        if (_player == null) return;

        if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
        float distance = Vector3.Distance(transform.position, _player.position);

        switch (_state)
        {
            case State.Idle:
                if (distance <= chaseRange) _state = State.Chase;
                break;

            case State.Chase:
                RotateTowards(_player.position);
                if (distance <= chargeRange && _cooldownTimer <= 0f)
                {
                    StartWindup();
                }
                break;

            case State.Windup:
                RotateTowards(_player.position);
                _stateTimer -= Time.deltaTime;
                if (_stateTimer <= 0f) StartCharge();
                break;

            case State.Charge:
                _stateTimer -= Time.deltaTime;
                if (_stateTimer <= 0f) StartRecover();
                break;

            case State.Recover:
                _stateTimer -= Time.deltaTime;
                if (_stateTimer <= 0f)
                {
                    _state = State.Chase;
                    _cooldownTimer = chargeCooldown;
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;

        Vector3 velocity = Vector3.zero;
        if (_state == State.Chase && _player != null)
        {
            Vector3 direction = _player.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f)
            {
                velocity = direction.normalized * chaseSpeed;
            }
        }
        else if (_state == State.Charge)
        {
            velocity = _chargeDirection * chargeSpeed;
        }

        _rb.velocity = new Vector3(velocity.x, _rb.velocity.y, velocity.z);
    }

    private void StartWindup()
    {
        _state = State.Windup;
        _stateTimer = windupTime;
        if (telegraph != null) telegraph.SetActive(true);
    }

    private void StartCharge()
    {
        if (telegraph != null) telegraph.SetActive(false);

        Vector3 direction = _player.position - transform.position;
        direction.y = 0f;
        _chargeDirection = direction.sqrMagnitude > 0.01f ? direction.normalized : transform.forward;

        _state = State.Charge;
        _stateTimer = chargeDuration;
        _hitThisCharge = false;
    }

    private void StartRecover()
    {
        _state = State.Recover;
        _stateTimer = recoverTime;
        _rb.velocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_state != State.Charge || _hitThisCharge) return;
        if (!collision.collider.CompareTag("Player")) return;

        PlayerStates player = collision.collider.GetComponent<PlayerStates>();
        if (player != null)
        {
            player.TakeDamage(chargeDamage);
            _hitThisCharge = true;
            StartRecover();
        }
    }

    private void RotateTowards(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion target = Quaternion.LookRotation(direction);
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, target, 0.2f));
    }
}
