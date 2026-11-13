using System.Collections.Generic;

public class HitEvaluation
{
    public float FinalDamage { get; }
    public List<EffectDefinition> Defs { get; }

    public HitEvaluation(float finalDamage, List<EffectDefinition> defs)
    {
        FinalDamage = finalDamage;
        Defs = defs;
    }
}
