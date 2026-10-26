public class AttackState : ZombieStateBase
{
    public override string Name => "Attack";

    public override void Tick(Zombie zombie)
    {
        if (zombie.DistanceToPlayer > zombie.AttackDistance)
        {
            zombie.ChangeState(ZombieStates.Trace);
            return;
        }

        zombie.TickAttackTimer();
    }
}
