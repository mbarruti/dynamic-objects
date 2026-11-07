// Shared, read-only data for an effect (configured on an Attack).
// Never holds per-target state: that lives in the IEffect instances it creates.
[System.Serializable]
public abstract class EffectDefinition
{
    // Creates a new runtime instance for one target.
    public abstract IEffect CreateInstance();
    public abstract ElementType Element { get; }
}
