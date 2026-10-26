public static class ZombieStates
{
    public static readonly ZombieStateBase Idle = new IdleState();          //대기 : 제자리에서 플레이어를 기다림
    public static readonly ZombieStateBase Roaming = new RoamingState();    // 순찰 : 현재 지점 주변의 원형 경로를 돈다
    public static readonly ZombieStateBase Trace = new TraceState();        // 추격 : 플레이어를 쫒아간다
    public static readonly ZombieStateBase Comback = new ComebackState();   //복귀 : 처음 자리로 돌아간다.
    public static readonly ZombieStateBase Attack = new AttackState();      // 일정시간마다 :플레이어를 때린다
    public static readonly ZombieStateBase Hit = new HitState();            //피격 : 공격(총알, 폭탄)을 받아 잠시 멈칫한다
    public static readonly ZombieStateBase Death = new DeathState();        // 죽음 : 쓰러지고 잠시 뒤 사라진다
}
