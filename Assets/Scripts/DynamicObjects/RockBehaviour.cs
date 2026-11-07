using UnityEngine;

public class RockBehaviour : DynamicObjectBehaviour
{
    protected override bool IsDamageable => false;

    protected override float HitPoints => 10;

    protected override ObjectMaterial Material => ObjectMaterial.Stone;
}
