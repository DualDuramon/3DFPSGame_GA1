using UnityEngine;

public class MeleeAttackHitbox : MonoBehaviour
{
    [SerializeField] private PlayerMeleeAttack _owner;
    private LayerMask _attackableLayer;

    private void Awake()
    {
        if (_owner == null)
        {
            _owner = GetComponentInParent<PlayerMeleeAttack>();
        }

    }

    private void Start()
    {
        _attackableLayer = _owner.AttackableLayer;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.gameObject.layer & (1 << _attackableLayer.value)) != 0)
        {
            _owner.NotifyHitboxDetected(other);
        }
    }
}
