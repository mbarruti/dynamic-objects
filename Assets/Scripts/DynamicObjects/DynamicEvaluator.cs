using System.Collections.Generic;
using UnityEngine;

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

    // Meter

    public static void React(DynamicObjectBehaviour target, ElementPulse pulse)
    {
        switch (pulse.Type)
        {
            case ElementType.Fire:
                ApplyIgnite(target, pulse);
                break;
        }
    }

    public static void ApplyIgnite(DynamicObjectBehaviour target, ElementPulse pulse)
    {
        if (pulse.EffectDef == null || !pulse.EffectDef.CanAffect(target.Data))
            return;

        if (target.DamageMeter(pulse.Amount))
        {
            target.AddOrRefreshEffect(pulse.EffectDef);

            if (target.TryGetEffect(out BurnEffect burning))
            {
                if (pulse.StartsFire)
                    Debug.Log($"[{target.name}] tool pulse joined its burn, fire now has {burning.Budget.SpreadPoints:0.#} pts");

                burning.Budget = FireBudget.Join(burning.Budget, pulse.Budget);
                // burning.Budget.Add(pulse.SpreadPoints);
            }
        }
    }
}
