using UnityEngine;

public static class CellEvaluation
{
    public static void React(ElementGrid grid, ElementCell cell, ElementPulse pulse, ref float points)
    {
        switch (pulse.Type)
        {
            case ElementType.Fire:
                ApplyIgnite(grid, cell, pulse.Amount, ref points, pulse.StartsFire);
                break;
        }
    }

    public static void ApplyIgnite(ElementGrid grid, ElementCell cell, float dmg, ref float points,
                                    bool free = false, ElementCell source = null)
    {
        if (cell.State != CellState.Normal || !cell.Settings.traits.flammable)
            return;

        if (cell.Meter.Damage(dmg, cell.Settings.meterSettings))
            grid.TryBurn(cell, ref points, free, source);
        
        if (cell.State == CellState.Normal)
            grid.MarkDamaged(cell);
    }
}