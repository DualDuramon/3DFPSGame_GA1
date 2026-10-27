using UnityEngine;

public class ZombieAnimationModule : MonoBehaviour
{
    [SerializeField] private Animator _anim;
    [SerializeField] private Zombie _owner;

    private static readonly int IdleToMove = Animator.StringToHash("IdleToMove");
    private static readonly int MoveToIdle = Animator.StringToHash("MoveToIdle");
    private static readonly int MoveAttackDelay = Animator.StringToHash("MoveAttackDelay");
    private static readonly int StartAttack = Animator.StringToHash("StartAttack");
    private static readonly int AttackToMove = Animator.StringToHash("AttackToMove");
    private static readonly int IdleState = Animator.StringToHash("Base Layer.Z_Idle");
    private static readonly int MoveState = Animator.StringToHash("Base Layer.Move");
    private static readonly int DelayState = Animator.StringToHash("Base Layer.AttackProcess.AttackDelay");
    private static readonly int DeathState = Animator.StringToHash("Death");
    private static readonly int HitTrigger = Animator.StringToHash("Hit");
    private static readonly int HitState = Animator.StringToHash("Base Layer.Hit");

    private enum Mode { Idle, Move, Attack, Hit, Dead }
    private Mode _mode;
    private bool _attackRequested;
    private bool _canDealDamage;
    private bool _hitPending;

    public bool IsHitAnimationPlaying => _anim != null && _anim.isActiveAndEnabled &&
        (_hitPending || _anim.GetCurrentAnimatorStateInfo(0).fullPathHash == HitState ||
        (_anim.IsInTransition(0) && _anim.GetNextAnimatorStateInfo(0).fullPathHash == HitState));

    public bool IsWaitingForAttack => _mode == Mode.Attack && !_attackRequested &&
        _anim != null && _anim.isActiveAndEnabled && !_anim.IsInTransition(0) &&
        _anim.GetCurrentAnimatorStateInfo(0).fullPathHash == DelayState;

    private void Awake()
    {
        if (_owner == null) _owner = GetComponentInParent<Zombie>();
        if (_anim == null) _anim = GetComponent<Animator>();
        // NavMeshAgent owns movement, including during attack clips.
        if (_anim != null) _anim.applyRootMotion = false;
    }

    private void LateUpdate()
    {
        if (_anim == null || !_anim.isActiveAndEnabled || _mode == Mode.Dead) return;
        if (_mode == Mode.Hit)
        {
            if (_anim.GetCurrentAnimatorStateInfo(0).fullPathHash == HitState)
                _hitPending = false;
            return;
        }

        // Discard stale triggers and follow the graph from the stable current state.
        // A command received during a transition is applied once that transition ends.
        ResetTriggers();
        if (_anim.IsInTransition(0)) return;
        int state = _anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        if (_mode == Mode.Attack)
        {
            if (state == IdleState)
            {
                _anim.SetTrigger(IdleToMove);
            }
            else if (state == MoveState)
            {
                _anim.SetTrigger(MoveAttackDelay);
            }
            else if (state == DelayState && _attackRequested)
            {
                _attackRequested = false;
                _canDealDamage = true;
                _anim.SetTrigger(StartAttack);
            }
        }
        else if (state != IdleState && state != MoveState)
        {
            _anim.SetTrigger(AttackToMove);
        }
        else if (state == IdleState && _mode == Mode.Move)
        {
            _anim.SetTrigger(IdleToMove);
        }
        else if (state == MoveState && _mode == Mode.Idle)
        {
            _anim.SetTrigger(MoveToIdle);
        }
    }

    public void BeginAttack()
    {
        _mode = Mode.Attack;
        _attackRequested = false;
        _canDealDamage = false;
    }

    public void PlayAttackAnimation()
    {
        if (IsWaitingForAttack) _attackRequested = true;
    }

    public void SetMoveAnimation(bool isMoveStart)
    {
        if (_mode == Mode.Dead) return;
        _mode = isMoveStart ? Mode.Move : Mode.Idle;
        _attackRequested = false;
        _canDealDamage = false;
    }

    public void PlayDeathAnimation()
    {
        _mode = Mode.Dead;
        _attackRequested = false;
        _canDealDamage = false;
        if (_anim == null) return;
        ResetTriggers();
        _hitPending = false;
        _anim.ResetTrigger(HitTrigger);
        _anim.speed = 1f;
        _anim.SetTrigger(DeathState);
    }

    public void PlayHitAnimation()
    {
        if (_mode == Mode.Dead || _anim == null) return;
        _mode = Mode.Hit;
        _attackRequested = false;
        _canDealDamage = false;
        _hitPending = true;
        ResetTriggers();
        _anim.ResetTrigger(HitTrigger);
        _anim.SetTrigger(HitTrigger);
    }

    public void EndHitAnimation()
    {
        if (_mode == Mode.Hit) _mode = Mode.Idle;
    }

    public void ResetAnimation()
    {
        _mode = Mode.Idle;
        _hitPending = false;
        _attackRequested = false;
        _canDealDamage = false;
        if (_anim == null) return;
        _anim.speed = 1f;
        _anim.Rebind();
        _anim.ResetTrigger(HitTrigger);
        _anim.ResetTrigger(DeathState);
        ResetTriggers();
        _anim.Update(0f);
    }

    private void ResetTriggers()
    {
        _anim.ResetTrigger(IdleToMove);
        _anim.ResetTrigger(MoveToIdle);
        _anim.ResetTrigger(MoveAttackDelay);
        _anim.ResetTrigger(StartAttack);
        _anim.ResetTrigger(AttackToMove);
    }

    public void OnAttackEvent()
    {
        if (!_canDealDamage || _mode != Mode.Attack || _owner == null) return;
        _canDealDamage = false;
        _owner.HandleAttackPlayer();
    }
}
