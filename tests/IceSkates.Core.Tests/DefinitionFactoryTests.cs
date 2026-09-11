using IceSkates.Core;
using Xunit;

namespace IceSkates.Core.Tests;

public sealed class DefinitionFactoryTests
{
    [Fact]
    public void DefaultTuningValues_AreFasterButBounded()
    {
        SkateTuning tuning = SkateTuning.Default;

        tuning.Validate();
        Assert.True(tuning.SpeedMultiplier > 1f);
        Assert.True(tuning.SpeedMultiplier < 1.75f);
        Assert.True(tuning.SkateTurnResponsiveness < tuning.BaseTurnResponsiveness);
        Assert.True(tuning.CoastResponsiveness < tuning.SkateTurnResponsiveness);
    }

    [Fact]
    public void ApplianceDefinition_IsStarterFreeUniqueAndInteractable()
    {
        IceSkatesApplianceDefinition appliance = IceSkatesDefinitionFactory.CreateApplianceDefinition();

        Assert.True(appliance.IsStarter);
        Assert.True(appliance.IsFree);
        Assert.True(appliance.IsUnique);
        Assert.True(appliance.MovableDuringPrep);
        Assert.True(appliance.LockedDuringDay);
        Assert.True(appliance.IsInteractable);
        Assert.Equal(0, appliance.PurchaseCost);
    }

    [Fact]
    public void ItemDefinition_IsPerPlayerAndNotPersistentAcrossResets()
    {
        IceSkatesItemDefinition item = IceSkatesDefinitionFactory.CreateItemDefinition();

        Assert.True(item.AffectsOnlyHolder);
        Assert.False(item.PersistentAcrossResets);
        Assert.Equal("Ice Skates", item.DisplayName);
    }
}
