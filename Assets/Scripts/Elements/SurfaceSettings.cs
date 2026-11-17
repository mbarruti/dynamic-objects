using UnityEngine;

[System.Serializable]
public class SurfaceSettings
{
    [Header("Grid")]
    [Min(0.1f)] public float cellSize = 1f;
    [Min(1)] public int maxCells = 4096;
    [Min(0.01f)] public float tickInterval = 0.1f;
    [Min(0)] public int maxSearchCells = 32;

    [Header("Fire")]
    [Tooltip("Lifetime of a burning cell until burn out.")]
    [Min(0.01f)] public float burnLifetime = 5f;
    [Tooltip("Damage per second a burning cell deals to each neighbor cell to ignite it.")]
    [Min(0f)] public float igniteDamagePerSecond = 5f;
    [Tooltip("How fast touching burning cells even out their points, per second. 0 = never.")]
    [Min(0f)] public float diffusion = 2f;
    [Tooltip("Spreading points spent to ignite one cell")]
    [Min(0f)] public float spreadCost = 8f;
    [Tooltip("Meter threshold before an element takes effect")]
    public ElementMeterSettings meterSettings = new();

    [Header("Fire Pulses")]
    [Tooltip("Interval for element emission")]
    [Min(0.05f)] public float pulseInterval = 0.5f;
    [Tooltip("Radius for element emission")]
    [Min(0f)] public float pulseRadius = 1f;
    [Tooltip("Share of a burning cell's points each pulse offers to other surfaces.")]
    [Range(0f, 1f)] public float pulseShare = 0.25f;
    [Tooltip("Burn given to flammable objects that hear a burning cell.")]
    public BurnEffectDefinition objectEffect = new();

    [Header("Fire Fuel")]
    [Tooltip("Chance that a cell adds spread points to the fire when it ignites.")]
    [Range(0f, 1f)] public float fuelChance = 0f;
    [Tooltip("Spread points added when the fuel roll succeeds.")]
    [Min(0f)] public float fuelPoints = 0f;
    [Tooltip("Fixed seed so the same actions produce the same fire.")]
    [SerializeField] public bool useSeed = true;
    [SerializeField] public int seed = 0;

    [Header("Projection")]
    [Tooltip("How far above the reference height the ray starts.")]
    [Min(0f)] public float probeUp = 2f;
    [Tooltip("How far below the reference height the ray reaches.")]
    [Min(0f)] public float probeDown = 4f;
    [Tooltip("Layers that can cover (disable) a cell, like rocks.")]
    public LayerMask blockerMask = ~0;
}
