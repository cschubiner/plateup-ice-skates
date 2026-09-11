using IceSkates.Workshop.Runtime;
using Kitchen;

namespace IceSkates.Workshop.Systems;

public sealed class ResetSkatesOnLifecycleSystem : GenericSystemBase, KitchenMods.IModSystem
{
    private bool _hadPlayers;
    private bool wasDay;
    private bool wasRestaurant;

    protected override void OnUpdate()
    {
        bool hasPlayers = GetEntityQuery(typeof(CPlayer)).CalculateEntityCount() > 0;
        bool restaurant = Has<SKitchenMarker>() && !Has<SPerformSceneTransition>();
        bool day = Has<SIsDayTime>();
        if (restaurant != wasRestaurant || day != wasDay)
        {
            SkateRuntimeState.Reset("restaurant/day transition");
            foreach (var motion in UnityEngine.Object.FindObjectsOfType<SkateMotion>())
            {
                if (!restaurant) motion.SetEquipped(false);
                else motion.ResetMomentum();
            }
        }
        wasRestaurant = restaurant;
        wasDay = day;
        if (_hadPlayers && !hasPlayers)
        {
            SkateRuntimeState.Reset("all players despawned or mode changed");
        }

        _hadPlayers = hasPlayers;
    }
}
