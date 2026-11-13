using UnityEngine;

public class BurnEffect : TimedEffect<BurnEffectDefinition>
{
    public FireBudget Budget { get; set; }

    public BurnEffect(BurnEffectDefinition def) : base(def) {}

    protected override void OnApply(DynamicObjectBehaviour target)
    {
        Budget ??= new FireBudget();
        Debug.Log($"{target.name} is burning for {Def.Duration}s");
    }

    protected override void OnTick(DynamicObjectBehaviour target, float deltaTime)
    {
        Budget.Add(Def.PulseData.spreadPoints);

        ElementPulses.Emit(new ElementPulse
        {
            Type = Def.Element,
            Position = target.transform.position,
            Radius = Def.PulseData.radius,
            Amount = Def.PulseData.amount,
            Budget = Budget,
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
