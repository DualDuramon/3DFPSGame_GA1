using System.Collections;
using UnityEngine;

//FSM(유한상태머신) : 유한개의 상태가 있고, 상태마다 행동을 다르게 함으로써 AI를 구현하는 유명한 방법
// - State : 객체가 가질 수 잇는 어떤 조건이나 상황
// - transition : 조건에 따라 현재 상태에서 다음 상태로 넘어가는 것
// - Machine : 위 방식에 따라 규칙을 세워둔 것

public enum ZombieState
{
    Idle,       //대기 : 제자리에서 플레이어를 기다림
    Trace,      // 추격 : 플레이어를 쫒아간다
    Comback,    //복귀 : 처음 자리로 돌아간다.
    Attack,     // 일정시간마다 :플레이어를 때린다
    Hit,        //피격 : 공격(총알, 폭탄)을 받아 잠시 멈칫한다
    Death,      // 죽음 : 쓰러지고 잠시 뒤 사라진다
    Roaming,    // 순찰 : 현재 지점 주변의 원형 경로를 돈다
}

public class Zombie : MonoBehaviour, IDamageable
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


    private ZombieState _state = ZombieState.Idle;


    public float CurrentHealthRatio => _health / _maxHealth;

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
        _player = GameObject.FindAnyObjectByType<PlayerMove>().gameObject;
        _startPosition = transform.position;
    }

    private void Update()
    {
        if (GameManager.Instance.GameState != GameState.Playing) return;

        switch (_state)
        {
            case ZombieState.Idle:
                Idle();
                break;
            case ZombieState.Roaming:
                Roaming();
                break;
            case ZombieState.Trace:
                Trace();
                break;
            case ZombieState.Attack:
                Attack();
                break;
            case ZombieState.Comback:
                Comback();
                break;
            case ZombieState.Hit:
                break;
            case ZombieState.Death:
                break;
            default:
                break;
        }
    }

    private void Idle()
    {
        if (TryDetectPlayer()) return;

        _idleTimer += Time.deltaTime;
        if (_idleTimer >= IdleDuration)
        {
            _idleTimer = 0f;
            _roamingCenter = transform.position;
            _startPosition = _roamingCenter;
            _roamingAngle = 0f;
            _state = ZombieState.Roaming;
        }
    }

    private bool TryDetectPlayer()
    {
        if (Vector3.Distance(transform.position, _player.transform.position) > _detectDistance)
        {
            return false;
        }

        GameLogger.Log($"{_state} => Trace");
        if (_state == ZombieState.Idle) _startPosition = transform.position;
        _idleTimer = 0f;
        _state = ZombieState.Trace;
        return true;
    }

    private void Roaming()
    {
        if (TryDetectPlayer()) return;
        if (_roamingRadius <= 0f) return;

        float radians = _roamingAngle * Mathf.Deg2Rad;
        Vector3 target = _roamingCenter + new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians)) * _roamingRadius;
        Vector3 direction = target - transform.position;

        direction.y = 0f;
        float distance = direction.magnitude;

        if (distance <= 0.01f)
        {
            _roamingAngle = Mathf.Repeat(_roamingAngle + RoamingAngleStep, 360f);
            return;
        }

        if (Time.deltaTime <= 0f) return;

        transform.forward = direction.normalized;
        float speed = Mathf.Min(Mathf.Max(0f, _moveSpeed), distance / Time.deltaTime);
        _controller.SimpleMove(direction.normalized * speed);
    }

    private void Trace()
    {
        float distanceToPlayer = Mathf.Abs(Vector3.Distance(transform.position, _player.transform.position));
        //플레이어를 쫒아간다
        if (distanceToPlayer > _combackDistance)
        {
            _state = ZombieState.Comback;
            return;
        }

        if (distanceToPlayer < _attackDistance)
        {
            _state = ZombieState.Attack;
            return;
        }

        Vector3 direction = _player.transform.position - transform.position;

        direction.y = 0;
        direction.Normalize();

        transform.forward = direction;
        _controller.SimpleMove(direction * _moveSpeed);

    }
    private void Comback()
    {
        Vector3 direction = _startPosition - transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;

        if (distance < 0.01f)
        {
            _idleTimer = 0f;
            _state = ZombieState.Idle;
            return;
        }

        if (Time.deltaTime <= 0f) return;
        transform.forward = direction.normalized;
        float speed = Mathf.Min(Mathf.Max(0f, _moveSpeed), distance / Time.deltaTime);
        _controller.SimpleMove(direction.normalized * speed);
    }

    private void Attack()
    {
        if (Vector3.Distance(transform.position, _player.transform.position) > _attackDistance)
        {
            _state = ZombieState.Trace;
            return;
        }

        _attackTimer += Time.deltaTime;
        if (_attackTimer > _attackInterval)
        {

            _animModule.PlayAttackAnimation();
            _attackTimer = 0f;
        }
    }

    public void TakeDamage(DamageInfo info)
    {
        if (_state == ZombieState.Death) return;
        //맞으면 잠깐 멈칫하고 싶다!

        _health -= info.DamageValue;

        if (_health <= 0f)
        {
            _state = ZombieState.Death;
            if (_shockCoroutine != null)
            {
                StopCoroutine(_shockCoroutine);
                _shockCoroutine = null;
            }
            if (_deathCoroutine != null) StopCoroutine(_deathCoroutine);
            _deathCoroutine = StartCoroutine(DeathCoroutine());
            return;
        }

        _state = ZombieState.Hit;
        if (_shockCoroutine != null)
        {
            StopCoroutine(_shockCoroutine);
        }
        _shockCoroutine = StartCoroutine(HitCoroutine());

        Transform effect = Instantiate(_bloodEffectPrefab, gameObject.transform).transform;
        effect.transform.position = info.HitPoint;
        effect.forward = info.HitNormal;
    }

    private IEnumerator HitCoroutine()
    {

        yield return new WaitForSeconds(0.2f);

        if (_state != ZombieState.Death) _state = ZombieState.Trace;
        _shockCoroutine = null;
    }

    private IEnumerator DeathCoroutine()
    {

        _animModule.PlayDeathAnimation();

        yield return new WaitForSecondsRealtime(1f);

        StopAllCoroutines();
        Destroy(gameObject);

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
}
