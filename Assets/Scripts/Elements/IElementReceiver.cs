using UnityEngine;

public interface IElementReceiver
{
    float ReceivePulse(ElementPulse pulse);

    bool AcceptsEffect(EffectDefinition def, Collider hit);
}