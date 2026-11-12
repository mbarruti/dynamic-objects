public interface IElementProperty : IGameplayProperty
{
    ElementType Type { get; }
    EffectDefinition Def { get; }
}
