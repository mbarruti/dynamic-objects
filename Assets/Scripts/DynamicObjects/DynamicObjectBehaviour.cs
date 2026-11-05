using UnityEngine;

public abstract class DynamicObjectBehaviour : MonoBehaviour, IInteractable
{
    public DynamicObject Data { get; protected set; }
    protected abstract float HitPoints { get; }
    protected abstract ObjectMaterial Material { get; }

    public float CurrentHP { get; protected set; }

    protected virtual void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        Data = new DynamicObject(
            gameObject.name,
            HitPoints,
            Material
        );

        CurrentHP = HitPoints;
    }

    public virtual void Apply()
    {
        Debug.Log($"Object material is {Data.Material}");
    }

    public virtual void OnHit(HitInfo hitInfo)
    {
        if (CurrentHP - hitInfo.Damage < 0)
        {
            CurrentHP = 0;
            Destroy(gameObject);
        }
        else
        {
            CurrentHP -= hitInfo.Damage;
        }
        Debug.Log($"Current HP of the object: {CurrentHP}");
    }
}