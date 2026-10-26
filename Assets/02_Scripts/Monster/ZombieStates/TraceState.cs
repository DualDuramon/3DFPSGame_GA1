public class TraceState : ZombieStateBase
{
    public override string Name => "Trace";

    public override void Tick(Zombie zombie)
    {
        float distanceToPlayer = zombie.DistanceToPlayer;

        if (distanceToPlayer > zombie.CombackDistance)
        {
            zombie.ChangeState(ZombieStates.Comback);
            return;
        }

        if (distanceToPlayer < zombie.AttackDistance)
        {
            zombie.ChangeState(ZombieStates.Attack);
            return;
        }

        zombie.MoveToPlayer();
    }
}
