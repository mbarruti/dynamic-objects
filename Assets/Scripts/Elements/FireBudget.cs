using UnityEngine;

public class FireBudget
{
    private float spreadPoints;
    private FireBudget mergedInto;

    public FireBudget(float points = 0f) => spreadPoints = Mathf.Max(0f, points);

    public FireBudget Root
    {
        get
        {
            // Path compresion
            if (mergedInto == null) return this;
            mergedInto = mergedInto.Root;
            return mergedInto;
        }
    }

    public float SpreadPoints => Root.spreadPoints;
    public void Add(float points) => Root.spreadPoints += Mathf.Max(0f, points);

    public bool TrySpend(float cost)
    {
        if (Root.spreadPoints < cost)
            return false;
        Root.spreadPoints -= cost;
        return true;
    }

    public static FireBudget Join(FireBudget local, FireBudget incoming)
    {
        if (local == null) return incoming?.Root;
        if (incoming == null) return local.Root;
        
        FireBudget a = local.Root;
        FireBudget b = incoming.Root;

        // Same fire
        if (a == b) return a;

        a.spreadPoints += b.spreadPoints;
        b.spreadPoints = 0f;
        b.mergedInto = a;
        return a;
    }
}