using System.Collections;
using UnityEngine;

public class BurnEffect : TimedEffect
{
    private const float TickInterval = 1f;

    // Shared data
    private readonly BurnEffectDefinition def;

    public ElementType Element { get; private set; }

    public BurnEffect(BurnEffectDefinition def) : base(def)
    {
        this.def = def;
    }

    protected override void OnApply(DynamicObjectBehaviour target)
    {
        Debug.Log($"{target.name} started burning for {def.Duration}s");
    }

    protected override void OnTick(DynamicObjectBehaviour target, float deltaTime)
    {
        target.TakeDamage(def.DamagePerSecond * deltaTime);
    }

    protected override void OnRemove(DynamicObjectBehaviour target)
    {
        Debug.Log($"{target.name} stopped burning");
    }
}
