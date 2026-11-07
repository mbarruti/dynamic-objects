using UnityEngine;

public class BoxBehaviour : DynamicObjectBehaviour
{
    protected override bool IsDamageable => true;

    protected override float HitPoints => 100;

    protected override ObjectMaterial Material => ObjectMaterial.Wood;
}