using UnityEngine;

public class DevControl : MonoBehaviour
{
    [Tooltip("Object to move. If empty, moves the GameObject this component is on.")]
    public Transform target;

    [Tooltip("Position used by the 'Move To Custom Position' button.")]
    public Vector3 customPosition;

    Transform Target => target != null ? target : transform;

    public void MoveToCustomPosition()
    {
        Teleport(customPosition);
    }

    public void TeleportToMap1()
    {
        Teleport(new Vector3(0f, 1f, -10f));
    }

    public void TeleportToMap2()
    {
        Teleport(new Vector3(100f, 1f, -10f));
    }

    public void Teleport(Vector3 position)
    {
        Transform t = Target;

        // A CharacterController overwrites the transform on its next move, so turn it off while teleporting.
        bool hasCC = t.TryGetComponent(out CharacterController cc);
        if (hasCC) cc.enabled = false;

        t.position = position;

        if (t.TryGetComponent(out Rigidbody rb))
        {
            rb.position = position;
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        if (hasCC) cc.enabled = true;
    }
}
