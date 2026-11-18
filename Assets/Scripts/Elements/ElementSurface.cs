using System.Collections.Generic;
using UnityEngine;

// Entity/Surface that reacts to elements
public class ElementSurface : MonoBehaviour, IElementReceiver, IElementEmitter
{
    [SerializeField] private SurfaceSettings settings = new();
    [SerializeField] private PulseSettings pulseData = new();

    [Header("Dev")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private bool drawLifetime = false;

    private ElementGrid grid;
    private readonly List<ElementCell> hitCells = new();

    private float tickTimer;
    private float pulseTimer;

    public PulseSettings PulseData => pulseData;
    public ElementGrid Grid => grid;

    private void Update()
    {
        if (grid == null)
            return;

        tickTimer += Time.deltaTime;
        while (tickTimer >= settings.tickInterval)
        {
            tickTimer -= settings.tickInterval;
            grid.Tick(settings.tickInterval);
        }

        pulseTimer += Time.deltaTime;
        if (pulseTimer >= settings.pulseInterval)
        {
            pulseTimer -= settings.pulseInterval;
            EmitPulses();
        }
    }

    public bool Owns(Collider col) =>
        col != null && col.GetComponentInParent<ElementSurface>() == this;

    private ElementGrid GetOrCreateGrid()
    {
        grid ??= new ElementGrid(this, settings,
                                settings.useSeed ? new System.Random(settings.seed)
                                : new System.Random());
        return grid;
    }

    // ---------------- Receiving ----------------

    public float ReceivePulse(ElementPulse pulse)
    {
        ElementGrid g = GetOrCreateGrid();
        g.GetCellsInRadius(pulse.Position, pulse.Radius, hitCells);

        int count = hitCells.Count;
        if (count == 0)
            return 0f;

        float points = pulse.SpreadPoints;

        // Random start so the same corner of the area doesn't always go first.
        int first = g.RandomIndex(count);

        // 1. Heat the cells. Ignitions use each cell's own points plus the pulse's (tools are free).
        for (int i = 0; i < count; i++)
            CellEvaluation.React(g, hitCells[(first + i) % count], pulse, ref points);

        // 2. Top up cells that are heating, never past what they need to ignite.
        for (int i = 0; i < count && points > 0f; i++)
        {
            ElementCell cell = hitCells[(first + i) % count];

            if (cell.State != CellState.Normal || !cell.IsRecovering)
                continue;

            float give = Mathf.Min(points, g.Settings.spreadCost - cell.StoredPoints);
            if (give <= 0f)
                continue;

            cell.StoredPoints += give;
            points -= give;
        }

        // 3. The rest feeds the burning cells it reached.
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

        // Whatever is left wasn't used: the sender keeps it (cell pulses) or loses it (box, tools).
        return pulse.SpreadPoints - points;
    }


    // ---------------- Sending ----------------

    public void EmitPulses()
    {
        // IReadOnlyList<ElementCell> burningCells = grid.BurningCells;

        foreach (ElementCell cell in grid.BurningCells)
        {
            float offer = cell.StoredPoints * settings.pulseShare;

            float used = ElementPulses.Emit(new ElementPulse
            {
                Type = ElementType.Fire,
                Position = cell.Position,
                Radius = settings.pulseRadius,
                Amount = pulseData.amount,
                SpreadPoints = offer,
                EffectDef = settings.objectEffect,
                Source = this
            });

            cell.StoredPoints -= used;
        }
    }

    // Dev

    private void OnDrawGizmos()
    {
        if (!drawGizmos || grid == null)
            return;

        float size = settings.cellSize * 0.95f;

        foreach (ElementCell cell in grid.Cells)
        {
            Gizmos.color = CellColor(cell);
            Vector3 p = cell.Position + Vector3.up * 0.02f;

            if (cell.State == CellState.Burning || cell.State == CellState.Burnt)
                // Gizmos.DrawCube(p, new Vector3(size, 0.02f, size));
            {
                float k = cell.Lifetime / settings.burnLifetime;          // 1 → 0
                Gizmos.DrawCube(p, new Vector3(size * k, 0.02f, size * k));
            }
            else
                Gizmos.DrawWireCube(p, new Vector3(size, 0f, size));

            #if UNITY_EDITOR
                if (drawLifetime && cell.State == CellState.Burning)
                    UnityEditor.Handles.Label(p + Vector3.up * 0.1f, cell.Lifetime.ToString("0.0"));
            #endif

            // if (cell.State != CellState.Disabled)
            // {
            //     Gizmos.color = Color.yellow;
            //     Gizmos.DrawLine(cell.Position, cell.Position + cell.Normal * 0.3f);
            // }
        }
    }

    private Color CellColor(ElementCell cell)
    {
        switch (cell.State)
        {
            case CellState.Disabled: return new Color(0.8f, 0.2f, 0.2f);
            case CellState.Burning: return new Color(1f, 0.4f, 0f);
            case CellState.Burnt: return new Color(0.15f, 0.15f, 0.15f);
        }

        // Green when healthy, yellow as fire takes its hitpoints.
        return Color.Lerp(Color.yellow, Color.green, cell.Meter.Value / settings.meterSettings.value);
    }
}