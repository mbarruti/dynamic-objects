using UnityEngine;

[System.Serializable]
public class BurnEffectDefinition : TimedEffectDefinition
{
    [SerializeField] private float damagePerSecond = 1f;

    public float DamagePerSecond => damagePerSecond;
    public override ElementType Element => ElementType.Fire;
    public override IEffect CreateInstance() => new BurnEffect(this);
}
