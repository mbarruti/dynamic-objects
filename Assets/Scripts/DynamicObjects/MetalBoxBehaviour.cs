using UnityEngine;

public class MetalBoxBehaviour : DynamicObjectBehaviour
{
    protected override bool IsDamageable => true;

    protected override float HitPoints => 10;

    protected override ObjectMaterial Material => ObjectMaterial.Metal;
}
