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
        if ((_attackableLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            _owner.NotifyHitboxDetected(other);
        }
    }
}
