using UnityEngine;

[System.Serializable]
public class SurfaceSettings
{
    [Header("Grid")]
    [Min(0.1f)] public float cellSize = 1f;
    [Min(1)] public int maxCells = 4096;
    [Min(0.01f)] public float tickInterval = 0.1f;

    [Header("Fire")]
    [Tooltip("Hit points damaged when a cell is being ignited")]
    [Min(0.01f)] public float cellHitPoints = 10f;
    [Tooltip("Lifetime of a burning cell until burn out.")]
    [Min(0.01f)] public float burnLifetime = 5f;
    [Tooltip("Damage per second a burning cell deals to each neighbor cell to ignite it.")]
    [Min(0f)] public float igniteDamagePerSecond = 5f;
    [Tooltip("Spreading points spent to ignite one cell")]
    [Min(0f)] public float spreadCost = 8f;
    [Tooltip("Seconds without ignition before a cell gets its full hitpoints back.")]
    [Min(0f)] public float recoverDelay = 3f;
    [Tooltip("HP a damaged cell regains per second once its recover delay has passed.")]
    [Min(0f)] public float regenPerSecond = 2f;

    [Header("Projection")]
    [Tooltip("How far above the reference height the ray starts.")]
    [Min(0f)] public float probeUp = 2f;
    [Tooltip("How far below the reference height the ray reaches.")]
    [Min(0f)] public float probeDown = 4f;
    [Tooltip("Layers that can cover (disable) a cell, like rocks.")]
    public LayerMask blockerMask = ~0;
}
