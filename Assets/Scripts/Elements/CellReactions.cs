using UnityEngine;

public static class CellEvaluation
{
    public static void ApplyFire(ElementGrid grid, ElementCell cell, float dmg)
    {
        if (cell.State != CellState.Normal)
            return;

        cell.HitPoints = Mathf.Max(0f, cell.HitPoints - dmg);

        if (cell.HitPoints <= 0f)
            grid.TryIgnite(cell);
    }
}