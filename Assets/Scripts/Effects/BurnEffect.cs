using UnityEngine;

public class BurnEffect : TimedEffect<BurnEffectDefinition>
{
    private float storedPoints;

    public BurnEffect(BurnEffectDefinition def) : base(def) {}

    public void AddPoints(float points) => storedPoints += Mathf.Max(0f, points);

    protected override void OnApply(DynamicObjectBehaviour target)
    {
        Debug.Log($"{target.name} is burning for {Def.Duration}s");
    }

    protected override void OnTick(DynamicObjectBehaviour target, float deltaTime)
    {
        // Debug.Log($"Spread points added from burn: {target.PulseData.spreadPoints}");

        float used = ElementPulses.Emit(new ElementPulse
        {
            Type = Def.Element,
            Position = target.ObjectOrigin,
            Radius = target.PulseData.radius,
            Amount = target.PulseData.amount,
            SpreadPoints = storedPoints + target.PulseData.spreadPoints,
            EffectDef = Def,
            Source = target,
        });
        target.TakeDamage(Def.DamagePerSecond * deltaTime);

        // Spend the stock first, so the box gets back to its own points per tick as soon as possible.
        // Its own points that go unused are lost; the stock only ever shrinks.
        storedPoints = Mathf.Max(0f, storedPoints - used);
    }

    protected override void OnRemove(DynamicObjectBehaviour target)
    {
        Debug.Log($"{target.name} stopped burning");
    }

    public override bool CanAffect(DynamicObject target) => Def.CanAffect(target);
}
