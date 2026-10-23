/// <summary>
/// 데미지를 받을 수 있다는 "약속(인터페이스)"
/// </summary>
public interface IDamageable
{
    public void TakeDamage(DamageInfo damageInfo);
}