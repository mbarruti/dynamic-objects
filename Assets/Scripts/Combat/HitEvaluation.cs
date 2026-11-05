public class HitEvaluation
{
    public float FinalDamage { get; }
    public bool EffectResistance { get; }

    public HitEvaluation(float finalDamage, bool effectResistance)
    {
        FinalDamage = finalDamage;
        EffectResistance = effectResistance;
    }
}