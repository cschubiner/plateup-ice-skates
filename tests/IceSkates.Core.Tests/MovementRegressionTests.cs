using Xunit;

namespace IceSkates.Core.Tests;

public class MovementRegressionTests
{
    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    [InlineData(float.NaN)]
    public void InvalidOrPausedDeltaCannotAccelerate(float dt)
    {
        var result = SkateRules.CalculateMovement(SkateVector2.Zero, new SkateVector2(1, 0), 1, dt, true);
        Assert.Equal(SkateVector2.Zero, result.Velocity);
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    public void InvalidInputCannotPoisonMomentum(float value)
    {
        var result = SkateRules.CalculateMovement(new SkateVector2(1, 0), new SkateVector2(value, 0), 1, 0.02f, true);
        Assert.Equal(SkateVector2.Zero, result.Velocity);
    }

    [Fact]
    public void UnequipRemovesInertiaImmediately()
    {
        var result = SkateRules.CalculateMovement(new SkateVector2(1, 0), new SkateVector2(0, 1), 1, 0.02f, false);
        Assert.Equal(new SkateVector2(0, 1), result.Velocity);
    }

    [Fact]
    public void AnalogInputRetainsMagnitude()
    {
        var result = SkateRules.CalculateMovement(SkateVector2.Zero, new SkateVector2(0.5f, 0), 1, 0.02f, true);
        var full = SkateRules.CalculateMovement(SkateVector2.Zero, new SkateVector2(1, 0), 1, 0.02f, true);
        Assert.Equal(full.Velocity.X / 2, result.Velocity.X, 5);
    }

    [Fact]
    public void ConstantInputIsFrameRateIndependent()
    {
        SkateVector2 Simulate(int frames)
        {
            var velocity = SkateVector2.Zero;
            for (int i = 0; i < frames; i++)
                velocity = SkateRules.CalculateMovement(velocity, new SkateVector2(1, 0), 1, 1f / frames, true).Velocity;
            return velocity;
        }
        Assert.Equal(Simulate(30).X, Simulate(120).X, 4);
    }

    [Fact]
    public void ReleasingInputEventuallyStops()
    {
        var velocity = new SkateVector2(1.32f, 0);
        for (int i = 0; i < 600; i++)
            velocity = SkateRules.CalculateMovement(velocity, SkateVector2.Zero, 1, 1f / 60, true).Velocity;
        Assert.True(velocity.Magnitude < 0.001f);
    }

    [Fact]
    public void NonFiniteTuningIsRejected()
    {
        Assert.Throws<System.ArgumentOutOfRangeException>(() => new SkateTuning { SpeedMultiplier = float.NaN }.Validate());
    }
}
