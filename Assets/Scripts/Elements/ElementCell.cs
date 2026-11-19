using UnityEngine;

public enum CellState
{
    Normal,
    Burning,
    Burnt,
    Covered,
    Disabled    // No ground.
}

// A single cell from the entity's surface
public class ElementCell
{
    public readonly Vector2Int Coord;
    public readonly Vector3 Position;
    public readonly Vector3 Normal;
    public readonly SurfaceSettings Settings;

    public CellState State;
    public ElementMeter Meter;
    public float Lifetime;
    public bool IsRecovering = false;
    public float StoredPoints;
    public int SearchStamp; // last gather search that visited this cell
    public int LastFailedGather;   // tick in which this cell's fire last came up short

    public ElementCell(Vector2Int coord, Vector3 position, Vector3 normal, CellState state,
                        SurfaceSettings settings)
    {
        Coord = coord;
        Position = position;
        Normal = normal;
        State = state;
        Settings = settings;
    }
}