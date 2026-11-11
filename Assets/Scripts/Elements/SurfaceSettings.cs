using UnityEngine;

[System.Serializable]
public class SurfaceSettings
{
    [Header("Grid")]
    [Min(0.1f)] public float cellSize = 1f;
    [Min(1)] public int maxCells = 4096;
    [Min(0.01f)] public float tickInterval = 0.1f;

    [Header("Fire")]
    [Min(0.01f)] public float cellHitPoints = 10f;
    [Min(0.01f)] public float burnLifetime = 5f;

    [Tooltip("Damage per second a burning cell deals to each edge neighbour.")]
    [Min(0f)] public float burnDamagePerSecond = 5f;
    [Tooltip("Spreading points spent to ignite one cell")]
    [Min(0f)] public float spreadCost = 8f;

    [Header("Projection")]
    [Tooltip("How far above the reference height the ray starts.")]
    [Min(0f)] public float probeUp = 2f;
    [Tooltip("How far below the reference height the ray reaches.")]
    [Min(0f)] public float probeDown = 4f;
    [Tooltip("Layers that can cover (disable) a cell, like rocks.")]
    public LayerMask blockerMask = ~0;
}
