using UnityEngine;

// Values shared by the whole grid.
[System.Serializable]
public class GridSettings
{
    [Header("Grid")]
    [Min(0.1f)] public float cellSize = 1f;
    [Min(1)] public int maxCells = 4096;
    [Min(0.01f)] public float tickInterval = 0.1f;
    [Tooltip("Highest step between two neighbouring cells that still counts as touching. " +
             "Equal to cellSize allows 45° slopes.")]
    [Min(0f)] public float maxStep = 1f;

    [Header("Fire")]
    [Tooltip("How many connected burning cells a cell can search for points when it can't afford an ignition.")]
    [Min(1)] public int maxSearchCells = 32;
    [Tooltip("Seconds between the pulses burning cells send to objects.")]
    [Min(0.05f)] public float pulseInterval = 0.5f;

    [Header("Projection")]
    [Tooltip("How far above the reference height the ray starts.")]
    [Min(0f)] public float probeUp = 2f;
    [Tooltip("How far below the reference height the ray reaches.")]
    [Min(0f)] public float probeDown = 4f;
    [Tooltip("Layers that can cover (disable) a cell.")]
    public LayerMask blockerMask = ~0;

    [Header("Randomness")]
    [Tooltip("Fixed seed so the same actions produce the same fire.")]
    public bool useSeed = false;
    public int seed = 0;
}

