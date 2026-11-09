using UnityEngine;

public class BurnEffect : TimedEffect<BurnEffectDefinition>
{
    public BurnEffect(BurnEffectDefinition def) : base(def) {}

    public override bool CanAffect(DynamicObject target) => target.Flammable;

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
