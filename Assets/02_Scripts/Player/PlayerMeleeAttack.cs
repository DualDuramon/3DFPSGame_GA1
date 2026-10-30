using UnityEngine;

public enum MeleeHitMode
{
    Collider,
    Fan,        //부채꼴 방식
}

public class PlayerMeleeAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerGunFire _gunFire;
    [SerializeField] private PlayerCommonStatus _commonStatus;
    [SerializeField] private Animator _animator;
    [SerializeField] private Collider _hitboxCollider;
    [SerializeField] private Transform _attackStartPosition;

    [Header("Common Settings")]
    [SerializeField] private LayerMask _attackableLayer;
    [SerializeField] private MeleeHitMode _meleeHitMode = MeleeHitMode.Collider;


    [Header("Fan Settings")]
    [SerializeField] private float _range = 2f;
    [SerializeField] private float _angle = 90f;


    public LayerMask AttackableLayer => _attackableLayer;


    private void Awake()
    {
        _gunFire = GetComponent<PlayerGunFire>();
        _animator = GetComponentInChildren<Animator>();

        if (_commonStatus == null)
        {
            _commonStatus = GetComponent<PlayerCommonStatus>();
        }

        if (_hitboxCollider == null)
        {
            //_hitboxCollider = GetComponentInChildren<Collider>();
            Debug.LogWarning($"{name} : 근접공격 공격 히트박스 할당 안됨!");
        }

        _hitboxCollider.enabled = false;
    }
    private void Update()
    {
        if (GameManager.Instance.GameState != GameState.Playing || _gunFire.WeaponMode != WeaponMode.Melee) return;

        if (Input.GetButtonDown("Fire2"))
        {
            _animator.SetTrigger("Slash");
        }
    }

    public void NotifyHitboxDetected(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IDamageable target))
        {
            DamageInfo info = new DamageInfo();
            info.DamageValue = _commonStatus.Damage; //TODO : meele Attack Damage로 바꾸기
            info.HitPoint = other.ClosestPoint(transform.position);
            info.HitNormal = transform.position - info.HitPoint;

            target.TakeDamage(info);
        }

    }

    //애니메이션 이벤트에서 호출할 이벤트
    public void FanAttackEvent()
    {
        if (_meleeHitMode != MeleeHitMode.Fan) return;
        Collider[] colliders = Physics.OverlapSphere(_attackStartPosition.position, _range, _attackableLayer);

        foreach (Collider collider in colliders)
        {
            Vector3 hitPoint = collider.ClosestPoint(_attackStartPosition.position);

            if (!CheckAngleInside(_attackStartPosition.position, hitPoint) ||
                Physics.Linecast(_attackStartPosition.position, hitPoint, LayerMask.GetMask("Wall")))
            {
                continue;
            }
            if (collider.TryGetComponent(out IDamageable damageable))
            {
                DamageInfo info = new DamageInfo();
                info.DamageValue = _commonStatus.Damage; //TODO : meele Attack Damage로 바꾸기
                info.HitPoint = hitPoint;
                info.HitNormal = _attackStartPosition.position - info.HitPoint;

                damageable.TakeDamage(info);
            }
        }

    }

    private bool CheckAngleInside(Vector3 origionPos, Vector3 targetPosition)
    {
        Vector3 playerToTarget = targetPosition - origionPos;
        playerToTarget.y = 0;
        playerToTarget.Normalize();

        Vector3 playerForward = transform.forward;
        playerForward.Normalize();

        return Vector3.Angle(playerForward, playerToTarget) < _angle * 0.5f;
    }

    public void ActivateMeleeCollider()
    {
        if (_meleeHitMode != MeleeHitMode.Collider) return;
        _hitboxCollider.enabled = true;
    }
    public void DeactivateMeleeCollider()
    {
        if (_meleeHitMode != MeleeHitMode.Collider) return;
        _hitboxCollider.enabled = false;
    }
}
