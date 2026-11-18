using UnityEngine;

// Fire values of one kind of ground (grass, dirt, wood...). Each cell reads them from its own surface.
[System.Serializable]
public class SurfaceSettings
{
    [Header("Traits")]
    public ElementTraits traits;

    [Header("Fire")]
    [Tooltip("Lifetime of a burning cell until burn out.")]
    [Min(0.01f)] public float burnLifetime = 5f;
    [Tooltip("Damage per second a burning cell on this surface deals to each neighbour.")]
    [Min(0f)] public float igniteDamagePerSecond = 5f;
    [Tooltip("Spread points needed to ignite a cell on this surface.")]
    [Min(0f)] public float spreadCost = 8f;
    [Tooltip("Meter threshold before an element takes effect.")]
    public ElementMeterSettings meterSettings = new();

    [Header("Pulses")]
    [Tooltip("Radius of the pulse a burning cell sends to nearby objects.")]
    [Min(0f)] public float pulseRadius = 1f;
    [Tooltip("Meter damage that pulse deals to objects.")]
    [Min(0f)] public float pulseAmount = 3f;
    [Tooltip("Burn given to flammable objects a burning cell reaches.")]
    public BurnEffectDefinition objectEffect = new();

    [Header("Fire Fuel")]
    [Tooltip("Chance that a cell adds spread points to the fire when it ignites.")]
    [Range(0f, 1f)] public float fuelChance = 0f;
    [Tooltip("Spread points added when the fuel roll succeeds.")]
    [Min(0f)] public float fuelPoints = 0f;
}