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
}

public class Zombie : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private CharacterController _controller;
    [SerializeField] private Material _skinMaterial;
    private GameObject _player;

    [Header("Move Status")]
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _detectDistance = 5f;
    [SerializeField] private Vector3 _startPosition;
    [SerializeField] private float _combackDistance = 10f;

    [Header("Attack Status")]
    [SerializeField] private float _attackDistance = 10f;
    [SerializeField] private float _attackTimer = 0f;
    [SerializeField] private float _attackInterval = 1f;

    [Header("Damage Status")]
    [SerializeField] private float _health = 30f;
    [SerializeField] private float _shockTime = 0f;
    private Coroutine _shockCoroutine;
    private Coroutine _deathCoroutine;


    private ZombieState _state = ZombieState.Idle;

    private void Awake()
    {
        if (_controller == null)
        {
            _controller = GetComponent<CharacterController>();
        }
    }

    private void Start()
    {
        _player = GameObject.FindAnyObjectByType<PlayerMove>().gameObject;
    }

    private void Update()
    {
        switch (_state)
        {
            case ZombieState.Idle:
                Idle();
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
        //대기 : 제자리에서 플레이어를 기다림
        //그러다 가까워지면 Trace로 전이

        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance <= _detectDistance)
        {
            GameLogger.Log($"{_state} => Trance");
            _startPosition = transform.position;
            _state = ZombieState.Trace;
        }
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
        if (Vector3.Distance(transform.position, _startPosition) < 0.1f)
        {
            _state = ZombieState.Idle;
            return;
        }

        Vector3 direction = _startPosition - transform.position;
        direction.y = 0;

        transform.forward = direction;
        _controller.SimpleMove(direction.normalized * _moveSpeed);
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
            //TODO : 공격 애니메이션 실행
            GameLogger.Log($"공격! 플레이어 ! 공격!");
            _attackTimer = 0f;
        }
    }

    public void TakeDamage(float damage)
    {
        if (_state == ZombieState.Death) return;
        //맞으면 잠깐 멈칫하고 싶다!

        _health -= damage;

        if (_health < 0f)
        {
            _state = ZombieState.Death;
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
    }

    private IEnumerator HitCoroutine()
    {

        yield return new WaitForSeconds(0.2f);

        _state = ZombieState.Trace;
        _shockCoroutine = null;
    }

    private IEnumerator DeathCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            transform.Rotate(0f, 0f, 15f);

            yield return null;
        }

        yield return new WaitForSecondsRealtime(1f);

        StopAllCoroutines();
        Destroy(gameObject);

    }
}
