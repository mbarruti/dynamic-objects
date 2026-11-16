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

    [Header("Lighter")]
    [SerializeField, Min(0.01f)] private float lighterInterval = 0.5f;
    [SerializeField] private FireProperty lighterFireElement;
    private float lighterTimer;

    [Header("Element")]
    [SerializeField] private FireProperty fireElement = new();
    [SerializeField] private IElementProperty element;

    private Hit hit;

    // Like a fire projectile
    public void ApplyHit(DynamicObjectBehaviour target, Vector3 point)
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

    // Lighter
    public void HoldLighter(Vector3 point, float deltaTime)
    {
        lighterTimer += deltaTime;
        if (lighterTimer < lighterInterval)
            return;

        lighterTimer -= lighterInterval;
        ApplyLighter(point);
    }

    // Next press fires immediately instead of waiting one interval.
    // public void ReleaseLighter() => lighterTimer = lighterInterval;

    public void ApplyLighter(Vector3 point)
    {
        AddElement(lighterFireElement);

        ElementPulses.Emit(new ElementPulse
        {
            Type = element.Type,
            Position = point,
            Radius = element.PulseData.radius,
            Amount = element.PulseData.amount,
            SpreadPoints = element.PulseData.spreadPoints,
            EffectDef = element.Def,
            Source = this
        });
    }

    // Molotov or fire explosive
    public void ApplyToArea(Vector3 point)
    {
        AddElement(fireElement);

        ElementPulses.Emit(new ElementPulse
        {
            Type = element.Type,
            Position = point,
            Radius = areaRadius,
            Amount = areaAmount,
            SpreadPoints = spreadPoints,
            EffectDef = element.Def,
            Source = this,
        });
    }

    public void AddElement(IElementProperty newElement)
    {
        element = newElement;
    }
}
