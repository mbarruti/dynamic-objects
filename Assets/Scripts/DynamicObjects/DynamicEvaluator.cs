using System.Collections.Generic;

public static class DynamicEvaluator
{
    // Damage multiplier per (hit type, material). Missing pair = 1 (full damage).
    private static readonly Dictionary<(HitType, ObjectMaterial), float> DamageMultipliers = new()
    {
        { (HitType.Slash,  ObjectMaterial.Wood),  0f   },
        { (HitType.Slash,  ObjectMaterial.Stone), 0f   },
        { (HitType.Slash,  ObjectMaterial.Metal), 0f   },

        { (HitType.Pierce, ObjectMaterial.Wood),  0f   },
        { (HitType.Pierce, ObjectMaterial.Stone), 0f   },
        { (HitType.Pierce, ObjectMaterial.Metal), 0f   },

        { (HitType.Blunt,  ObjectMaterial.Wood),  1f   },
        { (HitType.Blunt,  ObjectMaterial.Stone), 1f   },
        { (HitType.Blunt,  ObjectMaterial.Metal), 0.5f },
    };

    public static HitEvaluation EvaluateHit(Hit hit, DynamicObject data)
    {
        float damage = EvaluateDamage(hit, data);
        List<EffectDefinition> defs = EvaluateEffects(hit.EffectDefs, data);

        return new HitEvaluation(damage, defs);
    }

    private static float EvaluateDamage(Hit hit, DynamicObject data)
    {
        float multiplier = DamageMultipliers.TryGetValue((hit.HitType, data.Material), out float m) ? m : 1f;
        return hit.Damage * multiplier;
        // later: element modifiers (like fire vs wood)
    }

    public static List<EffectDefinition> EvaluateEffects(List<EffectDefinition> defs, DynamicObject data)
    {
        List<EffectDefinition> result = new();
        if (defs == null) return result;

        foreach (EffectDefinition def in defs)
        {
            if (def == null || !def.CanAffect(data)) continue;
            // TO-DO: resistance check here
            result.Add(def);
        }
        return result;
    }
}
