using UnityEngine;

public static class CellEvaluation
{
    public static void ApplyIgnite(ElementGrid grid, ElementCell cell, float dmg, FireBudget budget)
    {
        if (cell.State != CellState.Normal)
            return;

        cell.HitPoints = Mathf.Max(0f, cell.HitPoints - dmg);

        if (cell.HitPoints <= 0f)
            grid.TryBurn(cell, budget);
        
        if (cell.State == CellState.Normal)
            grid.MarkDamaged(cell);
    }
}