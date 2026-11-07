using System.Collections.Generic;

public class Hit
{
    public float Damage { get; private set; }
    public HitType HitType { get; private set; }
    public ElementType ElementType { get; private set; }
    public List<IEffect> Effects { get; private set; }

    public Hit(float damage, HitType hitType, ElementType elementType, List<IEffect> effects)
    {
        Damage = damage;
        HitType = hitType;
        ElementType = elementType;
        Effects = effects;
    }
}
