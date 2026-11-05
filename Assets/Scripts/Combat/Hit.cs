public class Hit
{
    public float Damage { get; private set; }
    public HitType HitType { get; private set; }

    public Hit(float damage, HitType hitType)
    {
        Damage = damage;
        HitType = hitType;
    }
}
