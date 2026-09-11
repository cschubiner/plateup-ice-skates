using IceSkates.Core;
using Xunit;

namespace IceSkates.Core.Tests;

public sealed class SkateRulesTests
{
    [Fact]
    public void SpeedMultiplier_IsDefaultWhenEquipped()
    {
        Assert.Equal(SkateTuning.DefaultSpeedMultiplier, SkateRules.GetSpeedMultiplier(skatesEquipped: true));
        Assert.Equal(1f, SkateRules.GetSpeedMultiplier(skatesEquipped: false));
    }

    [Fact]
    public void TurnResponsiveness_IsLowerWithSkates()
    {
        float normal = SkateRules.GetTurnResponsiveness(skatesEquipped: false, hasInput: true);
        float skating = SkateRules.GetTurnResponsiveness(skatesEquipped: true, hasInput: true);
        float coasting = SkateRules.GetTurnResponsiveness(skatesEquipped: true, hasInput: false);

        Assert.Equal(SkateTuning.DefaultBaseTurnResponsiveness, normal);
        Assert.Equal(SkateTuning.DefaultSkateTurnResponsiveness, skating);
        Assert.Equal(SkateTuning.DefaultCoastResponsiveness, coasting);
        Assert.True(skating < normal);
        Assert.True(coasting < skating);
    }

    [Fact]
    public void MovementCalculation_IncreasesTopSpeedWhenEquipped()
    {
        SkateMovementOutput normal = SkateRules.CalculateMovement(
            SkateVector2.Zero,
            new SkateVector2(1f, 0f),
            baseTopSpeed: 2f,
            deltaTimeSeconds: 0.016f,
            skatesEquipped: false);

        SkateMovementOutput skating = SkateRules.CalculateMovement(
            SkateVector2.Zero,
            new SkateVector2(1f, 0f),
            baseTopSpeed: 2f,
            deltaTimeSeconds: 0.016f,
            skatesEquipped: true);

        Assert.Equal(2f, normal.TopSpeed);
        Assert.Equal(2f * SkateTuning.DefaultSpeedMultiplier, skating.TopSpeed, precision: 4);
    }

    [Fact]
    public void MovementCalculation_ProducesMoreInertiaWithSkates()
    {
        SkateVector2 current = new(2f, 0f);
        SkateVector2 turnLeft = new(0f, 1f);

        SkateMovementOutput normal = SkateRules.CalculateMovement(current, turnLeft, 2f, 0.016f, skatesEquipped: false);
        SkateMovementOutput skating = SkateRules.CalculateMovement(current, turnLeft, 2f, 0.016f, skatesEquipped: true);

        Assert.True(skating.Blend < normal.Blend);
        Assert.True(skating.Velocity.X > normal.Velocity.X);
        Assert.True(skating.Velocity.Y < normal.Velocity.Y);
    }

    [Fact]
    public void MovementCalculation_ClampsInputMagnitude()
    {
        SkateMovementOutput output = SkateRules.CalculateMovement(
            SkateVector2.Zero,
            new SkateVector2(50f, 50f),
            baseTopSpeed: 2f,
            deltaTimeSeconds: 1f,
            skatesEquipped: true);

        Assert.True(output.Velocity.Magnitude <= output.TopSpeed + 0.001f);
    }
}
