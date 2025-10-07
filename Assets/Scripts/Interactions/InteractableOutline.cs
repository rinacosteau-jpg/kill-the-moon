using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class InteractableOutline : MonoBehaviour {
    private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineThickness = Shader.PropertyToID("_OutlineThickness");
    private static readonly int OutlineEnabled = Shader.PropertyToID("_OutlineEnabled");

    [Serializable]
    private struct OutlineTarget {
        public Renderer renderer;
        public Material material;
    }

    [SerializeField] private Renderer[] targetRenderers = Array.Empty<Renderer>();
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField, Range(0.001f, 0.2f)] private float outlineThickness = 0.02f;

    private readonly List<OutlineTarget> outlineTargets = new List<OutlineTarget>();
    private bool initialized;
    private bool isHighlighted;

    private void Awake() {
        Initialize();
        ApplySettings();
        UpdateHighlightState();
    }

    private void Reset() {
        AutoCollectRenderers();
    }

    private void OnValidate() {
        if (targetRenderers == null || targetRenderers.Length == 0)
            AutoCollectRenderers();

        if (!initialized)
            return;

        ApplySettings();
        UpdateHighlightState();
    }

    private void AutoCollectRenderers() {
        targetRenderers = GetComponentsInChildren<Renderer>();
    }

    private void Initialize() {
        if (initialized)
            return;

        if (targetRenderers == null || targetRenderers.Length == 0)
            AutoCollectRenderers();

        Shader outlineShader = Shader.Find("Custom/InteractableOutline");
        if (outlineShader == null) {
            Debug.LogError("[InteractableOutline] Shader 'Custom/InteractableOutline' not found.");
            return;
        }

        outlineTargets.Clear();

        foreach (Renderer renderer in targetRenderers) {
            if (renderer == null)
                continue;

            Material outlineMaterial = new Material(outlineShader) {
                hideFlags = HideFlags.HideAndDontSave
            };

            var materials = new List<Material>(renderer.sharedMaterials);
            materials.Add(outlineMaterial);
            renderer.sharedMaterials = materials.ToArray();

            outlineTargets.Add(new OutlineTarget {
                renderer = renderer,
                material = outlineMaterial
            });
        }

        initialized = true;

        ApplySettings();
        UpdateHighlightState();
    }

    private void ApplySettings() {
        foreach (OutlineTarget target in outlineTargets) {
            if (target.material == null)
                continue;

            target.material.SetColor(OutlineColor, outlineColor);
            target.material.SetFloat(OutlineThickness, outlineThickness);
        }
    }

    public void SetHighlighted(bool highlighted) {
        Initialize();

        isHighlighted = highlighted;
        UpdateHighlightState();
    }

    private void UpdateHighlightState() {
        float enabledValue = isHighlighted ? 1f : 0f;

        foreach (OutlineTarget target in outlineTargets) {
            if (target.material == null)
                continue;

            target.material.SetFloat(OutlineEnabled, enabledValue);
        }
    }

    private void OnDestroy() {
        foreach (OutlineTarget target in outlineTargets) {
            if (target.renderer != null && target.material != null) {
                var materials = new List<Material>(target.renderer.sharedMaterials);
                if (materials.Remove(target.material))
                    target.renderer.sharedMaterials = materials.ToArray();
            }

            if (target.material != null)
                Destroy(target.material);
        }

        outlineTargets.Clear();
        initialized = false;
    }
}
