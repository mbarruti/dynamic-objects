using UnityEngine;

[System.Serializable]
public class FireProperty : IElementProperty
{
    [SerializeField] private BurnEffectDefinition burn = new();
    [SerializeField] private PulseSettings pulseData = new();

    public ElementType Type => ElementType.Fire;

    public EffectDefinition Def => burn;

    public PulseSettings PulseData => pulseData;
}