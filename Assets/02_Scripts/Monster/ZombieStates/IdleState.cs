public class IdleState : ZombieStateBase
{
    public override string Name => "Idle";

    public override void Enter(Zombie zombie)
    {
        zombie.ResetIdleTimer();
    }

    public override void Tick(Zombie zombie)
    {
        if (zombie.IsPlayerDetected)
        {
            zombie.SetStartPositionHere();
            zombie.ChangeState(ZombieStates.Trace);
            return;
        }

        if (zombie.TickIdleTimer())
        {
            zombie.ChangeState(ZombieStates.Roaming);
        }
    }
}
