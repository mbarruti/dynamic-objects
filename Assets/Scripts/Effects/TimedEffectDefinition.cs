using UnityEngine;

[System.Serializable]
public abstract class TimedEffectDefinition : EffectDefinition
{
    // Base values
    [SerializeField] private float duration = 10f;
    [SerializeField] private float tickInterval = 1f;

    public float Duration => duration;

    // Clamped so a 0 or negative value in the Inspector can't stall the timer loop.
    public float TickInterval => Mathf.Max(0.01f, tickInterval);
}
