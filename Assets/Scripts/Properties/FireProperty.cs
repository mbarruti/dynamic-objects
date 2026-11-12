using UnityEngine;

[System.Serializable]
public class FireProperty : IElementProperty
{
    [SerializeField] private BurnEffectDefinition burn = new();

    public ElementType Type => ElementType.Fire;

    public EffectDefinition Def => burn;
}
