using System;
using System.IO;
using IceSkates.Workshop.Helpers;
using IceSkates.Workshop.Runtime;
using IceSkates.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IceSkates.Workshop.Diagnostics;

// Opt-in renders of the actual game assets, isolated from the restaurant and save state.
internal static class VisualChecks
{
    public static System.Collections.IEnumerator Capture()
    {
        var stage = new GameObject("Ice Skates isolated render stage");
        stage.transform.position = new Vector3(1000, 3, 1000);
        var cameraObject = new GameObject("Ice Skates render camera");
        var lightObject = new GameObject("Ice Skates render light");
        try
        {
            string folder = Path.Combine(Application.persistentDataPath, "IceSkatesChecks", KitchenIceSkates.Mod.MOD_VERSION);
            const string prefix = "--iceskates-render-dir=";
            foreach (var arg in Environment.GetCommandLineArgs())
                if (arg.StartsWith(prefix, StringComparison.Ordinal)) folder = arg.Substring(prefix.Length);
            Directory.CreateDirectory(folder);
            var camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.cullingMask = 1 << 30;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.16f, 0.19f, 0.23f);
            camera.orthographic = true;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 30;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.cullingMask = 1 << 30;
            light.transform.rotation = Quaternion.Euler(40, -35, 0);

            var model = Object.Instantiate(PrefabFactory.CreateWearablePrefab(), stage.transform, false);
            Layer(model);
            Render(camera, stage.transform.position + new Vector3(0, 0.24f, 0), new Vector3(1.1f, 0.95f, 1.3f), 0.53f, Path.Combine(folder, "skates-close.png"));
            model.SetActive(false);
            var rack = Object.Instantiate(PrefabFactory.CreateProviderPrefab(held: true), stage.transform, false);
            Layer(rack);
            Render(camera, stage.transform.position + new Vector3(0, 0.4f, 0), new Vector3(1.3f, 1.2f, 1.6f), 0.77f, Path.Combine(folder, "rack.png"));
            rack.SetActive(false);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(stage.transform, false);
            floor.transform.localPosition = new Vector3(0, -0.04f, -0.7f);
            floor.transform.localScale = new Vector3(4, 0.1f, 4);
            floor.layer = 30;
            var floorMaterial = new Material(model.GetComponentInChildren<Renderer>().sharedMaterial);
            floorMaterial.SetColor("_Color0", new Color(0.36f, 0.36f, 0.36f, 0));
            floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;
            var actor = new GameObject("Actual SkateMotion regression actor");
            actor.transform.SetParent(stage.transform, false);
            actor.layer = 30;
            actor.transform.localPosition = new Vector3(0, 0, -1.8f);
            var motion = actor.AddComponent<SkateMotion>();
            motion.SetMovementRoot(actor.transform);
            motion.SetEquipped(true);
            Layer(actor);
            var trails = actor.GetComponentsInChildren<TrailRenderer>();
            for (int i = 0; i < 80; i++)
            {
                // Real motion component, including idle render frames between physics steps.
                float t = (i / 2) / 39f;
                actor.transform.localPosition = new Vector3(0.28f * Mathf.Sin(t * Mathf.PI), 0, -1.8f + t * 1.8f);
                yield return null;
            }
            foreach (var trail in trails)
            {
                if (trail.positionCount < 3) throw new InvalidOperationException("Gameplay sampler did not generate a streak.");
                var point = trail.GetPosition(trail.positionCount - 1);
                if (Mathf.Abs(point.y - (stage.transform.position.y + SkateTrailRules.GroundOffset)) > 0.001f)
                    throw new InvalidOperationException("Streak is not above the translated floor.");
                var mesh = new Mesh();
                trail.BakeMesh(mesh, camera, true);
                int vertexCount = mesh.vertexCount;
                Object.Destroy(mesh);
                if (vertexCount < 4) throw new InvalidOperationException("Trail mesh did not generate: " + trail.positionCount + " points.");
                KitchenIceSkates.Mod.LogInfo("VISUAL trail generated " + vertexCount + " vertices.");
            }
            var streakPixels = Render(camera, stage.transform.position + new Vector3(0, 0.08f, -0.6f), new Vector3(1.2f, 2.3f, 2.1f), 1.15f, Path.Combine(folder, "ice-streaks.png"));
            foreach (var trail in trails) trail.enabled = false;
            var baselinePixels = Render(camera, stage.transform.position + new Vector3(0, 0.08f, -0.6f), new Vector3(1.2f, 2.3f, 2.1f), 1.15f, Path.Combine(folder, "ice-streaks-baseline.png"));
            foreach (var trail in trails) trail.enabled = true;
            int changedPixels = 0;
            for (int i = 0; i < streakPixels.Length; i++)
                if (streakPixels[i].b > baselinePixels[i].b + 20 && streakPixels[i].g > baselinePixels[i].g + 15) changedPixels++;
            if (changedPixels < 200) throw new InvalidOperationException("Trail geometry exists but is not visibly rendered over the floor: " + changedPixels + " pixels.");
            KitchenIceSkates.Mod.LogInfo("VISUAL PIXEL CHECK PASS: " + changedPixels + " ice-colored pixels above the opaque floor.");
            float fadeDeadline = Time.time + SkateTrailRules.Lifetime + 0.2f;
            var fadeTarget = RenderTexture.GetTemporary(64, 64, 24);
            try
            {
                camera.targetTexture = fadeTarget;
                while (Time.time < fadeDeadline)
                {
                    camera.Render();
                    yield return null;
                }
            }
            finally { camera.targetTexture = null; RenderTexture.ReleaseTemporary(fadeTarget); }
            var fadedPixels = Render(camera, stage.transform.position + new Vector3(0, 0.08f, -0.6f), new Vector3(1.2f, 2.3f, 2.1f), 1.15f, Path.Combine(folder, "ice-streaks-stopped.png"));
            int stalePixels = 0;
            for (int i = 0; i < fadedPixels.Length; i++)
                if (fadedPixels[i].b > baselinePixels[i].b + 20 && fadedPixels[i].g > baselinePixels[i].g + 15) stalePixels++;
            if (stalePixels > 30) throw new InvalidOperationException("Stationary streak did not fade: " + stalePixels + " pixels; points=" + trails[0].positionCount);
            KitchenIceSkates.Mod.LogInfo("VISUAL FADE CHECK PASS: " + stalePixels + " ice-colored pixels after stopping.");
            motion.SetSuspended(true);
            foreach (var trail in trails) if (trail.positionCount != 0) throw new InvalidOperationException("Pause left stale ice streaks.");
            motion.SetSuspended(false);
            var physicsRoot = new GameObject("Separate player physics root");
            physicsRoot.transform.SetParent(actor.transform, false);
            physicsRoot.layer = actor.layer;
            motion.SetMovementRoot(physicsRoot.transform);
            for (int i = 0; i < 8; i++)
            {
                physicsRoot.transform.localPosition += new Vector3(0.06f, 0, 0);
                yield return null;
            }
            foreach (var trail in trails) if (trail.positionCount < 3) throw new InvalidOperationException("Separate walking root did not produce a streak.");
            physicsRoot.transform.localPosition += Vector3.right * 10;
            yield return null;
            foreach (var trail in trails) if (trail.positionCount != 0) throw new InvalidOperationException("Teleport left a bridging streak.");
            motion.SetEquipped(false);
            Object.Destroy(floorMaterial);
            KitchenIceSkates.Mod.LogInfo("GAMEPLAY TRAIL CHECK PASS: SkateMotion sampling, stepped movement, translated floor, separate walking root, pause/teleport cleanup.");
            KitchenIceSkates.Mod.LogInfo("VISUAL CHECK PASS: actual Unity asset renders at " + folder);
        }
        finally
        {
            Object.Destroy(stage);
            Object.Destroy(cameraObject);
            Object.Destroy(lightObject);
        }
    }

    private static void Layer(GameObject root)
    {
        foreach (var child in root.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 30;
    }

    private static Color32[] Render(Camera camera, Vector3 target, Vector3 offset, float size, string path)
    {
        camera.transform.position = target + offset;
        camera.transform.LookAt(target);
        camera.orthographicSize = size;
        var targetTexture = RenderTexture.GetTemporary(1000, 850, 24, RenderTextureFormat.ARGB32);
        var previous = RenderTexture.active;
        Texture2D? pixels = null;
        try
        {
            camera.targetTexture = targetTexture;
            camera.Render();
            RenderTexture.active = targetTexture;
            pixels = new Texture2D(1000, 850, TextureFormat.RGB24, false);
            pixels.ReadPixels(new Rect(0, 0, 1000, 850), 0, 0);
            pixels.Apply();
            File.WriteAllBytes(path, pixels.EncodeToPNG());
            return pixels.GetPixels32();
        }
        finally
        {
            camera.targetTexture = null;
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(targetTexture);
            if (pixels != null) Object.Destroy(pixels);
        }
    }
}
