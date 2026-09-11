using System.Collections.Generic;
using IceSkates.Workshop.GDO;
using IceSkates.Workshop.Runtime;
using Kitchen;
using KitchenData;
using KitchenIceSkates;
using KitchenLib.Utils;
using Unity.Collections;
using Unity.Entities;

namespace IceSkates.Workshop.Systems;

public sealed class TrackEquippedSkatesSystem : GenericSystemBase, KitchenMods.IModSystem
{
    private EntityQuery _players;

    protected override void Initialise()
    {
        base.Initialise();
        _players = GetEntityQuery(typeof(CPlayer));
        Mod.LogInfo("Ice Skates movement state system initialised.");
    }

    protected override void OnUpdate()
    {
        Item? skates = GDOUtils.GetCastedGDO<Item, IceSkatesItem>();
        if (skates == null)
        {
            return;
        }

        NativeArray<Entity> playerEntities = _players.ToEntityArray(Allocator.Temp);
        List<int> activePlayerIds = new(playerEntities.Length);

        try
        {
            foreach (Entity entity in playerEntities)
            {
                if (!Require(entity, out CPlayer player))
                {
                    continue;
                }

                activePlayerIds.Add(player.ID);
                bool hasSkates = TryGetCurrentToolItemId(entity, out int itemId) && itemId == skates.ID;
                SkateRuntimeState.SetEquipped(player.ID, hasSkates, "tool state sync");
            }

            SkateRuntimeState.CleanupMissingPlayers(activePlayerIds);
        }
        finally
        {
            playerEntities.Dispose();
        }
    }

    private bool TryGetCurrentToolItemId(Entity playerEntity, out int itemId)
    {
        itemId = 0;
        if (!Require(playerEntity, out CToolUser toolUser) || toolUser.CurrentTool == default)
        {
            return false;
        }

        if (!Require(toolUser.CurrentTool, out CItem item))
        {
            return false;
        }

        itemId = item.ID;
        return true;
    }
}
