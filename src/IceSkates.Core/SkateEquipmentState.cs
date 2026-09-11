using System;
using System.Collections.Generic;

namespace IceSkates.Core;

public enum SkateEquipAction
{
    Equip,
    Unequip,
    Toggle,
    Cleanup
}

public readonly record struct SkateStateChange(int PlayerId, bool WasEquipped, bool IsEquipped, SkateEquipAction Action)
{
    public bool Changed => WasEquipped != IsEquipped;
}

public sealed class SkateEquipmentState
{
    private readonly HashSet<int> _equippedPlayers = new();

    public IReadOnlyCollection<int> EquippedPlayers => _equippedPlayers;

    public bool IsEquipped(int playerId) => _equippedPlayers.Contains(playerId);

    public SkateStateChange Apply(int playerId, SkateEquipAction action)
    {
        bool before = IsEquipped(playerId);
        bool after = action switch
        {
            SkateEquipAction.Equip => true,
            SkateEquipAction.Unequip => false,
            SkateEquipAction.Toggle => !before,
            SkateEquipAction.Cleanup => false,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };

        if (after)
        {
            _equippedPlayers.Add(playerId);
        }
        else
        {
            _equippedPlayers.Remove(playerId);
        }

        return new SkateStateChange(playerId, before, after, action);
    }

    public IReadOnlyList<SkateStateChange> CleanupMissingPlayers(IEnumerable<int> activePlayerIds)
    {
        HashSet<int> active = new(activePlayerIds);
        List<SkateStateChange> changes = new();

        foreach (int playerId in new List<int>(_equippedPlayers))
        {
            if (!active.Contains(playerId))
            {
                changes.Add(Apply(playerId, SkateEquipAction.Cleanup));
            }
        }

        return changes;
    }

    public IReadOnlyList<SkateStateChange> ResetAll(SkateEquipAction action = SkateEquipAction.Cleanup)
    {
        List<SkateStateChange> changes = new();

        foreach (int playerId in new List<int>(_equippedPlayers))
        {
            changes.Add(Apply(playerId, action));
        }

        return changes;
    }
}
