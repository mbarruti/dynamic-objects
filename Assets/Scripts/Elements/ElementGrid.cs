using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static Constants;

// ElementGrid stores the cells of one surface, 
// creates them by projecting them onto the world 
// and runs the rules that change them over time, using that surface's settings.
public class ElementGrid
{
    private readonly ElementSurface owner;
    private readonly SurfaceSettings settings;
    private readonly System.Random random;
    private readonly Dictionary<Vector2Int, ElementCell> cells = new();
    private readonly RaycastHit[] rayHits = new RaycastHit[16];
    private readonly Collider[] overlapHits = new Collider[16];
    private readonly List<ElementCell> burningCells = new();
    private readonly List<ElementCell> scratch = new();
    private readonly List<ElementCell> burningNeighbors = new();
    private readonly List<ElementCell> recoveringCells = new();
    // TO-DO: implement wind effect for fire spread
    private static readonly Vector2Int[] EdgeNeighbors =
    {
        Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down
    };
    private readonly Queue<ElementCell> searchQueue = new();
    private int searchStamp;
    private int tickCount;

    public SurfaceSettings Settings => settings;

    public IReadOnlyList<ElementCell> BurningCells => burningCells;

    public Dictionary<Vector2Int, ElementCell>.ValueCollection Cells => cells.Values;

    public ElementGrid(ElementSurface owner, SurfaceSettings settings, System.Random random)
    {
        this.owner = owner;
        this.settings = settings;
        this.random = random;
    }

    // Coordinates

    public Vector2Int WorldToCell(Vector3 worldPos) =>
        new(Mathf.FloorToInt(worldPos.x / settings.cellSize),
            Mathf.FloorToInt(worldPos.z / settings.cellSize));

    public Vector3 CellCentre(Vector2Int coord, float y) =>
        new((coord.x + 0.5f) * settings.cellSize, y, (coord.y + 0.5f) * settings.cellSize);

    public bool TryGetCell(Vector2Int coord, out ElementCell cell) =>
        cells.TryGetValue(coord, out cell);

    // Build grid

    public ElementCell GetOrCreateCell(Vector2Int coord, float referenceY)
    {
        if (cells.TryGetValue(coord, out ElementCell existing))
            return existing;

        if (cells.Count >= settings.maxCells)
            return null;

        ElementCell cell = ProjectCell(coord, referenceY);
        cells.Add(coord, cell);
        return cell;
    }

    public void GetCellsInRadius(Vector3 centre, float radius, List<ElementCell> results)
    {
        results.Clear();
        Vector2Int middle = WorldToCell(centre);
        int range = Mathf.CeilToInt(radius / settings.cellSize);

        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                Vector2Int coord = new(middle.x + x, middle.y + z);
                bool isClosest = x == 0 && z == 0;

                if (!isClosest && FlatDistance(CellCentre(coord, 0f), centre) > radius)
                    continue;

                ElementCell cell = GetOrCreateCell(coord, centre.y);
                if (cell != null)
                    results.Add(cell);
            }
        }
    }

    private ElementCell ProjectCell(Vector2Int coord, float referenceY)
    {
        Vector3 center = CellCentre(coord, referenceY);
        Vector3 origin = center + Vector3.up * settings.probeUp;

        int count = Physics.RaycastNonAlloc(origin, Vector3.down, rayHits,
                                            settings.probeUp + settings.probeDown,
                                            Physics.DefaultRaycastLayers,
                                            QueryTriggerInteraction.Ignore);
        
        int nearest = -1;
        for (int i = 0; i < count; i++)
        {
            if (!owner.Owns(rayHits[i].collider))
                continue;
            
            if (nearest < 0 || rayHits[i].distance < rayHits[nearest].distance)
                nearest = i;
        }

        if (nearest < 0)
            return new ElementCell(coord, center, Vector3.up, CellState.Disabled);

        RaycastHit hit = rayHits[nearest];
        CellState state = IsCovered(hit.point) ? CellState.Disabled : CellState.Normal;

        return new ElementCell(coord, hit.point, hit.normal, state)
        {
            Meter = new(settings.meterSettings),
            Lifetime = settings.burnLifetime,
        };
    }

    private bool IsCovered(Vector3 point)
    {
        float half = settings.cellSize * 0.25f;
        Vector3 boxCentre = point + Vector3.up * 0.3f;
        Vector3 halfExtents = new(half, 0.25f, half);

        int count = Physics.OverlapBoxNonAlloc(boxCentre, halfExtents, overlapHits, Quaternion.identity,
                                               settings.blockerMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            if (IsBlocker(overlapHits[i]))
                return true;
        }

        return false;
    }

    private bool IsBlocker(Collider col)
    {
        if (owner.Owns(col))
            return false;                                               // The ground itself.

        if (col.GetComponentInParent<CharacterController>() != null)
            return false;                                               // Player/NPCs move around.

        DynamicObjectBehaviour obj = col.GetComponentInParent<DynamicObjectBehaviour>();
        if (obj != null)
            return !obj.Data.Flammable;                                 // Rock/metal block; wood burns on its own.

        return true;                                                    // Static scenery (walls, buildings).
    }

    // State changes

    public bool TryBurn(ElementCell cell, ref float points, bool free, ElementCell source = null)
    {
        if (cell.State != CellState.Normal || cell.Meter.Value > 0f)
            return false;
        
        if (!free)
        {
            float heatPoints = Mathf.Min(cell.StoredPoints, settings.spreadCost);
            float missing = settings.spreadCost - heatPoints;

            bool shortOfPoints = points < missing - EPSILON;

            if (shortOfPoints && source != null && source.LastFailedGather != tickCount)
            {
                points += GatherPoints(source, missing - points);

                if (points < missing - EPSILON)
                    source.LastFailedGather = tickCount;
            }

            if (points < missing - EPSILON)
                return false;

            points = Mathf.Max(0f, points - missing);
            cell.StoredPoints -= missing;
        }

        cell.State = CellState.Burning;

        if (Roll(settings.fuelChance))
            cell.StoredPoints += settings.fuelPoints;

        burningCells.Add(cell);
        return true;
    }

    private void BurnOut(ElementCell cell)
    {
        cell.State = CellState.Burnt;
        burningCells.Remove(cell);

        // Leftover points go to burning neighbors
        // TO-DO: maybe leftover points vanish if not used before burnout
        burningNeighbors.Clear();
        foreach(Vector2Int offset in EdgeNeighbors)
        {
            if (TryGetCell(cell.Coord + offset, out ElementCell n)
                && n.State == CellState.Burning)
                burningNeighbors.Add(n);
        }

        if (burningNeighbors.Count > 0)
        {
            float share = cell.StoredPoints / burningNeighbors.Count;
            foreach (ElementCell n in burningNeighbors)
                n.StoredPoints += share;
        }
        
        cell.StoredPoints = 0;
    }

    // Simulation

    public void Tick(float dt)
    {
        UpdateBurning(dt);
        UpdateRecovering(dt);
    }

    private void UpdateBurning(float dt)
    {
        tickCount++;

        int count = burningCells.Count;
        if (count == 0) return;

        float igniteDamage = settings.igniteDamagePerSecond * dt;

        int first = random.Next(count);

        for (int i = 0; i < count; i++)
        {
            ElementCell cell = burningCells[(first + i) % count];

            int start = random.Next(EdgeNeighbors.Length);
            for (int k = 0; k < EdgeNeighbors.Length; k++)
            {
                Vector2Int offset = EdgeNeighbors[(start + k) % EdgeNeighbors.Length];

                ElementCell neighbor = GetOrCreateCell(cell.Coord + offset, cell.Position.y);
                if (neighbor != null)
                    CellEvaluation.ApplyIgnite(this, neighbor, igniteDamage, ref cell.StoredPoints, source: cell);
            }

            cell.Lifetime -= dt;
            if (cell.Lifetime <= 0f)
                scratch.Add(cell);
        }

        foreach (ElementCell cell in scratch)
            BurnOut(cell);
        
        scratch.Clear();
    }

    // Takes up to `needed` points from burning cells connected to `start`, nearest first.
    // Never takes from `start` itself. Returns what it took.
    private float GatherPoints(ElementCell startCell, float needed)
    {
        float gathered = 0f;
        int visitedCells = 0;

        searchStamp++;
        searchQueue.Clear();
        startCell.SearchStamp = searchStamp;
        searchQueue.Enqueue(startCell);

        while (searchQueue.Count > 0 && gathered < needed && visitedCells < settings.maxSearchCells)
        {
            ElementCell cell = searchQueue.Dequeue();
            visitedCells++;

            if (cell != startCell)
            {
                float taken = Mathf.Min(cell.StoredPoints, needed - gathered);
                cell.StoredPoints -= taken;
                gathered += taken;
            }

            int start = random.Next(EdgeNeighbors.Length);
            for (int k = 0; k < EdgeNeighbors.Length; k++)
            {
                Vector2Int offset = EdgeNeighbors[(start + k) % EdgeNeighbors.Length];
                if (TryGetCell(cell.Coord + offset, out ElementCell c)
                    && c.State == CellState.Burning
                    && c.SearchStamp != searchStamp)
                {
                    c.SearchStamp = searchStamp;
                    searchQueue.Enqueue(c);
                }
            }
        }

        return gathered;
    }
    
    public void MarkDamaged(ElementCell cell)
    {
        // cell.RecoverTimer = settings.recoverDelay;

        if (!cell.IsRecovering)
        {
            cell.IsRecovering = true;
            recoveringCells.Add(cell);
        }
    }

    private void UpdateRecovering(float dt)
    {
        float max = settings.meterSettings.value;

        for (int i = recoveringCells.Count - 1; i >= 0; i--)
        {
            ElementCell cell = recoveringCells[i];

            if (cell.State == CellState.Normal)
            {
                float heatBefore = max - cell.Meter.Value;
                bool recovered = cell.Meter.RecoveryTick(dt, settings.meterSettings);
                float heatAfter = max - cell.Meter.Value;

                // Stored spread points are reduced relative to the meter
                cell.StoredPoints = heatBefore > 0f
                    ? cell.StoredPoints * (heatAfter / heatBefore)
                    : 0f;
                
                if (!recovered)
                    continue;
            }

            cell.IsRecovering = false;
            recoveringCells.RemoveAt(i);
        }
    }

    // Misc

    private static float FlatDistance(Vector3 a, Vector3 b) =>
        Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));

    private bool Roll(float chance) => chance > 0f && random.NextDouble() < chance;

    // Random index from this grid's seeded generator.
    public int RandomIndex(int count) => random.Next(count);
}