using System;

namespace IceSkates.Core;

public static class SkateRules
{
    public static float GetSpeedMultiplier(bool skatesEquipped, SkateTuning? tuning = null)
    {
        tuning ??= SkateTuning.Default;
        tuning.Validate();
        return skatesEquipped ? tuning.SpeedMultiplier : 1f;
    }

    public static float GetTurnResponsiveness(bool skatesEquipped, bool hasInput, SkateTuning? tuning = null)
    {
        tuning ??= SkateTuning.Default;
        tuning.Validate();

        if (!skatesEquipped)
        {
            return tuning.BaseTurnResponsiveness;
        }

        return hasInput ? tuning.SkateTurnResponsiveness : tuning.CoastResponsiveness;
    }

    public static SkateMovementOutput CalculateMovement(
        SkateVector2 currentVelocity,
        SkateVector2 rawInput,
        float baseTopSpeed,
        float deltaTimeSeconds,
        bool skatesEquipped,
        SkateTuning? tuning = null)
    {
        tuning ??= SkateTuning.Default;
        tuning.Validate();

        if (!Finite(rawInput.X) || !Finite(rawInput.Y) || !Finite(baseTopSpeed))
            return new SkateMovementOutput(SkateVector2.Zero, 0f, 0f, 0f);
        if (!Finite(currentVelocity.X) || !Finite(currentVelocity.Y)) currentVelocity = SkateVector2.Zero;
        float dt = Finite(deltaTimeSeconds) ? SkateMath.Clamp(deltaTimeSeconds, 0f, tuning.MaximumDeltaTime) : 0f;
        SkateVector2 input = rawInput.ClampMagnitude(tuning.MaxInputMagnitude);
        bool hasInput = input.Magnitude > 0.001f;
        float topSpeed = Math.Max(0f, baseTopSpeed) * GetSpeedMultiplier(skatesEquipped, tuning);
        SkateVector2 desiredVelocity = hasInput ? input * (topSpeed / tuning.MaxInputMagnitude) : SkateVector2.Zero;
        float responsiveness = GetTurnResponsiveness(skatesEquipped, hasInput, tuning);
        if (!skatesEquipped) return new SkateMovementOutput(desiredVelocity, topSpeed, responsiveness, 1f);
        float blend = 1f - (float)Math.Exp(-responsiveness * dt);
        SkateVector2 velocity = SkateVector2.Lerp(currentVelocity, desiredVelocity, blend).ClampMagnitude(topSpeed);

        return new SkateMovementOutput(velocity, topSpeed, responsiveness, blend);
    }

    private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}

public readonly record struct SkateMovementOutput(
    SkateVector2 Velocity,
    float TopSpeed,
    float Responsiveness,
    float Blend);
