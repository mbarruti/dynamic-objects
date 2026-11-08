using UnityEngine;

[System.Serializable]
public class FireProperty : IElementProperty
{
    [SerializeField] private BurnEffectDefinition burn = new();

    public ElementType Element => ElementType.Fire;
    public EffectDefinition EffectDefinition => burn;
}
