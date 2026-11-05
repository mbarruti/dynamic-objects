using UnityEngine;

public class BoxBehaviour : DynamicObjectBehaviour
{
    protected override ObjectMaterial Material => ObjectMaterial.Wood;

    protected override float HitPoints => 10;
}