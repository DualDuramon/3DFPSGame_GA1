using UnityEngine;

public class ZombieAnimationModule : MonoBehaviour
{
    [SerializeField] private Animator _anim;
    [SerializeField] private Zombie _owner;

    private static int _attackTriggerHash = Animator.StringToHash("AttackTrigger");
    private static int _deathTriggerHash = Animator.StringToHash("DeathTrigger");

    private void Awake()
    {
        if (_owner == null)
        {
            _owner = GetComponentInParent<Zombie>();
        }
        if (_anim == null)
        {
            _anim = GetComponent<Animator>();
        }
    }

    public void PlayAttackAnimation()
    {
        _anim.SetTrigger(_attackTriggerHash);
    }
    public void PlayDeathAnimation()
    {
        _anim.SetTrigger(_deathTriggerHash);
    }

    public void OnAttackEvent()
    {
        _owner.HandleAttackPlayer();
    }
}
