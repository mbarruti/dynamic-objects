using System.Collections.Generic;
using UnityEngine;
using static DynamicEvaluator;

[System.Serializable]
public class Attack
{
    [SerializeField] private float damage;
    [SerializeField] private HitType hitType;

    [Header("AOE")]
    [SerializeField] private float areaRadius = 2f;
    [SerializeField] private float areaAmount = 100f;
    [SerializeField] private float spreadPoints = 300f;

    [Header("Element")]
    [SerializeField] private FireProperty fireElement = new();
    [SerializeField] private IElementProperty element;

    private Hit hit;

    public void ApplyHit(DynamicObjectBehaviour target)
    {
        AddElement(fireElement);

        List<EffectDefinition> defs = new()
        {
            element.Def
        };

        hit = new(damage, hitType, element.Type, defs);

        HitEvaluation evaluation = EvaluateHit(hit, target.Data);
        HitInfo hitInfo = new(this, evaluation.FinalDamage, hit.HitType, hit.ElementType, evaluation.Defs);

        // Debug.Log($"Damage done is {hitInfo.Damage}");

        target.OnHit(hitInfo);
    }

    public void ApplyToArea(Vector3 point)
    {
        if (element == null)
            AddElement(fireElement);

        FireBudget budget = null;
        if (element.Type == ElementType.Fire)
        {
            budget = new FireBudget();
            budget.Add(spreadPoints);
        }

        ElementPulses.Emit(new ElementPulse
        {
            Type = element.Type,
            Position = point,
            Radius = areaRadius,
            Amount = areaAmount,
            Budget = budget,
            EffectDef = element.Def,
            Source = this,
        });
    }

    public void AddElement(IElementProperty newElement)
    {
        element = newElement;
    }
}
