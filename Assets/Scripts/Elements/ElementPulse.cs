using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public struct ElementPulse
{
    public ElementType Type;
    public Vector3 Position;
    public float Radius;
    public float Amount;
    public float SpreadPoints;
    public bool StartsFire;
    public EffectDefinition EffectDef;
    public object Source;
}

public static class ElementPulses
{
    private static readonly Collider[] hits = new Collider[64];

    // ------- Dev --------
    public struct PulseRecord
    {
        public Vector3 Position;
        public float Radius;
        public ElementType Type;
        public int Receivers;
        public float Time;
    }

    // Debug only: filled while a PulseGizmos component is active.
    public static bool Record;
    public static readonly List<PulseRecord> Recent = new();

    // -----------------------

    public static float Emit(ElementPulse pulse)
    {
        float radius = Mathf.Max(0.05f, pulse.Radius);
        int count = Physics.OverlapSphereNonAlloc(pulse.Position, radius, hits,
                                                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        
        if (count == hits.Length)
            Debug.LogWarning($"ElementPulses: buffer full at {pulse.Position}, some receivers may have been skipped");
        
        // Collect first, then notify: a receiver could emit its own pulse, which reuses hits list.
        using(ListPool<IElementReceiver>.Get(out List<IElementReceiver> receivers))
        {
            for (int i = 0; i < count; i++)
            {
                IElementReceiver receiver = ResolveReceiver(hits[i]);

                if (receiver == null || ReferenceEquals(receiver, pulse.Source) || receivers.Contains(receiver))
                    continue;
                
                receivers.Add(receiver);
            }

            if (receivers.Count > 1)
            pulse.SpreadPoints /= receivers.Count;

            // Dev
            if (Record)
            {
                Recent.Add(new PulseRecord
                {
                    Position = pulse.Position,
                    Radius = radius,                 // the clamped one, i.e. what OverlapSphere used
                    Type = pulse.Type,
                    Receivers = receivers.Count,
                    Time = Time.time,
                });
            }
            // ---------
            float used = 0f;
            foreach (IElementReceiver receiver in receivers)
                used += receiver.ReceivePulse(pulse);
            
            return used;
        }
    }

    // Every surface is part of the scene's one grid, so they all map to ElementWorld.
    // A pulse touching two surfaces then reaches the grid once, and its points aren't split between them.
    private static IElementReceiver ResolveReceiver(Collider col)
    {
        if (col.GetComponentInParent<ElementSurface>() != null)
            return ElementGridManager.Instance;

        return col.GetComponentInParent<IElementReceiver>();
    }

}

[System.Serializable]
public class PulseSettings
{
    [Min(0f)] public float radius;
    [Min(0f)] public float amount;
    [Min(0f)] public float spreadPoints;
}

public interface IElementEmitter
{
    PulseSettings PulseData { get; }
}