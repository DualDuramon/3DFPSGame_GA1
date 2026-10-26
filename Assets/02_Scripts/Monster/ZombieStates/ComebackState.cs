public class ComebackState : ZombieStateBase
{
    public override string Name => "Comback";

    public override void Tick(Zombie zombie)
    {
        if (zombie.MoveTowards(zombie.StartPosition))
        {
            zombie.ChangeState(ZombieStates.Idle);
        }
    }
}
