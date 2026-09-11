using Controllers;
using HarmonyLib;
using IceSkates.Core;
using IceSkates.Workshop.GDO;
using IceSkates.Workshop.Runtime;
using Kitchen;
using KitchenLib.Utils;
using UnityEngine;

namespace IceSkates.Workshop.Patches;

// This vanilla view update already crosses the network. No host-only equipment cache.
[HarmonyPatch(typeof(PlayerHoldingSubview), "UpdateData", new[] { typeof(PlayerHoldingSubview.ViewData) })]
internal static class SkateToolViewPatch
{
    private static void Postfix(PlayerHoldingSubview __instance, PlayerHoldingSubview.ViewData view_data)
    {
        var player = __instance.GetComponentInParent<PlayerView>();
        if (player == null) return;
        var motion = player.GetComponent<SkateMotion>() ?? player.gameObject.AddComponent<SkateMotion>();
        var skates = GDOUtils.GetCustomGameDataObject<IceSkatesItem>();
        motion.SetEquipped(skates != null && view_data.UsingToolID == skates.ID);
    }
}

[HarmonyPatch(typeof(PlayerView), "UpdateData", new[] { typeof(PlayerView.ViewData) })]
internal static class SkatePausePatch
{
    private static void Postfix(PlayerView __instance, PlayerView.ViewData view_data)
    {
        __instance.GetComponent<SkateMotion>()?.SetSuspended(view_data.IsPaused || view_data.Inputs.IsCaptured || view_data.Inputs.IsDisconnected);
    }
}

[HarmonyPatch(typeof(PlayerView), nameof(PlayerView.SetPosition))]
internal static class SkateTeleportPatch
{
    private static void Postfix(PlayerView __instance, UpdateViewPositionData pos)
    {
        if (pos.Force) __instance.GetComponent<SkateMotion>()?.ResetMomentum();
    }
}

[HarmonyPatch(typeof(PlayerWalkingComponent), nameof(PlayerWalkingComponent.UpdateMovement))]
internal static class PlayerWalkingComponentPatch
{
    private static bool Prefix(PlayerWalkingComponent __instance, bool is_my_player, int player_id,
        CInputData inputs, float speed_factor, float base_speed, Transform ___RootTransform,
        Rigidbody ___Rigidbody, Animator ___Animator)
    {
        var player = __instance.GetComponentInParent<PlayerView>();
        var motion = player == null ? null : player.GetComponent<SkateMotion>();
        if (motion == null || !motion.Equipped) return true;
        // Only the owning client contributes motion; PlateUp transmits its position to peers.
        if (!is_my_player) return true;
        if (___RootTransform == null || ___Rigidbody == null) return true;
        motion.SetMovementRoot(___RootTransform);
        var position = ___RootTransform.position;
        if (Mathf.Abs(position.y) > 0.001f)
        {
            position.y = 0f;
            ___RootTransform.position = position;
        }

        var state = inputs.State;
        if (!inputs.IsCaptured && !inputs.IsDisconnected && InputSourceIdentifier.DefaultInputSource != null &&
            InputSourceIdentifier.DefaultInputSource.GetCurrentInputData(player_id, out var local)) state = local;
        bool blocked = inputs.IsCaptured || inputs.IsDisconnected || ___Rigidbody.isKinematic ||
            state.StopMoving == ButtonState.Held || state.StopMoving == ButtonState.Pressed;
        if (blocked)
        {
            motion.ResetMomentum();
            __instance.IsMoving = false;
            if (___Animator != null) ___Animator.SetFloat("MovementSpeed", 0);
            return false;
        }

        var input = state.Movement;
        if (input.magnitude <= 0.5f) input = Vector2.zero;
        var camera = Camera.main;
        var forward = Vector3.ProjectOnPlane(camera != null ? camera.transform.forward : Vector3.forward, Vector3.up).normalized;
        var right = Vector3.ProjectOnPlane(camera != null ? camera.transform.right : Vector3.right, Vector3.up).normalized;
        var desired = forward * input.y + right * input.x;
        var result = SkateRules.CalculateMovement(motion.Steering, new SkateVector2(desired.x, desired.z), 1f, Time.deltaTime, true);
        motion.Steering = result.Velocity;
        var direction = new Vector3(result.Velocity.X, 0, result.Velocity.Y);
        __instance.IsMoving = direction.sqrMagnitude > 0.0001f;
        if (__instance.IsMoving)
        {
            ___RootTransform.rotation = Quaternion.RotateTowards(___RootTransform.rotation,
                Quaternion.LookRotation(direction, Vector3.up), SkateTuning.DefaultFacingDegreesPerSecond * Time.deltaTime);
            // base_speed is a FORCE scale in PlateUp, not metres/second. Preserve vanilla drag/collisions.
            ___Rigidbody.AddForce(direction * (base_speed * speed_factor * Mathf.Min(Time.deltaTime, SkateTuning.DefaultMaximumDeltaTime)));
        }
        if (___Animator != null) ___Animator.SetFloat("MovementSpeed", __instance.IsMoving ? 1f : 0f);
        return false;
    }
}
