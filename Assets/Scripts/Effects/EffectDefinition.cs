// Shared, read-only data for an effect (configured on an Attack).
// Never holds per-target state.
[System.Serializable]
public abstract class EffectDefinition
{
    public abstract IEffect CreateInstance();
    public abstract ElementType Element { get; }
    public virtual bool CanStack => false;
    public virtual bool CanAffect(DynamicObject target) => true;
}
