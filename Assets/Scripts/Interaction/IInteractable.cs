using UnityEngine;

public interface IInteractable
{
    void Apply();
    void OnHit(HitInfo hitInfo);
}
