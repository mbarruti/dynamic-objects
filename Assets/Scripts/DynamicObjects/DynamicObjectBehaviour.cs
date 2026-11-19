using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class DynamicObjectBehaviour : MonoBehaviour, IInteractable, IElementReceiver, IElementEmitter
{
    [SerializeField] private ElementMeterSettings meterSettings = new();

    private PulseSettings pulseData;
    private Collider mainCollider;
    private Bounds reportedBounds;
    private List<IEffect> currentEffects = new();
    private ElementMeter meter;

    protected virtual bool IsDamageable { get; } = false;
    protected virtual float HitPoints { get; } = 10f;
    protected virtual ObjectMaterial Material { get; } = ObjectMaterial.Default;

    public DynamicObject Data { get; private set; }
    public float CurrentHP { get; protected set; }

    protected virtual ElementTraits Traits => default;

    public PulseSettings PulseData => pulseData;

    protected virtual void Awake()
    {
        Initialize();
    }

    protected virtual void OnEnable()
    {
        reportedBounds = ObjectBounds;
        ElementGridManager mgr = ElementGridManager.Instance;
        if (mgr != null) mgr.Grid.MarkCoverage(reportedBounds);
    }

    protected virtual void OnDisable()
    {
        if (reportedBounds == null) return;
        ElementGridManager mgr = ElementGridManager.Instance;
        if (mgr != null) mgr.Grid.MarkCoverage(reportedBounds);
    }

    private void Initialize()
    {
        mainCollider = GetComponentInChildren<Collider>();
        CurrentHP = HitPoints;
        meter = new ElementMeter(meterSettings);
        pulseData = new()
        {
            radius = mainCollider != null ? mainCollider.bounds.extents.magnitude : 0.5f,
            amount = 5f,
            spreadPoints = 4f
        };

        Data = new DynamicObject(
            gameObject.name,
            IsDamageable,
            HitPoints,
            Material,
            Traits
        );
    }

    protected virtual void Update()
    {
        float dt = Time.deltaTime;

        ReportMovement();
        
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

        // TO-DO: generalize for any continuous element effect
        if (!HasEffect<BurnEffect>())
            meter.RecoveryTick(dt, meterSettings);
    }

    // Bounding volume of the object
    public Bounds ObjectBounds =>
        mainCollider != null ? mainCollider.bounds : new Bounds(transform.position, Vector3.zero);

    // Centre of the object's bounds.
    public Vector3 ObjectOrigin =>
        mainCollider != null ? mainCollider.bounds.center : transform.position;

    public virtual void Apply()
    {
        Debug.Log($"Object material is {Data.Material}");
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
        // Debug.Log($"Current HP of the object: {CurrentHP}");

        if (CurrentHP <= 0)
            Destroy(gameObject);
    }

    public virtual void AddEffects(List<EffectDefinition> defs)
    {
        if (defs == null) return;
        foreach (EffectDefinition def in defs)
            AddOrRefreshEffect(def);
    }

    public void AddOrRefreshEffect(EffectDefinition def)
    {
        if (def == null)
            return;

        if (!def.CanStack && TryRefresh(def))
            return;

        AddEffectInstance(def.CreateInstance());
    }

    private void AddEffectInstance(IEffect effect)
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

    // Elements

    public float ReceivePulse(ElementPulse pulse)
    {
        DynamicEvaluator.React(this, pulse);

        if (!pulse.StartsFire || pulse.SpreadPoints <= 0f || !TryGetEffect( out BurnEffect burn))
            return 0f;
        
        burn.AddPoints(pulse.SpreadPoints);
        return pulse.SpreadPoints;
    }

    public bool AcceptsEffect(EffectDefinition def, Collider hit) => def.CanAffect(Data.Traits);

    public bool DamageMeter(float dmg) => meter.Damage(dmg, meterSettings);

    // Physics

    private void ReportMovement()
    {
        Bounds currentBounds = ObjectBounds;
        if ((currentBounds.center - reportedBounds.center).sqrMagnitude < 0.01f &&
            (currentBounds.size - reportedBounds.size).sqrMagnitude < 0.01f)
            return;
        
        ElementGridManager mgr = ElementGridManager.Instance;
        if (mgr != null)
        {
            mgr.Grid.MarkCoverage(reportedBounds);
            mgr.Grid.MarkCoverage(currentBounds);
            reportedBounds = currentBounds;
        }
    }

    // Misc

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        ElementGridManager mgr = ElementGridManager.Instance;
        if (mgr == null || !mgr.DrawSpreadPoints || !TryGetEffect(out BurnEffect burn))
            return;

        Vector3 top = ObjectBounds.center + Vector3.up * (ObjectBounds.extents.y + 0.2f);
        UnityEditor.Handles.Label(top, burn.StoredPoints.ToString("0.0"));
    }
#endif

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