using UnityEngine;

public class EffectContext
{
    public DynamicObjectBehaviour Target { get; }
    public HitInfo HitInfo { get; }

    public EffectContext(
        DynamicObjectBehaviour target,
        HitInfo hitInfo)
    {
        Target = target;
        HitInfo = hitInfo;
    }
}