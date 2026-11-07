using System.Collections.Generic;
using UnityEngine;

public class HitInfo
{
    public readonly Attack Owner;
    public readonly float Damage;
    public readonly HitType HitType;
    public readonly ElementType ElementType;
    public readonly List<IEffect> Effects;

    public HitInfo(Attack owner, float damage, HitType hitType, ElementType elementType, List<IEffect> effects)
    {
        Owner = owner;
        Damage = damage;
        HitType = hitType;
        ElementType = elementType;
        Effects = effects;
    }
}
