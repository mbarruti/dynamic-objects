using System.Collections.Generic;

public class Hit
{
    public float Damage { get; private set; }
    public HitType HitType { get; private set; }
    public ElementType ElementType { get; private set; }
    public List<EffectDefinition> EffectDefs { get; private set; }

    public Hit(float damage, HitType hitType, ElementType elementType, List<EffectDefinition> effectDefs)
    {
        Damage = damage;
        HitType = hitType;
        ElementType = elementType;
        EffectDefs = effectDefs;
    }
}
