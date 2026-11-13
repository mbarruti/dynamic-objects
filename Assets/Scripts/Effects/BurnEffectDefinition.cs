using UnityEngine;

[System.Serializable]
public class BurnEffectDefinition : TimedEffectDefinition, IElementEmitter
{
    [SerializeField] private float damagePerSecond = 1f;
    [SerializeField] private PulseSettings pulseData = new();

    public float DamagePerSecond => damagePerSecond;
    public PulseSettings PulseData => pulseData;

    public override ElementType Element => ElementType.Fire;
    public override IEffect CreateInstance() => new BurnEffect(this);
    public override bool CanAffect(DynamicObject target) => target.Flammable;
}
