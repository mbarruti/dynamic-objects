using System.Collections.Generic;

public class HitEvaluation
{
    public float FinalDamage { get; }
    public List<IEffect> Effects { get; }

    public HitEvaluation(float finalDamage, List<IEffect> effects)
    {
        FinalDamage = finalDamage;
        Effects = effects;
    }
}
