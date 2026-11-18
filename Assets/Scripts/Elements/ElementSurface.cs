using UnityEngine;

// Marks these colliders as ground that elements act on, and holds that ground's fire values.
// The cells themselves live in the scene's GridManager.
public class ElementSurface : MonoBehaviour
{
    [SerializeField] private SurfaceSettings settings = new();

    public SurfaceSettings Settings => settings;
}