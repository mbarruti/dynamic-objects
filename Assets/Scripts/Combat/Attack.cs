using System.Collections.Generic;
using UnityEngine;
using static DynamicEvaluator;
using static HitType;

[System.Serializable]
public class Attack
{
    [SerializeField] private float damage;
    [SerializeField] private HitType hitType;

    [Header("Area (when it hits the ground)")]
    [SerializeField] private float areaRadius = 0.5f;
    [SerializeField] private float areaAmount = 100f;
    [SerializeField] private float spreadPoints = 60f;

    [SerializeField] private IElementProperty element;

    private Hit hit;

    public void ApplyHit(DynamicObjectBehaviour target)
    {
        FireProperty fireElement = new();
        AddElement(fireElement);

        List<IEffect> effects = new()
        {
            element.EffectDefinition.CreateInstance()
        };

        hit = new(damage, hitType, element.Element, effects);

        HitEvaluation evaluation = Evaluate(hit, target.Data);
        HitInfo hitInfo = new(this, evaluation.FinalDamage, hit.HitType, hit.ElementType, evaluation.Effects);

        Debug.Log($"Damage done is {hitInfo.Damage}");

        target.OnHit(hitInfo);
    }

    public void ApplyToArea(Vector3 point)
    {
        if (element == null)
            AddElement(new FireProperty());

        ElementPulses.Emit(new ElementPulse
        {
            Element = element.Element,
            Position = point,
            Radius = areaRadius,
            Amount = areaAmount,
            SpreadPoints = element.Element == ElementType.Fire ? spreadPoints : 0f,
            Effect = element.EffectDefinition,
            Source = this,
        });
    }

    public void AddElement(IElementProperty newElement)
    {
        element = newElement;
    }
}
