using UnityEngine;

[System.Serializable]
public class SurfaceSettings
{
    [Header("Grid")]
    [Min(0.1f)] public float cellSize = 1f;
    [Min(1)] public int maxCells = 4096;
    [Min(0.01f)] public float tickInterval = 0.1f;

    [Header("Element")]
    [Tooltip("Lifetime of a burning cell until burn out.")]
    [Min(0.01f)] public float burnLifetime = 5f;
    [Tooltip("Damage per second a burning cell deals to each neighbor cell to ignite it.")]
    [Min(0f)] public float igniteDamagePerSecond = 5f;
    [Tooltip("Spreading points spent to ignite one cell")]
    [Min(0f)] public float spreadCost = 8f;
    [Tooltip("Meter threshold before an element takes effect")]
    public ElementMeterSettings meterSettings = new();

    [Header("Fire Pulses")]
    [Tooltip("Interval for element emission")]
    [Min(0.05f)] public float pulseInterval = 0.5f;
    [Tooltip("Radius for element emission")]
    [Min(0f)] public float pulseRadius = 1f;
    [Tooltip("Burn given to flammable objects that hear a burning cell.")]
    public BurnEffectDefinition objectEffect = new();

    [Header("Projection")]
    [Tooltip("How far above the reference height the ray starts.")]
    [Min(0f)] public float probeUp = 2f;
    [Tooltip("How far below the reference height the ray reaches.")]
    [Min(0f)] public float probeDown = 4f;
    [Tooltip("Layers that can cover (disable) a cell, like rocks.")]
    public LayerMask blockerMask = ~0;
}
