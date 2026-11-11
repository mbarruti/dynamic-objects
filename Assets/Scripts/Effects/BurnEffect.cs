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
        ElementPulses.Emit(new ElementPulse
        {
            Element = ElementType.Fire,
            Position = target.transform.position,
            Radius = Def.PulseRadius,
            Amount = Def.PulseDamagePerSecond * deltaTime,
            SpreadPoints = Def.SpreadPointsPerSecond * deltaTime,
            Effect = Def,
            Source = target,
        });
        target.TakeDamage(Def.DamagePerSecond * deltaTime);
    }

    protected override void OnRemove(DynamicObjectBehaviour target)
    {
        Debug.Log($"{target.name} stopped burning");
    }
}
