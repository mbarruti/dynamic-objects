using System.Collections.Generic;
using UnityEngine;

// The scene's one element grid. Every ElementSurface is part of it.
public class ElementGridManager : MonoBehaviour, IElementReceiver
{
    [SerializeField] private GridSettings settings = new();

    [Header("Dev")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private bool drawSpreadPoints = false;
    public bool DrawSpreadPoints => drawSpreadPoints;

    private ElementGrid grid;
    private float tickTimer;
    private float pulseTimer;

    // Static, so with domain reload off it must be cleared in OnDisable.
    public static ElementGridManager Instance { get; private set; }

    public ElementGrid Grid => grid;

    private void OnEnable()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Two ElementWorlds in the scene ({Instance.name}, {name}). Only the first is used.");
            return;
        }

        Instance = this;
        grid ??= new ElementGrid(settings,
                                 settings.useSeed ? new System.Random(settings.seed) : new System.Random());
    }

    private void OnDisable()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (Instance != this)
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

    public float ReceivePulse(ElementPulse pulse) => grid.ApplyPulse(pulse);

    public bool AcceptsEffect(EffectDefinition def, Collider hit)
    {
        ElementSurface surface = hit.GetComponentInParent<ElementSurface>();
        return surface != null && def.CanAffect(surface.Settings.traits);
    }


    // Burning cells only need to reach objects: every surface is already this grid.
    // Objects don't take points from fire, so these pulses carry none.
    private void EmitPulses()
    {
        IReadOnlyList<ElementCell> burning = grid.BurningCells;

        for (int i = 0; i < burning.Count; i++)
        {
            ElementCell cell = burning[i];
            SurfaceSettings s = cell.Settings;

            ElementPulses.Emit(new ElementPulse
            {
                Type = ElementType.Fire,
                Position = cell.Position,
                Radius = s.objectEffect.PulseData.radius,
                Amount = s.objectEffect.PulseData.amount,
                EffectDef = s.objectEffect,
                Source = this,          // the grid never hears its own pulses
            });
        }
    }

    // ---------------- Dev ----------------

    private void OnDrawGizmos()
    {
        if (!drawGizmos || grid == null)
            return;

        float size = settings.cellSize * 0.95f;

        foreach (ElementCell cell in grid.Cells)
        {
            Gizmos.color = CellColor(cell);
            Vector3 p = cell.Position + Vector3.up * 0.02f;

            switch (cell.State)
            {
                case CellState.Burning:
                {
                    float k = cell.Lifetime / cell.Settings.burnLifetime;      // 1 → 0
                    Gizmos.DrawCube(p, new Vector3(size * k, 0.02f, size * k));
                    break;
                }
                case CellState.Burnt:
                    Gizmos.DrawCube(p, new Vector3(size, 0.02f, size));
                    break;
                default:
                    Gizmos.DrawWireCube(p, new Vector3(size, 0f, size));
                    break;
            }

#if UNITY_EDITOR
            if (drawSpreadPoints && cell.StoredPoints > 0f)
                UnityEditor.Handles.Label(p + Vector3.up * 0.1f, cell.StoredPoints.ToString("0.0"));
#endif
        }
    }

    private static Color CellColor(ElementCell cell)
    {
        switch (cell.State)
        {
            case CellState.Disabled: return new Color(0.8f, 0.2f, 0.2f);
            case CellState.Burning: return new Color(1f, 0.4f, 0f);
            case CellState.Burnt: return new Color(0.15f, 0.15f, 0.15f);
        }

        // Green when cool, yellow as it heats up.
        return Color.Lerp(Color.yellow, Color.green, cell.Meter.Value / cell.Settings.meterSettings.value);
    }
}
