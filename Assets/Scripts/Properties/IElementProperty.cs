public interface IElementProperty : IGameplayProperty, IElementEmitter
{
    ElementType Type { get; }
    EffectDefinition Def { get; }
}
