using UnityEngine;

public class ZombieAnimationModule : MonoBehaviour
{
    [SerializeField] private Animator _anim;
    [SerializeField] private Zombie _owner;

    private static int _attackTriggerHash = Animator.StringToHash("AttackTrigger");
    private static int _deathBoolHash = Animator.StringToHash("IsDead");

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
        _anim.SetBool(_deathBoolHash, true);
    }

    public void ResetAnimation()
    {
        _anim.SetBool(_deathBoolHash, false);
        _anim.ResetTrigger(_attackTriggerHash);

        _anim.Rebind();
        _anim.Update(0f);
    }

    public void OnAttackEvent()
    {
        _owner.HandleAttackPlayer();
    }
}
