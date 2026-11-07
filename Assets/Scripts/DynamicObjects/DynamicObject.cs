public class DynamicObject
{
    public string Name { get; private set; }
    public bool IsDamageable { get; private set; }
    public float HitPoints { get; private set; }
    public ObjectMaterial Material { get; private set; }

    public DynamicObject(string name, bool isDamageable, float hitPoints, ObjectMaterial material)
    {
        Name = name;
        IsDamageable = isDamageable;
        HitPoints = hitPoints;
        Material = material;
    }
}
