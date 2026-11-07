public interface IElementProperty : IGameplayProperty
{
    ElementType Element { get; }
    EffectDefinition EffectDefinition { get; }
}
