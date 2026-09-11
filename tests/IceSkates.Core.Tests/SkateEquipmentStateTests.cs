using IceSkates.Core;
using Xunit;

namespace IceSkates.Core.Tests;

public sealed class SkateEquipmentStateTests
{
    [Fact]
    public void EquipAndUnequip_AffectOnlyRequestedPlayer()
    {
        SkateEquipmentState state = new();

        SkateStateChange equipped = state.Apply(1, SkateEquipAction.Equip);
        state.Apply(2, SkateEquipAction.Equip);
        SkateStateChange unequipped = state.Apply(1, SkateEquipAction.Unequip);

        Assert.True(equipped.Changed);
        Assert.True(unequipped.Changed);
        Assert.False(state.IsEquipped(1));
        Assert.True(state.IsEquipped(2));
    }

    [Fact]
    public void Toggle_SwitchesPlayerState()
    {
        SkateEquipmentState state = new();

        Assert.True(state.Apply(42, SkateEquipAction.Toggle).IsEquipped);
        Assert.False(state.Apply(42, SkateEquipAction.Toggle).IsEquipped);
    }

    [Fact]
    public void ResetAll_RemovesEveryEquippedPlayer()
    {
        SkateEquipmentState state = new();
        state.Apply(1, SkateEquipAction.Equip);
        state.Apply(2, SkateEquipAction.Equip);

        IReadOnlyList<SkateStateChange> changes = state.ResetAll();

        Assert.Equal(2, changes.Count);
        Assert.Empty(state.EquippedPlayers);
        Assert.All(changes, change => Assert.Equal(SkateEquipAction.Cleanup, change.Action));
    }

    [Fact]
    public void CleanupMissingPlayers_RemovesOnlyDespawnedPlayers()
    {
        SkateEquipmentState state = new();
        state.Apply(1, SkateEquipAction.Equip);
        state.Apply(2, SkateEquipAction.Equip);
        state.Apply(3, SkateEquipAction.Equip);

        IReadOnlyList<SkateStateChange> changes = state.CleanupMissingPlayers(new[] { 1, 3 });

        Assert.Single(changes);
        Assert.Equal(2, changes[0].PlayerId);
        Assert.True(state.IsEquipped(1));
        Assert.False(state.IsEquipped(2));
        Assert.True(state.IsEquipped(3));
    }
}
