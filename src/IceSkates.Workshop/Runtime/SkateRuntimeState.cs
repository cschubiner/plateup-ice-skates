using System.Collections.Generic;
using IceSkates.Core;
using KitchenIceSkates;

namespace IceSkates.Workshop.Runtime;

internal static class SkateRuntimeState
{
    private static readonly SkateEquipmentState Equipment = new();
    private static readonly Dictionary<int, SkateVector2> Velocities = new();

    public static bool IsEquipped(int playerId) => Equipment.IsEquipped(playerId);

    public static SkateVector2 GetVelocity(int playerId)
    {
        return Velocities.TryGetValue(playerId, out SkateVector2 velocity) ? velocity : SkateVector2.Zero;
    }

    public static void SetVelocity(int playerId, SkateVector2 velocity)
    {
        Velocities[playerId] = velocity;
    }

    public static void SetEquipped(int playerId, bool equipped, string reason)
    {
        SkateStateChange change = Equipment.Apply(playerId, equipped ? SkateEquipAction.Equip : SkateEquipAction.Unequip);
        if (change.Changed)
        {
            Mod.LogInfo($"Player {playerId} {(change.IsEquipped ? "equipped" : "unequipped")} Ice Skates ({reason}).");
        }

        if (!equipped)
        {
            Velocities.Remove(playerId);
        }
    }

    public static void MarkMovementActive(int playerId)
    {
        if (!Velocities.ContainsKey(playerId))
        {
            Mod.LogInfo($"Movement state activated for Ice Skates player {playerId}.");
        }
    }

    public static void CleanupMissingPlayers(IEnumerable<int> activePlayerIds)
    {
        foreach (SkateStateChange change in Equipment.CleanupMissingPlayers(activePlayerIds))
        {
            Velocities.Remove(change.PlayerId);
            Mod.LogInfo($"Cleaned Ice Skates state for missing player {change.PlayerId}.");
        }
    }

    public static void Reset(string reason)
    {
        foreach (SkateStateChange change in Equipment.ResetAll())
        {
            Mod.LogInfo($"Reset Ice Skates state for player {change.PlayerId} ({reason}).");
        }

        Velocities.Clear();
    }
}
