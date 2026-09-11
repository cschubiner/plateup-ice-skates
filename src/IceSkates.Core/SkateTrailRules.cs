using System;

namespace IceSkates.Core;

public static class SkateTrailRules
{
    public const float Lifetime = 1.15f;
    public const float Width = 0.11f;
    public const float MinimumSpeed = 0.15f;
    public const float TeleportDistance = 2.5f;
    public const float GroundOffset = 0.055f;
    public const float PointSpacing = 0.025f;

    public static bool ShouldClear(float distance) => !Finite(distance) || distance < 0 || distance > TeleportDistance;

    public static bool ShouldEmit(bool equipped, bool suspended, float distance, float deltaTime) =>
        equipped && !suspended && !ShouldClear(distance) && Finite(deltaTime) && deltaTime > 0 &&
        distance / deltaTime >= MinimumSpeed;

    public static bool KeepEmitting(bool alreadyEmitting, bool equipped, bool suspended, float distance, float deltaTime) =>
        equipped && !suspended && !ShouldClear(distance) &&
        (alreadyEmitting || ShouldEmit(equipped, suspended, distance, deltaTime));

    private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}
