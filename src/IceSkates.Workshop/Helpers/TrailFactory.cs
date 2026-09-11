using IceSkates.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace IceSkates.Workshop.Helpers;

internal static class TrailFactory
{
    private static Material? material;

    internal static Material Material
    {
        get
        {
            if (material != null) return material;
            var shader = Shader.Find("Sprites/Default");
            if (shader == null || !shader.isSupported)
                throw new System.InvalidOperationException("Ice streak sprite shader unavailable.");
            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false) { name = "Ice streak soft edges", wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color[256];
            for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                float across = Mathf.Abs((y / 15f - 0.5f) * 2);
                float alpha = Mathf.Pow(1 - across, 0.6f);
                pixels[y * 16 + x] = new Color(1, 1, 1, alpha);
            }
            texture.SetPixels(pixels); texture.Apply(false, true);
            material = new Material(shader) { name = "Ice Skates fading streak", mainTexture = texture, color = Color.white };
            return material;
        }
    }

    internal static TrailRenderer Create(Transform parent, float side)
    {
        var anchor = new GameObject(side < 0 ? "Left ice streak" : "Right ice streak");
        anchor.layer = parent.gameObject.layer;
        anchor.transform.SetParent(parent, false);
        anchor.transform.localPosition = new Vector3(side * 0.17f, SkateTrailRules.GroundOffset, -0.14f);
        anchor.transform.rotation = Quaternion.Euler(90, 0, 0);
        var trail = anchor.AddComponent<TrailRenderer>();
        trail.sharedMaterial = Material;
        trail.time = SkateTrailRules.Lifetime;
        trail.startWidth = SkateTrailRules.Width;
        trail.endWidth = 0.015f;
        trail.minVertexDistance = SkateTrailRules.PointSpacing;
        trail.numCornerVertices = 2;
        trail.numCapVertices = 2;
        trail.alignment = LineAlignment.TransformZ;
        trail.shadowCastingMode = ShadowCastingMode.Off;
        trail.receiveShadows = false;
        trail.textureMode = LineTextureMode.Stretch;
        trail.colorGradient = new Gradient
        {
            colorKeys = new[] { new GradientColorKey(new Color(0.72f, 0.97f, 1f), 0), new GradientColorKey(new Color(0.2f, 0.7f, 0.96f), 1) },
            alphaKeys = new[] { new GradientAlphaKey(0.85f, 0), new GradientAlphaKey(0.45f, 0.55f), new GradientAlphaKey(0, 1) }
        };
        trail.emitting = false;
        return trail;
    }
}
