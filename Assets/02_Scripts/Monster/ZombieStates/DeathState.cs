public class DeathState : ZombieStateBase
{
    public override string Name => "Death";

    public override void Tick(Zombie zombie) { }

    public override void OnDamaged(Zombie zombie, DamageInfo damageInfo) { }
}
