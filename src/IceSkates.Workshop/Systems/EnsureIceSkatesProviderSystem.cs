using IceSkates.Workshop.GDO;
using Kitchen;
using KitchenData;
using KitchenLib.Utils;
using Unity.Collections;
using Unity.Entities;

namespace IceSkates.Workshop.Systems;

public sealed class EnsureIceSkatesProviderSystem : RestaurantSystem, KitchenMods.IModSystem
{
    private EntityQuery appliances;
    private EntityQuery pending;
    private EntityQuery parcels;

    protected override void Initialise()
    {
        base.Initialise();
        appliances = GetEntityQuery(typeof(CAppliance));
        pending = GetEntityQuery(typeof(CCreateAppliance));
        parcels = GetEntityQuery(typeof(CLetterAppliance));
    }

    protected override void OnUpdate()
    {
        if (!Has<SIsNightTime>() || !Has<SLayout>() || Has<SPerformSceneTransition>()) return;
        var provider = GDOUtils.GetCastedGDO<Appliance, IceSkatesProviderAppliance>();
        if (provider == null) return;
        using var existing = appliances.ToComponentDataArray<CAppliance>(Allocator.Temp);
        foreach (var value in existing) if (value.ID == provider.ID) return;
        using var queued = pending.ToComponentDataArray<CCreateAppliance>(Allocator.Temp);
        foreach (var value in queued) if (value.ID == provider.ID) return;
        using var letters = parcels.ToComponentDataArray<CLetterAppliance>(Allocator.Temp);
        foreach (var value in letters) if (value.ApplianceID == provider.ID) return;

        // Use the game's free parcel path. Never place on an occupied fallback tile.
        foreach (var tile in GetPostTiles())
        {
            if (TileManager.GetOccupant(tile) != Entity.Null || TileManager.GetTile(tile).HasFeature) continue;
            PostHelpers.CreateApplianceParcel(EntityManager, tile, provider.ID);
            KitchenIceSkates.Mod.LogInfo($"Delivered free Ice Skates rack parcel at {tile}.");
            return;
        }
    }
}
