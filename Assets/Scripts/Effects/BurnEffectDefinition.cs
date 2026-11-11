using UnityEngine;

[System.Serializable]
public class BurnEffectDefinition : TimedEffectDefinition
{
    [SerializeField] private float damagePerSecond = 1f;

    [Header("Pulse")]
    [SerializeField] private bool hasPulse = true;
    [SerializeField] private float pulseRadius = 1f;
    [SerializeField] private float pulseDamagePerSecond = 5f;
    [SerializeField] private float spreadPointsPerSecond = 8f;

    public float DamagePerSecond => damagePerSecond;
    public override ElementType Element => ElementType.Fire;

    public bool HasPulse => hasPulse;

    public float PulseRadius => pulseRadius;

    public float PulseDamagePerSecond => pulseDamagePerSecond;

    public float SpreadPointsPerSecond => spreadPointsPerSecond;

    public override IEffect CreateInstance() => new BurnEffect(this);
}
