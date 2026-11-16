using UnityEngine;
using UnityEngine.InputSystem;

public class Interact : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float maxDistance;
    [SerializeField] private Attack attack;

    private void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        if (mouse.rightButton.isPressed)
            TryLighter();
        // else
        //     attack.ReleaseLighter();

        if (mouse.leftButton.wasPressedThisFrame)
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

            if (interactable is DynamicObjectBehaviour dynamicObject)
            {
                attack.ApplyHit(dynamicObject, hit.point);
            }
        }
    }

    private void TryLighter()
    {
        Ray ray = mainCamera.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
                attack.HoldLighter(hit.point, Time.deltaTime);
        }
    }

    private void TryInteractLeft()
    {
        Ray ray = mainCamera.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
                attack.ApplyToArea(hit.point);
        }
    }
}