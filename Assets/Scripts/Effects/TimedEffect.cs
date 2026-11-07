using System.Collections;
using UnityEngine;

// Base for effects that last a duration and tick at an interval (burn, poison, ...).
// Owns the timer: Apply/Refresh/Remove are fixed here; subclasses customize via the On* hooks.
// Non-generic so DynamicObjectBehaviour can check `existing is TimedEffect` without knowing the definition type.
public abstract class TimedEffect : IEffect
{
    private Coroutine routine;

    protected TimedEffectDefinition Definition { get; set; }

    // Seconds elapsed in the current run. Reset to 0 on Refresh.
    protected float Elapsed { get; private set; }

    public bool IsFinished { get; private set; }

    public virtual bool CanStack => false;

    protected TimedEffect(TimedEffectDefinition definition)
    {
        Definition = definition;
    }

    public EffectDefinition GetDefinition() => Definition;

    public void Apply(DynamicObjectBehaviour target)
    {
        OnApply(target);
        StartTimer(target);
    }

    // Restarts the timer from zero.
    public void Refresh(DynamicObjectBehaviour target, TimedEffectDefinition definition)
    {
        if (definition != null && definition.GetType() == Definition.GetType())
            Definition = definition;

        StopTimer(target);

        // May have finished this frame but not been removed by Update yet.
        IsFinished = false;

        OnRefresh(target);
        StartTimer(target);
    }

    public void Remove(DynamicObjectBehaviour target)
    {
        StopTimer(target);
        OnRemove(target);
    }

    // --- Hooks for subclasses ---

    protected virtual void OnApply(DynamicObjectBehaviour target) { }

    // Called after each interval. `deltaTime` is the interval length (last one may be shorter).
    protected virtual void OnTick(DynamicObjectBehaviour target, float deltaTime) { }

    protected virtual void OnRefresh(DynamicObjectBehaviour target) { }

    protected virtual void OnRemove(DynamicObjectBehaviour target) { }

    // --- Timer ---

    private void StartTimer(DynamicObjectBehaviour target)
    {
        routine = target.StartCoroutine(Run(target));
    }

    private void StopTimer(DynamicObjectBehaviour target)
    {
        if (routine == null)
            return;

        target.StopCoroutine(routine);
        routine = null;
    }

    private IEnumerator Run(DynamicObjectBehaviour target)
    {
        Elapsed = 0f;

        // Definition is read live each loop, so Inspector edits affect active effects too.
        while (Elapsed < Definition.Duration)
        {
            // Last tick may be shorter if duration isn't a multiple of the interval.
            float step = Mathf.Min(Definition.TickInterval, Definition.Duration - Elapsed);
            yield return new WaitForSeconds(step);

            Elapsed += step;
            OnTick(target, step);
        }

        IsFinished = true;
        routine = null;
    }
}

// Typed layer so subclasses get their own definition without casting.
public abstract class TimedEffect<TDef> : TimedEffect where TDef : TimedEffectDefinition
{
    protected TDef Def => (TDef)Definition;

    protected TimedEffect(TDef definition) : base(definition) { }
}
