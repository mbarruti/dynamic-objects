using UnityEngine;

public class MetalBoxBehaviour : DynamicObjectBehaviour
{
    protected override bool IsDamageable => true;

    protected override ObjectMaterial Material => ObjectMaterial.Metal;

    protected override bool Conductor => true;
}
