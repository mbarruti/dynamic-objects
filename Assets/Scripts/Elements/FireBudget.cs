using UnityEngine;

public class FireBudget
{
    public float SpreadPoints { get; private set; }

    public void Add(float points) => SpreadPoints += Mathf.Max(0f, points);

    public bool TrySpend(float cost)
    {
        if (SpreadPoints < cost)
            return false;
        
        SpreadPoints -= cost;
        return true;
    }
}