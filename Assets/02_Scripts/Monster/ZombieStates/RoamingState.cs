public class RoamingState : ZombieStateBase
{
    public override string Name => "Roaming";

    public override void Enter(Zombie zombie)
    {
        zombie.BeginRoaming();
    }

    public override void Tick(Zombie zombie)
    {
        if (zombie.IsPlayerDetected)
        {
            zombie.ChangeState(ZombieStates.Trace);
            return;
        }

        if (zombie.RoamingRadius <= 0f) return;

        if (zombie.MoveTowards(zombie.RoamingTarget))
        {
            zombie.AdvanceRoamingAngle();
        }
    }
}
