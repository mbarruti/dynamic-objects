using System.Collections.Generic;
using UnityEngine;

// Entity/Surface that reacts to elements
public class ElementSurface : MonoBehaviour
{
    [SerializeField] private SurfaceSettings settings = new();
    [SerializeField] private bool drawGizmos = true;
    private ElementGrid grid;

    private readonly List<ElementCell> hitCells = new();

    public bool Owns(Collider col) =>
        col != null && col.GetComponentInParent<ElementSurface>() == this;

    private ElementGrid GetOrCreateGrid()
    {
        grid ??= new ElementGrid(this, settings);
        return grid;
    }

    public void DebugBuildArea(Vector3 point, float radius)
    {
        GetOrCreateGrid().GetCellsInRadius(point, radius, hitCells);
        Debug.Log($"{name}: {grid.Cells.Count} cells");
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || grid == null)
            return;

        float size = settings.cellSize * 0.95f;

        foreach (ElementCell cell in grid.Cells)
        {
            Gizmos.color = CellColor(cell);
            Gizmos.DrawWireCube(cell.Position + Vector3.up * 0.02f, new Vector3(size, 0f, size));

            if (cell.State != CellState.Disabled)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(cell.Position, cell.Position + cell.Normal * 0.3f);
            }
        }
    }

    private Color CellColor(ElementCell cell) =>
        cell.State == CellState.Disabled ? new Color(0.8f, 0.2f, 0.2f) : Color.green;

}