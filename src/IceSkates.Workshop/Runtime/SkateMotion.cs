using IceSkates.Core;
using IceSkates.Workshop.Helpers;
using UnityEngine;

namespace IceSkates.Workshop.Runtime;

public sealed class SkateMotion : MonoBehaviour
{
    public bool Equipped { get; private set; }
    public SkateVector2 Steering;
    private GameObject? boots;
    private TrailRenderer[] trails = System.Array.Empty<TrailRenderer>();
    private Vector3 previousPosition;
    private bool suspended;
    private Transform? movementRoot;
    private bool loggedTrail;

    private Transform MovementRoot => movementRoot != null ? movementRoot : transform;

    internal void SetMovementRoot(Transform root)
    {
        if (movementRoot == root) return;
        movementRoot = root;
        ResetMomentum();
    }

    public void SetEquipped(bool equipped)
    {
        if (Equipped == equipped) return;
        Equipped = equipped;
        loggedTrail = false;
        ResetMomentum();
        if (boots == null && equipped)
        {
            boots = Object.Instantiate(PrefabFactory.CreateWearablePrefab(), transform, false);
            boots.name = "Equipped Ice Skates";
            boots.transform.localPosition = new Vector3(0, 0.015f, 0);
            trails = new[] { TrailFactory.Create(transform, -1), TrailFactory.Create(transform, 1) };
        }
        if (boots != null) boots.SetActive(equipped);
        KitchenIceSkates.Mod.LogInfo($"Player view {GetInstanceID()} {(equipped ? "equipped; movement active" : "unequipped; movement reset")}.");
    }

    public void SetSuspended(bool value)
    {
        suspended = value;
        if (value) ResetMomentum();
    }

    public void ResetMomentum()
    {
        Steering = SkateVector2.Zero;
        previousPosition = MovementRoot.position;
        foreach (var trail in trails)
        {
            if (trail == null) continue;
            trail.emitting = false;
            trail.Clear();
        }
    }

    private void LateUpdate()
    {
        var root = MovementRoot;
        float distance = Vector3.ProjectOnPlane(root.position - previousPosition, Vector3.up).magnitude;
        previousPosition = root.position;
        bool clear = SkateTrailRules.ShouldClear(distance);
        for (int i = 0; i < trails.Length; i++)
        {
            var trail = trails[i];
            if (trail == null) continue;
            var point = root.TransformPoint(new Vector3(i == 0 ? -0.17f : 0.17f, 0, -0.14f));
            // Restaurant views can live under a translated scene container. World Y=0
            // is not necessarily the floor. Keep the stroke just above the chef's base.
            point.y = root.position.y + SkateTrailRules.GroundOffset;
            trail.transform.position = point;
            trail.transform.rotation = Quaternion.Euler(90, 0, 0);
            trail.gameObject.layer = root.gameObject.layer;
            // Arm on movement, then let Unity's distance threshold sample continuously.
            // Toggling off on idle render frames loses physics-step motion; adding points
            // to a never-emitting trail builds a mesh but does not render in this runtime.
            trail.emitting = SkateTrailRules.KeepEmitting(trail.emitting, Equipped, suspended, distance, Time.deltaTime);
            if (clear) trail.Clear();
            Diagnostics.LiveTrailCapture.TryCapture(this, trail);
            if (!loggedTrail && trail.positionCount > 2)
            {
                loggedTrail = true;
                KitchenIceSkates.Mod.LogInfo($"Ice streaks active for view {GetInstanceID()}: root={root.position}, stroke={point}, layer={trail.gameObject.layer}.");
            }
        }
    }

    private void OnDisable() => SetEquipped(false);
}
