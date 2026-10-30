using UnityEngine;

public class PlayerMeleeAttack : MonoBehaviour
{

    [SerializeField] private PlayerGunFire _gunFire;
    [SerializeField] private Animator _animator;
    [SerializeField] private Collider _hitboxCollider;


    private void Awake()
    {
        _gunFire = GetComponent<PlayerGunFire>();
        _animator = GetComponentInChildren<Animator>();
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

    public void ActivateMeleeCollider() => _hitboxCollider.enabled = true;
    public void DeactivateMeleeCollider() => _hitboxCollider.enabled = false;

}
