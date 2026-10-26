using Lean.Pool;
using System;
using System.Collections;
using UnityEngine;

//FSM(유한상태머신) : 유한개의 상태가 있고, 상태마다 행동을 다르게 함으로써 AI를 구현하는 유명한 방법
// - State : 객체가 가질 수 잇는 어떤 조건이나 상황 (ZombieStates.cs의 상태 클래스)
// - transition : 조건에 따라 현재 상태에서 다음 상태로 넘어가는 것 (ChangeState)
// - Machine : 위 방식에 따라 규칙을 세워둔 것

public class Zombie : MonoBehaviour, IDamageable, IPoolable
{
    [Header("Reference")]
    [SerializeField] private CharacterController _controller;
    [SerializeField] private ZombieAnimationModule _animModule;
    [SerializeField] private GameObject _bloodEffectPrefab;
    private GameObject _player;

    [Header("Move Status")]
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _detectDistance = 5f;
    [SerializeField] private Vector3 _startPosition;
    [SerializeField] private float _combackDistance = 10f;

    [Header("Roaming Status")]
    [SerializeField] private float _roamingRadius = 3f;
    private const float IdleDuration = 3f;
    private const float RoamingAngleStep = 5f;
    private float _idleTimer;
    private float _roamingAngle;
    private Vector3 _roamingCenter;

    [Header("Attack Status")]
    [SerializeField] private float _attackDamage = 30f;
    [SerializeField] private float _attackDistance = 10f;
    [SerializeField] private float _attackTimer = 0f;
    [SerializeField] private float _attackInterval = 1f;
    [SerializeField] private LayerMask _attackableLayer;
    [SerializeField] private Transform _attackPosition;


    [Header("Damage Status")]
    [SerializeField] private float _health = 30f;
    [SerializeField] private float _maxHealth = 30f;
    [SerializeField] private float _shockTime = 0f;
    private Coroutine _shockCoroutine;
    private Coroutine _deathCoroutine;

    public event Action OnTakeHit;


    private ZombieStateBase _currentState = ZombieStates.Idle;


    public float CurrentHealthRatio => _health / _maxHealth;
    public bool IsPlayerDetected => DistanceToPlayer <= _detectDistance;
    public float DistanceToPlayer => Vector3.Distance(transform.position, _player.transform.position);
    public float CombackDistance => _combackDistance;
    public float AttackDistance => _attackDistance;
    public float RoamingRadius => _roamingRadius;
    public Vector3 StartPosition => _startPosition;
    public Vector3 RoamingTarget
    {
        get
        {
            float radians = _roamingAngle * Mathf.Deg2Rad;
            return _roamingCenter + new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians)) * _roamingRadius;
        }
    }

    private void Awake()
    {
        if (_controller == null)
        {
            _controller = GetComponent<CharacterController>();
        }
        if (_animModule == null)
        {
            _animModule = GetComponentInChildren<ZombieAnimationModule>();
        }

        _health = _maxHealth;
    }

    private void Start()
    {
        if (_player == null)
        {
            _player = GameObject.FindAnyObjectByType<PlayerMove>().gameObject;
            _startPosition = transform.position;
        }
    }

    private void Update()
    {
        if (GameManager.Instance.GameState != GameState.Playing) return;

        _currentState.Tick(this);
    }

    public void ChangeState(ZombieStateBase next)
    {
        GameLogger.Log($"{_currentState.Name} => {next.Name}");
        _currentState = next;
        _currentState.Enter(this);
    }

    public void ResetIdleTimer()
    {
        _idleTimer = 0f;
    }

    // 대기 시간이 다 지났으면 true
    public bool TickIdleTimer()
    {
        _idleTimer += Time.deltaTime;
        return _idleTimer >= IdleDuration;
    }

    public void SetStartPositionHere()
    {
        _startPosition = transform.position;
    }

    public void BeginRoaming()
    {
        _roamingCenter = transform.position;
        _startPosition = _roamingCenter;
        _roamingAngle = 0f;
    }

    public void AdvanceRoamingAngle()
    {
        _roamingAngle = Mathf.Repeat(_roamingAngle + RoamingAngleStep, 360f);
    }

    // target 쪽으로 이동한다. 이미 도착했으면 true
    public bool MoveTowards(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;

        if (distance <= 0.01f) return true;
        if (Time.deltaTime <= 0f) return false;

        transform.forward = direction.normalized;
        float speed = Mathf.Min(Mathf.Max(0f, _moveSpeed), distance / Time.deltaTime);
        _controller.SimpleMove(direction.normalized * speed);
        return false;
    }

    public void MoveToPlayer()
    {
        Vector3 direction = _player.transform.position - transform.position;

        direction.y = 0;
        direction.Normalize();

        transform.forward = direction;
        _controller.SimpleMove(direction * _moveSpeed);
    }

    public void TickAttackTimer()
    {
        _attackTimer += Time.deltaTime;
        if (_attackTimer > _attackInterval)
        {
            _animModule.PlayAttackAnimation();
            _attackTimer = 0f;
        }
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        _currentState.OnDamaged(this, damageInfo);
    }

    // 상태가 데미지를 받아들이기로 했을 때 호출된다 (ZombieStateBase.OnDamaged)
    public void ApplyDamage(DamageInfo damageInfo)
    {
        _health -= damageInfo.DamageValue;
        OnTakeHit?.Invoke();

        if (_health <= 0f)
        {
            ChangeState(ZombieStates.Death);
            if (_shockCoroutine != null)
            {
                StopCoroutine(_shockCoroutine);
                _shockCoroutine = null;
            }
            if (_deathCoroutine != null) StopCoroutine(_deathCoroutine);
            _deathCoroutine = StartCoroutine(DeathCoroutine());
            return;
        }

        ChangeState(ZombieStates.Hit);
        if (_shockCoroutine != null)
        {
            StopCoroutine(_shockCoroutine);
        }
        _shockCoroutine = StartCoroutine(HitCoroutine());

        Transform effect = Instantiate(_bloodEffectPrefab, gameObject.transform).transform;
        effect.transform.position = damageInfo.HitPoint;
        effect.forward = damageInfo.HitNormal;
    }

    private IEnumerator HitCoroutine()
    {

        yield return new WaitForSeconds(0.2f);

        _currentState.OnShockEnd(this);
        _shockCoroutine = null;
    }

    private IEnumerator DeathCoroutine()
    {

        _animModule.PlayDeathAnimation();
        ZombieSpawner.Instance.DecreaseZombieCount();

        yield return new WaitForSecondsRealtime(2f);
        ZombieSpawner.Instance.DeSpawnZombie(gameObject);
    }

    public void HandleAttackPlayer()
    {
        if (Physics.Raycast(_attackPosition.position, _attackPosition.forward, out RaycastHit hitInfo, _attackDistance * 1.5f, _attackableLayer))
        {
            if (hitInfo.collider.TryGetComponent(out IDamageable target))
            {
                DamageInfo damageInfo = new DamageInfo(_attackDamage, hitInfo.point, hitInfo.normal);
                target.TakeDamage(damageInfo);
            }
        }
    }

    public void OnSpawn()
    {
        _player = GameObject.FindAnyObjectByType<PlayerMove>().gameObject;
        _startPosition = transform.position;

        // 풀에서 재사용될 때 이전 생애의 상태를 초기화한다
        _health = _maxHealth;
        ChangeState(ZombieStates.Idle);
        _attackTimer = 0f;
        _shockCoroutine = null;
        _deathCoroutine = null;

        _animModule.ResetAnimation();

        OnTakeHit?.Invoke();    // 체력바를 가득 찬 상태로 갱신
    }

    public void OnDespawn()
    {
        StopAllCoroutines();
        _shockCoroutine = null;
        _deathCoroutine = null;
    }
}
