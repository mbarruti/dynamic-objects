using UnityEngine;

public enum WindDirection
{
    Left,
    Right,
    Forward,
    Back,
    ForwardLeft,
    ForwardRight,
    BackLeft,
    BackRight
}

public class GlobalWind : MonoBehaviour
{
    [Tooltip("Direction the wind blows towards.")]
    [SerializeField] private WindDirection direction = WindDirection.Left;

    [Tooltip("Determines the reaction of affected entites (0 equals no wind).")]
    [Range(0f, 2f)] public float windStrength = 1f;

    public Vector3 Direction => direction switch
    {
        WindDirection.Left => Vector3.left,
        WindDirection.Right => Vector3.right,
        WindDirection.Forward => Vector3.forward,
        WindDirection.Back => Vector3.back,
        WindDirection.ForwardLeft => new Vector3(-1, 0, 1).normalized,
        WindDirection.ForwardRight => new Vector3(1, 0, 1).normalized,
        WindDirection.BackLeft => new Vector3(-1, 0, -1).normalized,
        WindDirection.BackRight => new Vector3(1, 0, -1).normalized,
        _ => Vector3.left
    };

    public static GlobalWind Instance { get; private set; }

    private void OnEnable()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Two GlobalWinds in the scene ({Instance.name}, {name}). Only the first is used.");
            return;
        }

        Instance = this;
    }

    private void OnDisable()
    {
        if (Instance == this)
            Instance = null;
    }

    public static float Factor(Vector3 from, Vector3 to)
    {
        GlobalWind globalWind = Instance;
        if (globalWind == null || globalWind.windStrength <= 0f)
            return 1f;
        
        Vector3 dir = to - from;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f)
            return 1f;

        return 1f + globalWind.windStrength * Vector3.Dot(globalWind.Direction, dir.normalized);
    }

    // Dev
    private void OnDrawGizmos()
    {
        if (windStrength <= 0f)
            return;

        Vector3 dir = Direction;
        Vector3 from = transform.position;
        Vector3 to = from + dir * (1f + windStrength) * 2f;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(from, to);

        // Arrowhead
        Vector3 side = Vector3.Cross(Vector3.up, dir) * 0.4f;
        Vector3 back = -dir * 0.6f;
        Gizmos.DrawLine(to, to + back + side);
        Gizmos.DrawLine(to, to + back - side);
    }
}