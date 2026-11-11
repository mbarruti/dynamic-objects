using UnityEngine;
using UnityEngine.InputSystem;

public class Interact : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float maxDistance;
    [SerializeField] private Attack attack;

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            TryInteractRight();
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryInteractLeft();
        }
    }

    private void TryInteractRight()
    {
        Ray ray = mainCamera.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            IInteractable interactable =
                hit.collider.GetComponentInParent<IInteractable>();

            interactable?.Apply();
        }
    }

    private void TryInteractLeft()
    {
        Ray ray = mainCamera.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            IInteractable interactable =
                hit.collider.GetComponentInParent<IInteractable>();

            if (interactable is DynamicObjectBehaviour dynamicObject)
            {
                attack.ApplyHit(dynamicObject);
            }
            else
            {
                attack.ApplyToArea(hit.point);
            }
        }
    }
}