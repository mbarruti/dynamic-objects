using System.Collections.Generic;
using UnityEngine;

// Entity/Surface that reacts to elements
public class ElementSurface : MonoBehaviour
{
    [SerializeField] private SurfaceSettings settings = new();
    [SerializeField] private bool drawGizmos = true;
    private ElementGrid grid;

    private readonly List<ElementCell> hitCells = new();

    private float tickTimer;

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
    }

    public bool Owns(Collider col) =>
        col != null && col.GetComponentInParent<ElementSurface>() == this;

    private ElementGrid GetOrCreateGrid()
    {
        grid ??= new ElementGrid(this, settings);
        return grid;
    }

    // public void DebugBuildArea(Vector3 point, float radius)
    // {
    //     GetOrCreateGrid().GetCellsInRadius(point, radius, hitCells);
    //     Debug.Log($"{name}: {grid.Cells.Count} cells");
    // }

    public void DebugDamage(Vector3 point, float damage)
    {
        ElementGrid g = GetOrCreateGrid();
        g.GetCellsInRadius(point, 0f, hitCells);

        foreach (ElementCell cell in hitCells)
        {
            CellEvaluation.ApplyFire(g, cell, damage);
        }
    }

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
                Gizmos.DrawCube(p, new Vector3(size, 0.02f, size));
            else
                Gizmos.DrawWireCube(p, new Vector3(size, 0f, size));

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
        return Color.Lerp(Color.yellow, Color.green, cell.HitPoints / settings.cellHitPoints);
    }


}