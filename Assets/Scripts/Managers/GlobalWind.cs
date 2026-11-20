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
}