using UnityEngine;

[System.Serializable]
public class FireProperty : IElementProperty
{
    // // Concrete type, so the burn settings show directly in the Inspector.
    [SerializeField] private BurnEffectDefinition burn = new();

    public ElementType Element => ElementType.Fire;
    public EffectDefinition EffectDefinition => burn;
}
