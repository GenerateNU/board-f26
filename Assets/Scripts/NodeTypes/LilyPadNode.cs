using System.Collections.Generic;
using UnityEngine;

public enum NodeHazard { None, Mites, Alligator, Winning }

public class LilyPadNode : BasicNode
{
    [Header("Visual Variety (placeholder until final art)")]
    [Tooltip("Randomize rotation, size and look when the game starts.")]
    [SerializeField] private bool randomizeLook = true;

    [Tooltip("The object to rotate/scale. Leave empty to use the object that has the pad's Renderer.")]
    [SerializeField] private Transform padVisual;

    [Tooltip("The pad's Renderer (MeshRenderer or SpriteRenderer). Leave empty to find it automatically.")]
    [SerializeField] private Renderer padRenderer;

    [Tooltip("Drop lily pad materials here. One is picked at random per pad. Empty = keep the current look.")]
    [SerializeField] private Material[] lilyPadMaterials;
    public override bool IsTraversable => !IsOccupied && !isFishOnlyNode;
    public override NodeHazard NodeType => NodeHazard.None;

    private void Awake()
    {
        if (randomizeLook) RandomizeLook();
    }

    public override void CroakUsedOnPad()
    {
        if (TryGetComponent<FlyPickup>(out var flyPickup))
        {
            flyPickup.RemoveFly();
        }
    }

    // Makes each pad look a little different: notch direction, size and leaf art.
    private void RandomizeLook()
    {
        if (padRenderer == null) padRenderer = FindPadRenderer();
        // if (padVisual == null) padVisual = padRenderer != null ? padRenderer.transform : transform;

        // Lay the pad flat (90 on X) and point the notch in a random direction.
        // padVisual.localEulerAngles = new Vector3(90, 0, Random.Range(0f, 360f));

        // +-10% size, relative to the current size.
        // float size = Random.Range(0.9f, 1.1f);
        // padVisual.localScale *= size;

        // Pick the art. sharedMaterial avoids creating a new material copy per pad.
        if (padRenderer != null && lilyPadMaterials != null && lilyPadMaterials.Length > 0)
        {
            Material material = lilyPadMaterials[Random.Range(0, lilyPadMaterials.Length)];
            if (material != null) padRenderer.sharedMaterial = material;
        }
    }

    // The pad's own Renderer: on this object first, otherwise the first child that has one.
    private Renderer FindPadRenderer()
    {
        if (TryGetComponent<Renderer>(out var own)) return own;
        return GetComponentInChildren<Renderer>();
    }
}