using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// One pooled particle effect per burning cell.
public class FireVisuals
{
    private readonly ParticleSystem prefab;
    private readonly float cellSize;
    private readonly Transform root;
    private readonly Stack<ParticleSystem> pool = new();
    private readonly Dictionary<ElementCell, ParticleSystem> active = new();

    private Material placeholderMaterial;

    public FireVisuals(ParticleSystem prefab, string ownerName, float cellSize)
    {
        this.prefab = prefab;
        this.cellSize = cellSize;

        // Own root at scale 1: parenting to the floor (scale 10) would scale the flames too.
        root = new GameObject($"{ownerName} Fire Visuals").transform;
    }

    public void Show(ElementCell cell)
    {
        if (active.ContainsKey(cell))
            return;

        ParticleSystem ps = pool.Count > 0 ? pool.Pop() : Create();
        ps.transform.position = cell.Position;
        ps.Play(true);
        active.Add(cell, ps);
    }

    public void Hide(ElementCell cell)
    {
        if (!active.Remove(cell, out ParticleSystem ps))
            return;

        // Stop emitting; live particles fade out on their own (world-space simulation).
        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        pool.Push(ps);
    }

    public void Dispose()
    {
        if (root != null)
            Object.Destroy(root.gameObject);
    }

    private ParticleSystem Create() =>
        prefab != null ? Object.Instantiate(prefab, root) : CreatePlaceholder();

    private ParticleSystem CreatePlaceholder()
    {
        GameObject go = new("FireCell");
        go.transform.SetParent(root, false);

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.playOnAwake = false;
        main.loop = true;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.35f * cellSize, 0.7f * cellSize);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.3f, 0.05f), new Color(1f, 0.75f, 0.2f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 40;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 18f;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(cellSize * 0.9f, 0.05f, cellSize * 0.9f);

        // Rise in world space. All three axes must use the same curve mode.
        ParticleSystem.VelocityOverLifetimeModule velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
        velocity.y = new ParticleSystem.MinMaxCurve(1f, 2.2f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);

        ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

        Gradient fade = new();
        fade.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0f), new GradientColorKey(new Color(0.8f, 0.1f, 0f), 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0f, 1f) });

        ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
        color.enabled = true;
        color.color = fade;

        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = GetPlaceholderMaterial();
        return ps;
    }

    // Additive URP particle material. Shader.Find is fine in the editor; for builds use a prefab.
    private Material GetPlaceholderMaterial()
    {
        if (placeholderMaterial != null)
            return placeholderMaterial;

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
        {
            placeholderMaterial = new Material(Shader.Find("Sprites/Default"));
            return placeholderMaterial;
        }

        placeholderMaterial = new Material(shader);
        placeholderMaterial.SetFloat("_Surface", 1f);   // Transparent
        placeholderMaterial.SetFloat("_Blend", 2f);     // Additive
        placeholderMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        placeholderMaterial.SetFloat("_DstBlend", (float)BlendMode.One);
        placeholderMaterial.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
        placeholderMaterial.SetFloat("_DstBlendAlpha", (float)BlendMode.One);
        placeholderMaterial.SetFloat("_ZWrite", 0f);
        placeholderMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        placeholderMaterial.renderQueue = (int)RenderQueue.Transparent;
        placeholderMaterial.SetColor("_BaseColor", Color.white);
        return placeholderMaterial;
    }
}