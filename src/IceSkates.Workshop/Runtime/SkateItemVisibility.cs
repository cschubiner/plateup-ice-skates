using Kitchen;
using UnityEngine;

namespace IceSkates.Workshop.Runtime;

// PlateUp keeps a native item view in its tool container even for NoPose tools.
// Hide only this item's geometry while parented to a chef, never their food or rig.
public sealed class SkateItemVisibility : MonoBehaviour
{
    private Renderer[] renderers = System.Array.Empty<Renderer>();
    private bool? visible;

    private void Awake() => renderers = GetComponentsInChildren<Renderer>(true);
    private void OnEnable() => Refresh();
    private void LateUpdate() => Refresh();

    internal void Refresh()
    {
        bool next = GetComponentInParent<PlayerView>() == null;
        if (visible == next) return;
        visible = next;
        foreach (var renderer in renderers) if (renderer != null) renderer.enabled = next;
    }
}
