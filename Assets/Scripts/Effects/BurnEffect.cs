using UnityEngine;

public class BurnEffect : TimedEffect<BurnEffectDefinition>
{
    public BurnEffect(BurnEffectDefinition def) : base(def) {}

    protected override void OnApply(DynamicObjectBehaviour target)
    {
        Debug.Log($"{target.name} is burning for {Def.Duration}s");
    }

    protected override void OnTick(DynamicObjectBehaviour target, float deltaTime)
    {
        // Debug.Log($"Spread points added from burn: {target.PulseData.spreadPoints}");

        ElementPulses.Emit(new ElementPulse
        {
            Type = Def.Element,
            Position = target.ObjectOrigin,
            Radius = target.PulseData.radius,
            Amount = target.PulseData.amount,
            SpreadPoints = target.PulseData.spreadPoints,
            EffectDef = Def,
            Source = target,
        });
        target.TakeDamage(Def.DamagePerSecond * deltaTime);
    }

    protected override void OnRemove(DynamicObjectBehaviour target)
    {
        Debug.Log($"{target.name} stopped burning");
    }

    public override bool CanAffect(DynamicObject target) => Def.CanAffect(target);
}
