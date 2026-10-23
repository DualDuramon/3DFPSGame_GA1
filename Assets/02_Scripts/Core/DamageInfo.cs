using UnityEngine;

public struct DamageInfo
{
    public float DamageValue; //데미지 양
    public Vector3 HitPoint;
    public Vector3 HitNormal;

    public DamageInfo(float damageValue, Vector3 hitPoint, Vector3 hitNormal)
    {
        DamageValue = damageValue;
        HitPoint = hitPoint;
        HitNormal = hitNormal;
    }
}
