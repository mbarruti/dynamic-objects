using UnityEngine;

[System.Serializable]
public class ElementMeterSettings
{
    [Tooltip("Max value of the meter")]
    [Min(0.01f)] public float value = 10f;
    [Tooltip("Seconds without damage before meter starts recovering back.")]
    [Min(0f)] public float recoverDelay = 1f;
    [Tooltip("Meter recovered per second once its recover delay has passed.")]
    [Min(0f)] public float regenPerSecond = 5f;
}

public struct ElementMeter
{
    public float Value;

    private float recoverTimer;

    public ElementMeter(ElementMeterSettings metterSettings)
    {
        Value = metterSettings.value;
        recoverTimer = 0f;
    }

    public bool IsFull(ElementMeterSettings meterSettings) => Value >= meterSettings.value;

    public bool Damage(float amount, ElementMeterSettings meterSettings)
    {
        Value = Mathf.Max(0f, Value - amount);
        recoverTimer = meterSettings.recoverDelay;
        return Value <= 0f;
    }

    public bool RecoveryTick(float dt, ElementMeterSettings s)
    {
        if (Value >= s.value) return true;

        if (recoverTimer > 0f)
        {
            recoverTimer -= dt;
            return false;
        }

        Value = Mathf.Min(s.value, Value + s.regenPerSecond * dt);
        return Value >= s.value;
    }

    public void Reset(ElementMeterSettings meterSettings)
    {
        Value = meterSettings.value;
        recoverTimer = 0f;
    }
}