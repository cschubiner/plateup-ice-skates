using System;

namespace IceSkates.Core;

public sealed record SkateTuning
{
    public const float DefaultFacingDegreesPerSecond = 360f;
    public const float DefaultSpeedMultiplier = 1.65f;
    public const float DefaultBaseTurnResponsiveness = 7.5f;
    public const float DefaultSkateTurnResponsiveness = 3.8f;
    public const float DefaultCoastResponsiveness = 1.7f;
    public const float DefaultMaxInputMagnitude = 1f;
    public const float DefaultMinimumDeltaTime = 0.001f;
    public const float DefaultMaximumDeltaTime = 0.08f;

    public float SpeedMultiplier { get; init; } = DefaultSpeedMultiplier;
    public float BaseTurnResponsiveness { get; init; } = DefaultBaseTurnResponsiveness;
    public float SkateTurnResponsiveness { get; init; } = DefaultSkateTurnResponsiveness;
    public float CoastResponsiveness { get; init; } = DefaultCoastResponsiveness;
    public float MaxInputMagnitude { get; init; } = DefaultMaxInputMagnitude;
    public float MinimumDeltaTime { get; init; } = DefaultMinimumDeltaTime;
    public float MaximumDeltaTime { get; init; } = DefaultMaximumDeltaTime;

    public static SkateTuning Default { get; } = new();

    public void Validate()
    {
        if (!Finite(SpeedMultiplier) || !Finite(BaseTurnResponsiveness) || !Finite(SkateTurnResponsiveness) ||
            !Finite(CoastResponsiveness) || !Finite(MaxInputMagnitude) || !Finite(MinimumDeltaTime) || !Finite(MaximumDeltaTime))
            throw new ArgumentOutOfRangeException(nameof(SpeedMultiplier), "Tuning must be finite.");
        if (SpeedMultiplier < 1f)
        {
            throw new ArgumentOutOfRangeException(nameof(SpeedMultiplier), "Ice Skates should not slow the player down.");
        }

        if (BaseTurnResponsiveness <= 0f || SkateTurnResponsiveness <= 0f || CoastResponsiveness <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(SkateTurnResponsiveness), "Responsiveness values must be positive.");
        }

        if (SkateTurnResponsiveness >= BaseTurnResponsiveness)
        {
            throw new ArgumentOutOfRangeException(nameof(SkateTurnResponsiveness), "Skate turning should be lower than base turning.");
        }

        if (MaxInputMagnitude <= 0f || MinimumDeltaTime <= 0f || MaximumDeltaTime < MinimumDeltaTime)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxInputMagnitude), "Input and delta-time bounds must be valid.");
        }
    }

    private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}
