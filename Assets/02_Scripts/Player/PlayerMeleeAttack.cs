using UnityEngine;

public class PlayerMeleeAttack : MonoBehaviour
{

    [SerializeField] private PlayerGunFire _gunFire;
    [SerializeField] private Animator _animator;
    [SerializeField] private Collider _hitboxCollider;
    private int _meleeLayerIndex;


    private void Awake()
    {
        _gunFire = GetComponent<PlayerGunFire>();
        _animator = GetComponentInChildren<Animator>();
        _meleeLayerIndex = _animator.GetLayerIndex("MeleeLayer");
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
