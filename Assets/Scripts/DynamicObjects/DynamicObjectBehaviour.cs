using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class DynamicObjectBehaviour : MonoBehaviour, IInteractable, IElementReceiver
{
    [SerializeField] private ElementMeterSettings meterSettings = new();
    private ElementMeter meter;
    private List<IEffect> currentEffects = new();

    public DynamicObject Data { get; private set; }

    protected virtual bool IsDamageable { get; } = false;
    protected virtual float HitPoints { get; } = 10f;

    protected virtual ObjectMaterial Material { get; } = ObjectMaterial.Default;
    protected virtual bool Flammable { get; } = false;
    protected virtual bool Conductor { get; } = false;

    public float CurrentHP { get; protected set; }

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
        meter = new ElementMeter(meterSettings);
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

            if (effect is TimedEffect timed)
                timed.Tick(Time.deltaTime);

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
        AddEffects(hitInfo.EffectDefs);
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

    public virtual void AddEffects(List<EffectDefinition> defs)
    {
        if (defs == null) return;
        foreach (EffectDefinition def in defs)
            AddEffect(def);
    }

    public void AddEffect(EffectDefinition def)
    {
        if (def == null)
            return;

        if (!def.CanStack && TryRefresh(def))
            return;

        AddInstance(def.CreateInstance());
    }

    private void AddInstance(IEffect effect)
{
    if (effect == null || (CurrentHP <= 0 && IsDamageable))
        return;

    currentEffects.Add(effect);
    effect.Apply(this);
}

    // Non-stacking timed effect already active: restart it instead of adding a second one.
    // Returns false when the effect should be added as a new instance.
    private bool TryRefresh(EffectDefinition def)
    {
        if (def == null)
            return false;
            
        IEffect existing = currentEffects.Find(e => e.GetDefinition().GetType() == def.GetType());
        if (existing == null)
            return false;

        if (existing is TimedEffect timed)
            timed.Refresh(this, def as TimedEffectDefinition);

        return true;   // non-timed and non-stackable: already active, nothing to do
    }

    public void ReceivePulse(ElementPulse pulse)
    {
        switch (pulse.Type)
        {
            case ElementType.Fire:
                if (TryGetEffect(out BurnEffect burning))
                {
                    burning.Budget = FireBudget.Join(burning.Budget, pulse.Budget);
                    TryRefresh(pulse.EffectDef);
                    return;
                }

                if (pulse.EffectDef == null || !pulse.EffectDef.CanAffect(Data))
                    return;

                IEffect effect = pulse.EffectDef.CreateInstance();
                if (effect is BurnEffect burn)
                    burn.Budget = pulse.Budget?.Root;

                AddInstance(effect);
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