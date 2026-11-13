using UnityEngine;

// Base for effects that last a duration and tick at an interval (burn, poison, ...).
// Owns the timer: Apply/Refresh/Remove are fixed here; subclasses customize via the On* hooks.
// Non-generic so DynamicObjectBehaviour can check `existing is TimedEffect` without knowing the definition type.
public abstract class TimedEffect : IEffect
{
    private const float Epsilon = 0.0001f;
    private float tickTimer;

    protected TimedEffectDefinition Definition { get; set; }

    // Seconds elapsed in the current run. Reset to 0 on Refresh.
    protected float Elapsed { get; private set; }

    public bool IsFinished { get; private set; }

    public DynamicObjectBehaviour Target { get; private set; }


    protected TimedEffect(TimedEffectDefinition definition)
    {
        Definition = definition;
    }

    public virtual bool CanAffect(DynamicObject target) => true;

    public EffectDefinition GetDefinition() => Definition;

    public void Apply(DynamicObjectBehaviour target)
    {
        Target = target;
        OnApply(target);
    }

    // Restarts the timer from zero.
    public void Refresh(DynamicObjectBehaviour target, TimedEffectDefinition definition)
    {
        if (definition != null && definition.GetType() == Definition.GetType())
            Definition = definition;

        Elapsed = 0f;
        // May have finished this frame but not been removed by Update yet.
        IsFinished = false;

        OnRefresh(target);
    }

    public void Remove(DynamicObjectBehaviour target)
    {
        OnRemove(target);
    }

    // --- Hooks ---

    protected virtual void OnApply(DynamicObjectBehaviour target) { }

    protected virtual void OnTick(DynamicObjectBehaviour target, float deltaTime) { }

    protected virtual void OnRefresh(DynamicObjectBehaviour target) { }

    protected virtual void OnRemove(DynamicObjectBehaviour target) { }

    // --- Timer ---

    public void Tick(float deltaTime)
    {
        if (IsFinished)
            return;

        // Don't advance past the end of the duration.
        float dt = Mathf.Min(deltaTime, Definition.Duration - Elapsed);
        Elapsed += dt;
        tickTimer += dt;

        // A while loop, not an if: a long frame (lag spike) can cover several intervals.
        while (tickTimer >= Definition.TickInterval - Epsilon)
        {
            tickTimer -= Definition.TickInterval;
            OnTick(Target, Definition.TickInterval);
        }

        if (Elapsed >= Definition.Duration - Epsilon)
        {
            tickTimer = 0f;
            IsFinished = true;
        }
    }
}

// Helper. Typed layer so subclasses get their own definition without casting.
public abstract class TimedEffect<TDef> : TimedEffect where TDef : TimedEffectDefinition
{
    protected TDef Def => (TDef)Definition;

    protected TimedEffect(TDef definition) : base(definition) { }
}
