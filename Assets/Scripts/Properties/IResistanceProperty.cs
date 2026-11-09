using UnityEngine;

public interface IResistanceProperty : IGameplayProperty
{
    ElementType Element { get; }
    float Resistance { get; }
    bool Nullify { get; }
}

public class FireResistance : IResistanceProperty
{
    [SerializeField] private float resistance = 1f;
    [SerializeField] public bool nullify = true;

    public ElementType Element => ElementType.Fire;
    public float Resistance => resistance;
    public bool Nullify => nullify;
}