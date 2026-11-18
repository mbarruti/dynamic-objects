using UnityEngine;

public class MetalBoxBehaviour : DynamicObjectBehaviour
{
    protected override bool IsDamageable => true;

    protected override ObjectMaterial Material => ObjectMaterial.Metal;

    protected override ElementTraits Traits => new() { conductor = true };
}
