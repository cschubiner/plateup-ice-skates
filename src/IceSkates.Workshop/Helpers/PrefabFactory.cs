using System.Collections.Generic;
using IceSkates.Workshop.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace IceSkates.Workshop.Helpers;

internal static class PrefabFactory
{
    private static GameObject? templates;
    private static GameObject? item;
    private static GameObject? wearable;
    private static GameObject? rack;
    private static GameObject? heldRack;
    private static Material? blue;
    private static Material? white;
    private static Material? metal;
    private static Material? dark;

    private static Transform Container
    {
        get
        {
            if (templates == null)
            {
                templates = new GameObject("Ice Skates templates");
                Object.DontDestroyOnLoad(templates);
                templates.SetActive(false);
            }
            return templates.transform;
        }
    }

    private static Material Material(Color colour)
    {
        var shader = Shader.Find("Simple Flat");
        if (shader == null || !shader.isSupported) throw new System.InvalidOperationException("PlateUp Simple Flat shader unavailable.");
        var result = new Material(shader) { name = "Ice Skates solid " + ColorUtility.ToHtmlStringRGB(colour) };
        // PlateUp's native flat shader reads _Color0, not Material.color (_Color).
        colour.a = 0;
        result.SetColor("_Color0", colour);
        result.SetFloat("_Shininess", 0.15f);
        result.SetFloat("_OverlayScale", 10f);
        return result;
    }

    private static void EnsureMaterials()
    {
        if (blue == null) blue = Material(new Color(0.08f, 0.68f, 0.86f));
        if (white == null) white = Material(new Color(0.92f, 0.96f, 1f));
        if (metal == null) metal = Material(new Color(0.66f, 0.78f, 0.84f));
        if (dark == null) dark = Material(new Color(0.055f, 0.09f, 0.12f));
    }

    private static void Block(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        Shape(parent, name, PrimitiveType.Cube, position, scale, material);
    }

    private static GameObject Shape(Transform parent, string name, PrimitiveType shape, Vector3 position, Vector3 scale, Material material)
    {
        var part = GameObject.CreatePrimitive(shape);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        var collider = part.GetComponent<Collider>();
        collider.enabled = false;
        Object.Destroy(collider);
        part.GetComponent<Renderer>().sharedMaterial = material;
        return part;
    }

    public static GameObject CreateSkatesItemPrefab()
    {
        if (item != null) return item;
        item = new GameObject("Ice Skates pair");
        item.transform.SetParent(Container, false);
        Object.Instantiate(CreateWearablePrefab(), item.transform, false);
        item.AddComponent<SkateItemVisibility>();
        return item;
    }

    public static GameObject CreateWearablePrefab()
    {
        if (wearable != null) return wearable;
        EnsureMaterials();
        wearable = new GameObject("Ice Skates boots");
        wearable.transform.SetParent(Container, false);
        AddBoot(wearable.transform, -0.17f);
        AddBoot(wearable.transform, 0.17f);
        return wearable;
    }

    private static void AddBoot(Transform parent, float x)
    {
        var boot = new GameObject(x < 0 ? "Left skate" : "Right skate");
        boot.transform.SetParent(parent, false);
        boot.transform.localPosition = new Vector3(x, 0, 0);
        var root = boot.transform;
        AddBlade(root);
        foreach (float z in new[] { -0.12f, 0.13f })
            Block(root, "Blade support", new Vector3(0, 0.105f, z), new Vector3(0.055f, 0.055f, 0.065f), metal!);
        var sole = Shape(root, "Black sole", PrimitiveType.Capsule, new Vector3(0, 0.145f, 0.015f), new Vector3(0.275f, 0.235f, 0.045f), dark!);
        sole.transform.localRotation = Quaternion.Euler(90, 0, 0);
        var toe = Shape(root, "Rounded white toe", PrimitiveType.Capsule, new Vector3(0, 0.225f, 0.035f), new Vector3(0.265f, 0.22f, 0.19f), white!);
        toe.transform.localRotation = Quaternion.Euler(90, 0, 0);
        Shape(root, "High ankle", PrimitiveType.Capsule, new Vector3(0, 0.29f, -0.115f), new Vector3(0.24f, 0.105f, 0.235f), white!);
        Shape(root, "Cyan padded cuff", PrimitiveType.Cylinder, new Vector3(0, 0.40f, -0.115f), new Vector3(0.265f, 0.026f, 0.25f), blue!);
        Shape(root, "Dark ankle opening", PrimitiveType.Cylinder, new Vector3(0, 0.428f, -0.115f), new Vector3(0.175f, 0.004f, 0.16f), dark!);
        var tongue = Shape(root, "Cyan tongue", PrimitiveType.Cube, new Vector3(0, 0.33f, 0.005f), new Vector3(0.12f, 0.025f, 0.22f), blue!);
        tongue.transform.localRotation = Quaternion.Euler(22, 0, 0);
        for (int i = 0; i < 4; i++)
        {
            float z = -0.055f + i * 0.045f;
            foreach (float angle in new[] { -22f, 22f })
            {
                var lace = Shape(root, "Crossed white lace", PrimitiveType.Cube,
                    new Vector3(0, 0.342f - (z - 0.005f) * 0.4f, z), new Vector3(0.15f, 0.012f, 0.012f), white!);
                lace.transform.localRotation = Quaternion.Euler(22, angle, 0);
            }
        }
        foreach (float side in new[] { -1f, 1f })
        {
            var stripe = Shape(root, "Cyan heel stripe", PrimitiveType.Cube,
                new Vector3(side * 0.121f, 0.28f, -0.09f), new Vector3(0.015f, 0.12f, 0.055f), blue!);
            stripe.transform.localRotation = Quaternion.Euler(0, 0, side * 15);
        }
    }

    private static void AddBlade(Transform parent)
    {
        // Convex runner profile with an upturned toe, extruded into a thin steel blade.
        var profile = new[] { new Vector2(-0.24f, 0.04f), new Vector2(-0.20f, 0.008f),
            new Vector2(0.18f, 0.008f), new Vector2(0.28f, 0.05f), new Vector2(0.29f, 0.085f), new Vector2(-0.24f, 0.085f) };
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        Vector3 V(int i, float x) => new Vector3(x, profile[i].y, profile[i].x);
        void Tri(Vector3 a, Vector3 b, Vector3 c)
        {
            int index = vertices.Count;
            vertices.Add(a); vertices.Add(b); vertices.Add(c);
            triangles.Add(index); triangles.Add(index + 1); triangles.Add(index + 2);
        }
        for (int i = 1; i < profile.Length - 1; i++)
        {
            Tri(V(0, -0.018f), V(i, -0.018f), V(i + 1, -0.018f));
            Tri(V(0, 0.018f), V(i + 1, 0.018f), V(i, 0.018f));
        }
        for (int i = 0; i < profile.Length; i++)
        {
            int j = (i + 1) % profile.Length;
            Tri(V(i, -0.018f), V(j, 0.018f), V(j, -0.018f));
            Tri(V(i, -0.018f), V(i, 0.018f), V(j, 0.018f));
        }
        var mesh = new Mesh { name = "Curved skate runner" };
        mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        var blade = new GameObject("Curved steel blade", typeof(MeshFilter), typeof(MeshRenderer));
        blade.transform.SetParent(parent, false);
        blade.GetComponent<MeshFilter>().sharedMesh = mesh;
        blade.GetComponent<MeshRenderer>().sharedMaterial = metal!;
    }

    public static GameObject CreateProviderPrefab(bool held = false)
    {
        if (held ? heldRack != null : rack != null) return held ? heldRack! : rack!;
        EnsureMaterials();
        var result = new GameObject(held ? "Held Ice Skates rack" : "Ice Skates rack");
        result.transform.SetParent(Container, false);
        Block(result.transform, "Shelf", new Vector3(0, 0.40f, 0), new Vector3(0.85f, 0.10f, 0.7f), metal!);
        Block(result.transform, "Base", new Vector3(0, 0.08f, 0), new Vector3(0.85f, 0.16f, 0.7f), blue!);
        foreach (float x in new[] { -0.35f, 0.35f })
            Block(result.transform, "Upright", new Vector3(x, 0.27f, 0), new Vector3(0.08f, 0.4f, 0.55f), white!);
        var pair = Object.Instantiate(CreateWearablePrefab(), result.transform, false);
        pair.transform.localPosition = new Vector3(0, 0.46f, 0);
        if (!held)
        {
            var collider = result.AddComponent<BoxCollider>();
            collider.center = new Vector3(0, 0.38f, 0);
            collider.size = new Vector3(0.85f, 0.76f, 0.7f);
            var obstacle = result.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = collider.center;
            obstacle.size = collider.size;
            obstacle.carving = true;
            rack = result;
        }
        else heldRack = result;
        return result;
    }
}
