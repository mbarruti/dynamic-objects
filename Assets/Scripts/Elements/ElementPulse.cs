using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public struct ElementPulse
{
    public ElementType Type;
    public Vector3 Position;
    public float Radius;
    public float Amount;
    // public float SpreadPoints;
    public FireBudget Budget;
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

    public static void Emit(ElementPulse pulse)
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
                IElementReceiver receiver = hits[i].GetComponentInParent<IElementReceiver>();

                if (receiver == null || ReferenceEquals(receiver, pulse.Source) || receivers.Contains(receiver))
                    continue;
                
                receivers.Add(receiver);
            }

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

            foreach (IElementReceiver receiver in receivers)
                receiver.ReceivePulse(pulse);
        }
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