using System.Collections.Generic;
using UnityEngine;
using static DynamicEvaluator;
using static HitType;

[System.Serializable]
public class Attack
{
    [SerializeField] private float damage;
    [SerializeField] private HitType hitType;

    // [SerializeReference]
    // private List<IGameplayProperty> properties = new();
    [SerializeField]
    private IElementProperty element;

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

    public void AddElement(IElementProperty newElement)
    {
        element = newElement;
    }
}
