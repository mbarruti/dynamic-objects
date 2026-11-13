using System.Collections.Generic;
using UnityEngine;

public class PulseGizmos : MonoBehaviour
{
    [SerializeField] private float showSeconds = 0.4f;

    private void OnEnable()
    {
        ElementPulses.Recent.Clear();
        ElementPulses.Record = true;
    }

    private void OnDisable() => ElementPulses.Record = false;

    // Prune here, not in OnDrawGizmos: that one doesn't run when gizmos are off,
    // and the list would grow forever.
    private void Update()
    {
        List<ElementPulses.PulseRecord> recent = ElementPulses.Recent;
        float now = Time.time;

        for (int i = recent.Count - 1; i >= 0; i--)
        {
            if (now - recent[i].Time > showSeconds)
                recent.RemoveAt(i);
        }
    }

    private void OnDrawGizmos()
    {
        float now = Time.time;

        foreach (ElementPulses.PulseRecord p in ElementPulses.Recent)
        {
            Color c = p.Receivers > 0 ? ElementColor(p.Type) : Color.gray;
            c.a = 1f - (now - p.Time) / showSeconds;      // fades out
            Gizmos.color = c;
            Gizmos.DrawWireSphere(p.Position, p.Radius);
        }
    }

    private static Color ElementColor(ElementType type)
    {
        switch (type)
        {
            case ElementType.Fire: return new Color(1f, 0.5f, 0f);
            default: return Color.white;
        }
    }
}
