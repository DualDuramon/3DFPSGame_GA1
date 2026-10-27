using Lean.Pool;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

//FSM(유한상태머신) : 유한개의 상태가 있고, 상태마다 행동을 다르게 함으로써 AI를 구현하는 유명한 방법
// - State : 객체가 가질 수 잇는 어떤 조건이나 상황 (ZombieStates.cs의 상태 클래스)
// - transition : 조건에 따라 현재 상태에서 다음 상태로 넘어가는 것 (ChangeState)
// - Machine : 위 방식에 따라 규칙을 세워둔 것

[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour, IDamageable, IPoolable
{
    [Header("Reference")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private ZombieAnimationModule _animModule;
    [SerializeField] private GameObject _bloodEffectPrefab;
    private GameObject _player;

    [Header("Move Status")]
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _detectDistance = 5f;
    [SerializeField] private Vector3 _startPosition;
    [SerializeField] private float _combackDistance = 0.7f;

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

    [Header("Jump Status")]
    [SerializeField] private float _jumpHeight = 1.5f;      // 시작·끝을 잇는 직선 위로 솟는 기본 높이
    [SerializeField] private float _jumpSpeed = 5f;         // 수평 이동 속도 (점프 시간 계산용)
    [SerializeField] private float _minJumpDuration = 0.3f;
    private Coroutine _jumpCoroutine;

    public event Action OnTakeHit;


    private ZombieStateBase _currentState = ZombieStates.Idle;


    public float CurrentHealthRatio => _health / _maxHealth;
    public bool IsPlayerDetected => DistanceToPlayer <= _detectDistance;
    public float DistanceToPlayer => Vector3.Distance(transform.position, _player.transform.position);
    public bool IsJumping => _jumpCoroutine != null;
    public float CombackDistance => _combackDistance;
    public float AttackDistance => _attackDistance;
    public float RoamingRadius => _roamingRadius;
    //public bool HasArrived => _agent.pathStatus == NavMeshPathStatus.PathComplete;
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
        if (_animModule == null)
        {
            _animModule = GetComponentInChildren<ZombieAnimationModule>();
        }
        if (_agent == null)
        {
            _agent = GetComponent<NavMeshAgent>();
        }
        // 링크를 자동으로 건너면 직선으로 미끄러지므로, 직접 포물선으로 점프시킨다 (JumpCoroutine)
        _agent.autoTraverseOffMeshLink = false;

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

        // 점프 중에는 상태 Tick을 멈춘다
        if (IsJumping) return;
        // 경로상의 링크(NavMeshSurface Generated Link) 시작점에 도착하면 점프한다
        if (_agent.isOnOffMeshLink)
        {
            _jumpCoroutine = StartCoroutine(JumpCoroutine());
            return;
        }

        _currentState.Tick(this);
    }

    private IEnumerator JumpCoroutine()
    {
        OffMeshLinkData data = _agent.currentOffMeshLinkData;
        // 링크 시작점이 아닌 현재 위치에서 출발해야 순간이동하지 않는다
        Vector3 start = transform.position;
        Vector3 end = data.endPos + Vector3.up * _agent.baseOffset;

        Vector3 flat = end - start;
        flat.y = 0f;
        if (flat.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(flat);

        float duration = Mathf.Max(_minJumpDuration, flat.magnitude / _jumpSpeed);
        // 올라가는 링크는 높이차만큼 더 솟아야 자연스럽다
        float height = _jumpHeight + Mathf.Max(0f, end.y - start.y) * 0.5f;

        float t = 0f;
        _animModule.PlayJumpStartAnimation();

        while (t < 1f)
        {
            // 게임오버 등 Playing이 아니면 공중에서 멈춘다
            if (GameManager.Instance.GameState == GameState.Playing)
                t = Mathf.Clamp01(t + Time.deltaTime / duration);

            // 직선 보간 + 4h·t(1-t) : t = 0.5에서 최고점 h
            Vector3 position = Vector3.Lerp(start, end, t);
            position.y += height * 4f * t * (1f - t);
            transform.position = position;
            yield return null;
        }

        // 링크 이동 완료 → 에이전트가 끝점부터 원래 경로를 이어간다
        _agent.CompleteOffMeshLink();
        _animModule.PlayLandingAnimation();
        _jumpCoroutine = null;
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

        if (distance <= Mathf.Max(0.1f, _agent.stoppingDistance))
        {
            _animModule.SetMoveAnimation(false);
            return true;
        }
        if (Time.deltaTime <= 0f) return false;

        _agent.SetDestination(target);
        _animModule.SetMoveAnimation(true);

        return false;
    }

    public void MoveToPlayer()
    {
        _agent.SetDestination(_player.transform.position);
        _animModule.SetMoveAnimation(true);
    }

    public void BeginAttack()
    {
        StopAgent();
        _attackTimer = 0f;
        _animModule.BeginAttack();
    }

    public void TickAttackTimer()
    {
        Vector3 direction = _player.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);

        if (!_animModule.IsWaitingForAttack) return;
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
        SpawnBloodEffect(damageInfo);

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

        // 공중에서는 경직되지 않는다 (StopAgent의 ResetPath가 링크 이동과 충돌하므로)
        if (IsJumping)
        {
            SpawnBloodEffect(damageInfo);
            return;
        }

        ChangeState(ZombieStates.Hit);
        if (_shockCoroutine != null)
        {
            StopCoroutine(_shockCoroutine);
        }
        _shockCoroutine = StartCoroutine(HitCoroutine());

    }

    private void SpawnBloodEffect(DamageInfo damageInfo)
    {
        Transform effect = Instantiate(_bloodEffectPrefab).transform;
        effect.transform.position = damageInfo.HitPoint;
        effect.forward = damageInfo.HitNormal;
        effect.SetParent(gameObject.transform);
    }

    private IEnumerator HitCoroutine()
    {

        StopAgent();
        _animModule.PlayHitAnimation();
        yield return new WaitForSeconds(_shockTime);
        yield return new WaitUntil(() => !_animModule.IsHitAnimationPlaying);

        _animModule.EndHitAnimation();
        _currentState.OnShockEnd(this);
        _shockCoroutine = null;
    }

    public void StopAgent()
    {
        _agent.ResetPath();
        _agent.velocity = Vector3.zero;

        _animModule.SetMoveAnimation(false);
    }

    private IEnumerator DeathCoroutine()
    {
        // 점프 중에 죽으면 착지한 뒤에 쓰러진다
        yield return new WaitUntil(() => !IsJumping);
        StopAgent();

        _animModule.PlayDeathAnimation();
        ZombieSpawner.Instance.DecreaseZombieCount();

        yield return new WaitForSecondsRealtime(2f);
        ZombieSpawner.Instance.DeSpawnZombie(gameObject);
    }

    public void HandleAttackPlayer()
    {
        if (_currentState != ZombieStates.Attack || _health <= 0f ||
            GameManager.Instance.GameState != GameState.Playing)
        {
            return;
        }

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
        _animModule.ResetAnimation();
        ChangeState(ZombieStates.Idle);
        _attackTimer = 0f;
        _shockCoroutine = null;
        _deathCoroutine = null;
        _jumpCoroutine = null;

        OnTakeHit?.Invoke();    // 체력바를 가득 찬 상태로 갱신
    }

    public void OnDespawn()
    {
        StopAllCoroutines();
        _shockCoroutine = null;
        _deathCoroutine = null;
        _jumpCoroutine = null;

        // 점프 도중 회수되면 링크 점유 상태를 정리한다
        if (_agent.isOnNavMesh && _agent.isOnOffMeshLink)
        {
            _agent.CompleteOffMeshLink();
        }
    }
}
