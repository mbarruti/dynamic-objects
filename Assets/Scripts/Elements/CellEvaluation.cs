using UnityEngine;

public static class CellEvaluation
{
    public static void React(ElementGrid grid, ElementCell cell, ElementPulse pulse, FireBudget budget, bool free)
    {
        switch (pulse.Type)
        {
            case ElementType.Fire:
                ApplyIgnite(grid, cell, pulse.Amount, budget, free);
                break;
        }
    }

    public static void ApplyIgnite(ElementGrid grid, ElementCell cell, float dmg, FireBudget budget, bool free = false)
    {
        if (cell.State != CellState.Normal)
            return;

        if (cell.Meter.Damage(dmg, grid.Settings.meterSettings))
            grid.TryBurn(cell, budget, free);
        
        if (cell.State == CellState.Normal)
            grid.MarkDamaged(cell);
    }
}