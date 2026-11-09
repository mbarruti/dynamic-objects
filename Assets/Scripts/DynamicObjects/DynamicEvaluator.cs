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

    public static HitEvaluation Evaluate(Hit hit, DynamicObject data)
    {
        float damage = EvaluateDamage(hit, data);
        List<IEffect> effects = EvaluateEffects(hit.Effects, data);

        return new HitEvaluation(damage, effects);
    }

    private static float EvaluateDamage(Hit hit, DynamicObject data)
    {
        float multiplier = DamageMultipliers.TryGetValue((hit.HitType, data.Material), out float m) ? m : 1f;
        return hit.Damage * multiplier;
        // later: element modifiers (like fire vs wood)
    }

    public static List<IEffect> EvaluateEffects(List<IEffect> effects, DynamicObject data)
    {
        List<IEffect> result = new();
        if (effects == null) return result;

        foreach (IEffect effect in effects)
        {
            if (effect == null || !effect.CanAffect(data)) continue;
            // later: resistance check here
            result.Add(effect);
        }
        return result;
    }
}
