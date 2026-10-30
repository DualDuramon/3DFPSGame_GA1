using UnityEngine;

public class PlayerMeleeAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerGunFire _gunFire;
    [SerializeField] private PlayerCommonStatus _commonStatus;
    [SerializeField] private Animator _animator;
    [SerializeField] private Collider _hitboxCollider;

    [SerializeField] private LayerMask _attackableLayer;


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

    public void ActivateMeleeCollider() => _hitboxCollider.enabled = true;
    public void DeactivateMeleeCollider() => _hitboxCollider.enabled = false;

}
