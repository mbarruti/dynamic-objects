using UnityEngine;

public class DynamicObject
{
    public string Name { get; private set; }
    public float HitPoints { get; private set; }
    public ObjectMaterial Material { get; private set; }

    public DynamicObject(string name, float hitPoints, ObjectMaterial material)
    {
        Name = name;
        HitPoints = hitPoints;
        Material = material;
    }
}
