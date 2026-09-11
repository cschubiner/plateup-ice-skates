using Xunit;

namespace IceSkates.Core.Tests;

public class SkateTrailRulesTests
{
    [Fact]
    public void FasterDefaultStillLeavesTurningPenalty()
    {
        Assert.Equal(1.65f, SkateRules.GetSpeedMultiplier(true));
        Assert.True(SkateTuning.Default.SkateTurnResponsiveness < SkateTuning.Default.BaseTurnResponsiveness);
    }

    [Theory]
    [InlineData(true, false, 0.05f, 0.02f, true)]
    [InlineData(false, false, 0.05f, 0.02f, false)]
    [InlineData(true, true, 0.05f, 0.02f, false)]
    [InlineData(true, false, 0f, 0.02f, false)]
    [InlineData(true, false, 0.001f, 0.02f, false)]
    [InlineData(true, false, 4f, 0.02f, false)]
    [InlineData(true, false, 0.05f, 0f, false)]
    [InlineData(true, false, float.NaN, 0.02f, false)]
    [InlineData(true, false, 0.05f, float.NaN, false)]
    public void TrailEmissionIsMovementOnly(bool equipped, bool suspended, float distance, float dt, bool expected) =>
        Assert.Equal(expected, SkateTrailRules.ShouldEmit(equipped, suspended, distance, dt));

    [Theory]
    [InlineData(3f)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NaN)]
    public void TeleportsOrInvalidPositionsClearTrails(float distance) => Assert.True(SkateTrailRules.ShouldClear(distance));

    [Theory]
    [InlineData(true, true, false, 0f, true)]
    [InlineData(false, true, false, 0f, false)]
    [InlineData(false, true, false, 0.05f, true)]
    [InlineData(true, false, false, 0.05f, false)]
    [InlineData(true, true, true, 0.05f, false)]
    [InlineData(true, true, false, 3f, false)]
    [InlineData(true, true, false, float.NaN, false)]
    public void EmissionSurvivesIdleFramesButNotResets(bool emitting, bool equipped, bool suspended, float distance, bool expected) =>
        Assert.Equal(expected, SkateTrailRules.KeepEmitting(emitting, equipped, suspended, distance, 0.02f));
}
