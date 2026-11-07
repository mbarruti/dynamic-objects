using UnityEngine;

public interface IGameplayProperty
{
    
}

public interface IResistanceProperty : IGameplayProperty
{
    ElementType Element { get; }
    float Resistance { get; }
    bool Nullify { get; }
}

public class FireResistance : IResistanceProperty
{
    public ElementType Element => ElementType.Fire;
    public float Resistance => 1f;
    public bool Nullify => true;
}