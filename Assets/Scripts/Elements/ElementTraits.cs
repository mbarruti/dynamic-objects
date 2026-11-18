using UnityEngine;

[System.Serializable]
public struct ElementTraits
{
    [Tooltip("Reacts to fire")]
    public bool flammable;
    [Tooltip("Reacts to electricity")]
    public bool conductor;
}