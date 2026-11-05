using UnityEngine;

public class HitInfo
{
    public readonly float Damage;
    public readonly HitType HitType;

    public HitInfo(float damage, HitType hitType)
    {
        Damage = damage;
        HitType = hitType;
    }
}
