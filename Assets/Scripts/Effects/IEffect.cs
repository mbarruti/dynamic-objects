// Runtime instance of an effect on one target. Created by an EffectDefinition.
public interface IEffect
{
    bool IsFinished { get; }

    // Called once when the effect is added to a target.
    void Apply(DynamicObjectBehaviour target);

    // Called once when the effect is removed from a target.
    void Remove(DynamicObjectBehaviour target);

    EffectDefinition GetDefinition();
}
