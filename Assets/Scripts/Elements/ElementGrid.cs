using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using static Constants;

// The one grid shared by every ElementSurface in the scene.
// Each cell is projected onto whichever surface is under it and reads its fire values from
// that surface's settings. Grid-wide values come from GridSettings.
public class ElementGrid
{
    private readonly GridSettings gridSettings;
    private readonly System.Random random;
    private readonly Dictionary<Vector2Int, ElementCell> cells = new();
    private readonly RaycastHit[] rayHits = new RaycastHit[16];
    private readonly Collider[] overlapHits = new Collider[16];
    private readonly List<ElementCell> spreadCells = new();
    private readonly List<ElementCell> burningCells = new();
    private readonly List<ElementCell> recoveringCells = new();
    private readonly List<ElementCell> scratch = new();
    private readonly List<ElementCell> burningNeighbors = new();
    private readonly List<ElementCell> hitCells = new();
    private readonly Queue<ElementCell> searchQueue = new();
    private readonly List<Bounds> coveredAreas = new();
    private int searchStamp;
    private int tickCount;

    // TO-DO: implement wind effect for fire spread
    private static readonly Vector2Int[] EdgeNeighbors =
    {
        Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down
    };

    public float SpreadRadius => gridSettings.cellSize * 1.5f;

    public IReadOnlyList<ElementCell> BurningCells => burningCells;

    public Dictionary<Vector2Int, ElementCell>.ValueCollection Cells => cells.Values;

    public ElementGrid(GridSettings gridSettings, System.Random random)
    {
        this.gridSettings = gridSettings;
        this.random = random;
    }

    // ---------------- Coordinates ----------------

    public Vector2Int WorldToCell(Vector3 gridSettingsPos) =>
        new(Mathf.FloorToInt(gridSettingsPos.x / gridSettings.cellSize),
            Mathf.FloorToInt(gridSettingsPos.z / gridSettings.cellSize));

    public Vector3 CellCentre(Vector2Int coord, float y) =>
        new((coord.x + 0.5f) * gridSettings.cellSize, y, (coord.y + 0.5f) * gridSettings.cellSize);

    public bool TryGetCell(Vector2Int coord, out ElementCell cell) =>
        cells.TryGetValue(coord, out cell);

    // Neighbours only touch if the step between them is small enough:
    // a wall top next to the ground isn't connected to it.
    private bool Connected(ElementCell a, ElementCell b) =>
        Mathf.Abs(a.Position.y - b.Position.y) <= gridSettings.maxStep;

    // ---------------- Building the grid ----------------

    public ElementCell GetOrCreateCell(Vector2Int coord, float referenceY)
    {
        if (cells.TryGetValue(coord, out ElementCell existing))
            return existing;

        if (cells.Count >= gridSettings.maxCells)
            return null;

        ElementCell cell = ProjectCell(coord, referenceY);
        cells.Add(coord, cell);
        return cell;
    }

    // Cells whose centre is within `radius` (flat) of `centre`, plus the closest one.
    // Skips cells too far above or below for the pulse to reach.
    public void GetCellsInRadius(Vector3 centre, float radius, List<ElementCell> results)
    {
        results.Clear();
        Vector2Int middle = WorldToCell(centre);
        int range = Mathf.CeilToInt(radius / gridSettings.cellSize);
        float reach = Mathf.Max(radius, gridSettings.maxStep);

        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                Vector2Int coord = new(middle.x + x, middle.y + z);
                bool isClosest = x == 0 && z == 0;

                if (!isClosest && FlatDistance(CellCentre(coord, 0f), centre) > radius)
                    continue;

                ElementCell cell = GetOrCreateCell(coord, centre.y);
                if (cell != null && Mathf.Abs(cell.Position.y - centre.y) <= reach)
                    results.Add(cell);
            }
        }
    }

    private ElementCell ProjectCell(Vector2Int coord, float referenceY)
    {
        Vector3 centre = CellCentre(coord, referenceY);
        Vector3 origin = centre + Vector3.up * gridSettings.probeUp;

        int count = Physics.RaycastNonAlloc(origin, Vector3.down, rayHits,
                                            gridSettings.probeUp + gridSettings.probeDown,
                                            Physics.DefaultRaycastLayers,
                                            QueryTriggerInteraction.Ignore);

        // Nearest hit that belongs to any surface
        int nearest = -1;
        ElementSurface surface = null;
        for (int i = 0; i < count; i++)
        {
            ElementSurface s = rayHits[i].collider.GetComponentInParent<ElementSurface>();
            if (s == null)
                continue;

            if (nearest < 0 || rayHits[i].distance < rayHits[nearest].distance)
            {
                nearest = i;
                surface = s;
            }
        }

        if (nearest < 0)
            return new ElementCell(coord, centre, Vector3.up, CellState.Disabled, null);

        RaycastHit hit = rayHits[nearest];
        SurfaceSettings settings = surface.Settings;
        CellState state = IsCovered(hit.point) ? CellState.Covered : CellState.Normal;

        return new ElementCell(coord, hit.point, hit.normal, state, settings)
        {
            Meter = new(settings.meterSettings),
            Lifetime = settings.burnLifetime,
        };
    }

    private bool IsCovered(Vector3 point)
    {
        float half = gridSettings.cellSize * 0.25f;
        Vector3 boxCentre = point + Vector3.up * 0.3f;
        Vector3 halfExtents = new(half, 0.25f, half);

        int count = Physics.OverlapBoxNonAlloc(boxCentre, halfExtents, overlapHits, Quaternion.identity,
                                               gridSettings.blockerMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            if (IsBlocker(overlapHits[i]))
                return true;
        }

        return false;
    }

    private static bool IsBlocker(Collider col)
    {
        if (col.GetComponentInParent<ElementSurface>() != null)
            return false;                                               // Ground, any surface.

        if (col.GetComponentInParent<CharacterController>() != null)
            return false;                                               // Player/NPCs move around.

        DynamicObjectBehaviour obj = col.GetComponentInParent<DynamicObjectBehaviour>();
        if (obj != null)
            return !obj.Data.Traits.flammable;                                 // Rock/metal block; wood burns on its own.

        return true;                                                    // Static scenery (walls, buildings).
    }

    // ---------------- Receiving pulses ----------------

    // Applies a pulse to the cells it covers. Returns the spread points it used.
    public float ApplyPulse(ElementPulse pulse)
    {
        GetCellsInRadius(pulse.Position, pulse.Radius, hitCells);

        int count = hitCells.Count;
        if (count == 0)
            return 0f;

        float points = pulse.SpreadPoints;

        // Random start so the same corner of the area doesn't always go first.
        int first = random.Next(count);

        // 1. Heat the cells. Ignitions use each cell's own points plus the pulse's (tools are free).
        for (int i = 0; i < count; i++)
            CellEvaluation.React(this, hitCells[(first + i) % count], pulse, ref points);

        // 2. Top up cells that are heating, never past what they need to ignite.
        for (int i = 0; i < count && points > 0f; i++)
        {
            ElementCell cell = hitCells[(first + i) % count];

            if (cell.State != CellState.Normal || !cell.IsRecovering)
                continue;

            float give = Mathf.Min(points, cell.Settings.spreadCost - cell.StoredPoints);
            if (give <= 0f)
                continue;

            cell.StoredPoints += give;
            points -= give;
        }

        // 3. The rest feeds the burning cells it reached (equal shares, so order doesn't matter).
        int burning = 0;
        foreach (ElementCell cell in hitCells)
            if (cell.State == CellState.Burning) burning++;

        if (burning > 0 && points > 0f)
        {
            float share = points / burning;
            foreach (ElementCell cell in hitCells)
                if (cell.State == CellState.Burning) cell.StoredPoints += share;
            points = 0f;
        }

        // Whatever is left wasn't used: the sender keeps it or loses it.
        return pulse.SpreadPoints - points;
    }

    // ---------------- State changes ----------------

    // The target cell's surface decides the cost and the fuel roll.
    public bool TryBurn(ElementCell cell, ref float points, bool free, ElementCell source = null)
    {
        if (cell.State != CellState.Normal || cell.Meter.Value > 0f)
            return false;

        SurfaceSettings s = cell.Settings;

        if (!free)
        {
            float heatPoints = Mathf.Min(cell.StoredPoints, s.spreadCost);
            float missing = s.spreadCost - heatPoints;

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
            cell.StoredPoints -= heatPoints;
        }

        cell.State = CellState.Burning;

        if (Roll(s.fuelChance))
            cell.StoredPoints += s.fuelPoints;

        burningCells.Add(cell);
        return true;
    }

    private void BurnOut(ElementCell cell)
    {
        cell.State = CellState.Burnt;
        burningCells.Remove(cell);

        // Leftover points go to burning neighbours.
        burningNeighbors.Clear();
        foreach (Vector2Int offset in EdgeNeighbors)
        {
            if (TryGetCell(cell.Coord + offset, out ElementCell n) &&
                n.State == CellState.Burning && Connected(cell, n))
                burningNeighbors.Add(n);
        }

        if (burningNeighbors.Count > 0)
        {
            float share = cell.StoredPoints / burningNeighbors.Count;
            foreach (ElementCell n in burningNeighbors)
                n.StoredPoints += share;
        }

        cell.StoredPoints = 0f;
    }

    private void SetCovered(ElementCell cell, bool covered)
    {
        switch (cell.State)
        {
            case CellState.Normal when covered:
                cell.State = CellState.Covered;
                cell.StoredPoints = 0f;                     // pre-payment lost under the rock
                break;

            case CellState.Burning when covered:
                BurnOut(cell);                              // smothered: leftovers go to burning neighbours
                break;

            case CellState.Covered when !covered:
                cell.State = CellState.Normal;
                cell.Meter = new(cell.Settings.meterSettings);
                cell.Lifetime = cell.Settings.burnLifetime;
                break;
        }
    }

    // ---------------- Simulation ----------------

    public void Tick(float dt)
    {
        UpdateCoverage();   
        UpdateBurning(dt);
        UpdateRecovering(dt);
    }

    private void UpdateCoverage()
    {
        if (coveredAreas.Count == 0 ) return;

        // An object moved by its transform (not by physics) could be read at its old position
        Physics.SyncTransforms();

        foreach (Bounds b in coveredAreas)
        {
            Bounds area = b;
            area.Expand(gridSettings.cellSize * 0.5f);

            Vector2Int min = WorldToCell(b.min);
            Vector2Int max = WorldToCell(b.max);

            for (int x = min.x; x <= max.x; x++)
            for (int z = min.y; z <= max.y; z++)
            {
                if (cells.TryGetValue(new(x, z), out ElementCell cell) && cell.Settings != null)
                    SetCovered(cell, IsCovered(cell.Position));
            }
        }
        coveredAreas.Clear();
    }

    private void UpdateBurning(float dt)
    {
        tickCount++;

        int count = burningCells.Count;
        if (count == 0)
            return;

        int first = random.Next(count);

        for (int i = 0; i < count; i++)
        {
            ElementCell cell = burningCells[(first + i) % count];

            Debug.Assert(cell.StoredPoints >= -EPSILON, $"Negative points at {cell.Coord}: {cell.StoredPoints}");

            BurnEffectDefinition burn = cell.Settings.objectEffect;
            // The burning cell's surface decides how hot it burns;
            // the neighbour's own meter decides when it ignites.
            float igniteDamage = cell.Settings.igniteDamagePerSecond * dt;

            GetCellsInRadius(cell.Position, SpreadRadius, spreadCells);
            int n = spreadCells.Count;
            int start = random.Next(n);
            for (int k = 0; k < n; k++)
            {
                ElementCell neighbor = spreadCells[(start + k) % n];

                if (neighbor != null && Connected(cell, neighbor))
                    CellEvaluation.ApplyIgnite(this, neighbor, igniteDamage, burn,
                                                ref cell.StoredPoints, source: cell);
            }

            cell.Lifetime -= dt;
            if (cell.Lifetime <= 0f)
                scratch.Add(cell);
        }

        foreach (ElementCell cell in scratch)
            BurnOut(cell);

        scratch.Clear();
    }

    private void UpdateRecovering(float dt)
    {
        for (int i = recoveringCells.Count - 1; i >= 0; i--)
        {
            ElementCell cell = recoveringCells[i];

            if (cell.State == CellState.Normal)
            {
                ElementMeterSettings meter = cell.Settings.meterSettings;

                float heatBefore = meter.value - cell.Meter.Value;
                bool recovered = cell.Meter.RecoveryTick(dt, meter);
                float heatAfter = meter.value - cell.Meter.Value;

                // Pre-paid points cool down with the meter.
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

    // Takes up to `needed` points from burning cells connected to `startCell`, nearest first.
    // Never takes from `startCell` itself. Returns what it took.
    private float GatherPoints(ElementCell startCell, float needed)
    {
        float gathered = 0f;
        int visitedCells = 0;

        searchStamp++;
        searchQueue.Clear();
        startCell.SearchStamp = searchStamp;
        searchQueue.Enqueue(startCell);

        while (searchQueue.Count > 0 && gathered < needed && visitedCells < gridSettings.maxSearchCells)
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
                if (TryGetCell(cell.Coord + offset, out ElementCell c) &&
                    c.State == CellState.Burning &&
                    c.SearchStamp != searchStamp &&
                    Connected(cell, c))
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
        if (!cell.IsRecovering)
        {
            cell.IsRecovering = true;
            recoveringCells.Add(cell);
        }
    }

    public void MarkCoverage(Bounds area) => coveredAreas.Add(area);

    // ---------------- Misc ----------------

    private static float FlatDistance(Vector3 a, Vector3 b) =>
        Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));

    private bool Roll(float chance) => chance > 0f && random.NextDouble() < chance;
}
