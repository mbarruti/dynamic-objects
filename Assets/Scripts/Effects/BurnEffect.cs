using UnityEngine;

public class BurnEffect : TimedEffect<BurnEffectDefinition>
{
    // Shared data
    // private readonly BurnEffectDefinition def;

    public BurnEffect(BurnEffectDefinition def) : base(def) {}

    protected override void OnApply(DynamicObjectBehaviour target)
    {
        Debug.Log($"{target.name} started burning for {Def.Duration}s");
    }

    protected override void OnTick(DynamicObjectBehaviour target, float deltaTime)
    {
        target.TakeDamage(Def.DamagePerSecond * deltaTime);
    }

    protected override void OnRemove(DynamicObjectBehaviour target)
    {
        Debug.Log($"{target.name} stopped burning");
    }
}
