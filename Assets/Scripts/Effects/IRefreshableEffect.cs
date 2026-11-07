public interface IRefreshableEffect : IEffect
{
    EffectDefinition Def { get; }

    void Refresh(DynamicObjectBehaviour target, IEffect incomingEffect)
    {
        
    }
}