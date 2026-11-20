using UnityEngine;

public static class CellEvaluation
{
    public static void React(ElementGrid grid, ElementCell cell, ElementPulse pulse, ref float points)
    {
        switch (pulse.Type)
        {
            case ElementType.Fire:
                float dmg = pulse.Amount * pulse.WindFactor(cell.Position);
                if (dmg > 0f) ApplyIgnite(grid, cell, dmg, pulse.EffectDef, ref points, pulse.StartsFire);
                break;
        }
    }

    public static void ApplyIgnite(ElementGrid grid, ElementCell cell, float dmg, EffectDefinition def,
                                    ref float points, bool free = false, ElementCell source = null)
    {
        if (cell.State != CellState.Normal || !def.CanAffect(cell.Settings.traits))
            return;

        if (cell.Meter.Damage(dmg, cell.Settings.meterSettings))
            grid.TryBurn(cell, ref points, free, source);
        
        if (cell.State == CellState.Normal)
            grid.MarkDamaged(cell);
    }
}