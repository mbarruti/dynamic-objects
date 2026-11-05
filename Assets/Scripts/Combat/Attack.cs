using UnityEngine;
using static DynamicEvaluator;
using static HitType;

public class Attack
{
    private Hit hit = new(5f, Slash);

    public void ApplyHit(DynamicObjectBehaviour target)
    {
        HitEvaluation evaluation = Evaluate(hit, target.Data);
        HitInfo hitInfo = new(evaluation.FinalDamage, hit.HitType);

        Debug.Log($"Damage done is {hitInfo.Damage}");

        target.OnHit(hitInfo);
    }
}
