// 좀비 FSM 상태의 공통 부모
// - 상태 객체는 필드를 갖지 않는다(타이머·위치 등 가변 데이터는 Zombie가 보유) → 모든 좀비가 같은 인스턴스를 공유한다.
// - 상태는 Zombie의 메서드를 호출해서 동작하고, 전이는 zombie.ChangeState()로 한다.
public abstract class ZombieStateBase
{
    public abstract string Name { get; }

    public virtual void Enter(Zombie zombie) { }
    public abstract void Tick(Zombie zombie);

    // 피격 : 기본은 데미지를 적용한다. 받지 않는 상태(Death)는 override해서 무시한다
    public virtual void OnDamaged(Zombie zombie, DamageInfo damageInfo)
    {
        zombie.ApplyDamage(damageInfo);
    }

    // 피격 경직이 끝났을 때 : 기본은 아무것도 하지 않는다
    public virtual void OnShockEnd(Zombie zombie) { }
}
