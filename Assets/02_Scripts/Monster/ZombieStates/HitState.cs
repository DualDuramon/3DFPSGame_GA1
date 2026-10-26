public class HitState : ZombieStateBase
{
    public override string Name => "Hit";

    public override void Tick(Zombie zombie) { }

    public override void OnShockEnd(Zombie zombie)
    {
        zombie.ChangeState(ZombieStates.Trace);
    }
}
