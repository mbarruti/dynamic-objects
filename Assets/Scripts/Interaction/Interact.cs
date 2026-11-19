using UnityEngine;
using UnityEngine.InputSystem;

public class Interact : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float maxDistance;
    [SerializeField] private Attack attack;

    [Header("Grab (hold right click)")]
    [Tooltip("How quickly the held object is pulled toward the cursor point.")]
    [SerializeField] private float grabStrength = 12f;
    [Tooltip("Top speed the held object can be moved at.")]
    [SerializeField] private float maxGrabSpeed = 20f;
    [Tooltip("How quickly the held object's spin is damped (0 = free spin).")]
    [SerializeField] private float angularDamping = 8f;
    [SerializeField] private float minHoldDistance = 1.5f;
    [SerializeField] private float scrollSensitivity = 0.005f;

    private Rigidbody grabbedBody;
    private Vector3 localGrabPoint;
    private float holdDistance;
    private RigidbodyInterpolation previousInterpolation;

    private void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        // Grab: hold right click
        if (mouse.rightButton.wasPressedThisFrame)
            TryGrab();
        else if (!mouse.rightButton.isPressed && grabbedBody != null)
            ReleaseGrab();

        // Scroll wheel pushes / pulls the held object
        if (grabbedBody != null)
        {
            float scroll = mouse.scroll.ReadValue().y;
            holdDistance = Mathf.Clamp(
                holdDistance + scroll * scrollSensitivity,
                minHoldDistance,
                maxDistance
            );
        }

        // // Lighter: hold F
        // Keyboard keyboard = Keyboard.current;
        // if (keyboard != null && keyboard.fKey.isPressed)
        //     TryLighter();
        // else
        //     attack.ReleaseLighter();

        if (mouse.leftButton.wasPressedThisFrame)
        {
            TryInteractLeft();
        }
    }

    private void FixedUpdate()
    {
        if (grabbedBody == null || Mouse.current == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Vector3 target = ray.GetPoint(holdDistance);
        Vector3 grabPointWorld = grabbedBody.transform.TransformPoint(localGrabPoint);

        // Drive velocity toward the target so the object still collides and
        // gets blocked by other physics bodies instead of teleporting through them.
        Vector3 desiredVelocity = (target - grabPointWorld) * grabStrength;
        grabbedBody.linearVelocity = Vector3.ClampMagnitude(desiredVelocity, maxGrabSpeed);

        grabbedBody.angularVelocity = Vector3.Lerp(
            grabbedBody.angularVelocity,
            Vector3.zero,
            angularDamping * Time.fixedDeltaTime
        );
    }

    private void TryGrab()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            return;

        Rigidbody body = hit.rigidbody;
        if (body == null || body.isKinematic)
            return;

        grabbedBody = body;
        localGrabPoint = body.transform.InverseTransformPoint(hit.point);
        holdDistance = Mathf.Max(hit.distance, minHoldDistance);

        previousInterpolation = body.interpolation;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.WakeUp();
    }

    private void ReleaseGrab()
    {
        // Velocity is kept on release, so flicking the mouse throws the object.
        grabbedBody.interpolation = previousInterpolation;
        grabbedBody = null;
    }

    private void OnDisable()
    {
        if (grabbedBody != null)
            ReleaseGrab();
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
