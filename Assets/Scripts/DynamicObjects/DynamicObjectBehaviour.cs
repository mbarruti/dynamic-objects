using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class DynamicObjectBehaviour : MonoBehaviour, IInteractable, IElementReceiver
{
    public DynamicObject Data { get; private set; }

    protected virtual bool IsDamageable { get; } = false;
    protected virtual float HitPoints { get; } = 10f;
    protected virtual ObjectMaterial Material { get; } = ObjectMaterial.Default;

    protected virtual bool Flammable { get; } = false;
    protected virtual bool Conductor { get; } = false;

    public float CurrentHP { get; protected set; }

    private List<IEffect> currentEffects = new();

    protected virtual void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        Data = new DynamicObject(
            gameObject.name,
            IsDamageable,
            HitPoints,
            Material,
            Flammable,
            Conductor
        );

        CurrentHP = HitPoints;
    }

    public virtual void Apply()
    {
        Debug.Log($"Object material is {Data.Material}");
    }

    protected virtual void Update()
    {
        // Iterate backwards so finished effects can be removed in place.
        for (int i = currentEffects.Count - 1; i >= 0; i--)
        {
            IEffect effect = currentEffects[i];

            if (effect.IsFinished)
            {
                effect.Remove(this);
                currentEffects.RemoveAt(i);
            }
        }
    }

    public virtual void OnHit(HitInfo hitInfo)
    {
        TakeDamage(hitInfo.Damage);
        AddEffects(hitInfo.Effects);
    }

    public virtual void TakeDamage(float damage)
    {
        // Already dead (Destroy is deferred to end of frame).
        if (!IsDamageable || damage == 0 || CurrentHP <= 0)
            return;

        CurrentHP = Mathf.Max(0, CurrentHP - damage);
        Debug.Log($"Current HP of the object: {CurrentHP}");

        if (CurrentHP <= 0)
            Destroy(gameObject);
    }

    public virtual void AddEffects(List<IEffect> effects)
    {
        if (effects == null || (CurrentHP <= 0 && IsDamageable))
            return;

        foreach (IEffect effect in effects)
        {
            if (effect == null)
                continue;

            if (TryRefresh(effect))
                continue;

            currentEffects.Add(effect);
            effect.Apply(this);
        }
    }

    // Non-stacking timed effect already active: restart it instead of adding a second one.
    // Returns false when the effect should be added as a new instance.
    private bool TryRefresh(IEffect effect)
    {
        if (effect is not TimedEffect incoming || incoming.CanStack)
            return false;

        if (currentEffects.Find(e => e.GetType() == effect.GetType()) is not TimedEffect existing)
            return false;

        existing.Refresh(this, incoming.GetDefinition() as TimedEffectDefinition);
        return true;
    }

    public void ReceivePulse(ElementPulse pulse)
    {
        switch (pulse.Type)
        {
            case ElementType.Fire:
                if (TryGetEffect(out BurnEffect burning))
                {
                    burning.Budget = FireBudget.Join(burning.Budget, pulse.Budget);
                    return;
                }

                if (pulse.EffectDef == null)
                    return;

                IEffect effect = pulse.EffectDef.CreateInstance();
                if (effect is BurnEffect burn)
                    burn.Budget = pulse.Budget?.Root;

                AddEffects(DynamicEvaluator.EvaluateEffects(new List<IEffect> { effect }, Data));
                break;
        }
    }

    public bool HasEffect<T>() where T : IEffect =>
        currentEffects.Exists(e => e is T && !e.IsFinished);

    public bool TryGetEffect<T>(out T effect) where T : class, IEffect
    {
        foreach (IEffect e in currentEffects)
        {
            if (e is T match && !e.IsFinished)
            {
                effect = match;
                return true;
            }
        }

        effect = null;
        return false;
    }
}