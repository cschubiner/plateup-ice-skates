using System;
using System.Linq;
using HarmonyLib;
using IceSkates.Workshop.GDO;
using IceSkates.Workshop.Helpers;
using IceSkates.Workshop.Runtime;
using Kitchen;
using KitchenData;
using KitchenLib.Utils;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Mod = KitchenIceSkates.Mod;

namespace IceSkates.Workshop.Diagnostics;

internal static class RuntimeChecks
{
    private static bool scheduled;

    public static void Schedule()
    {
        if (scheduled) return;
        scheduled = true;
        var runner = new GameObject("Ice Skates opt-in diagnostics");
        UnityEngine.Object.DontDestroyOnLoad(runner);
        runner.AddComponent<RuntimeCheckRunner>().PreviousBackground = Application.runInBackground;
        Application.runInBackground = true;
    }

    public static void Run()
    {
        try
        {
            var item = GDOUtils.GetCastedGDO<Item, IceSkatesItem>();
            var rack = GDOUtils.GetCastedGDO<Appliance, IceSkatesProviderAppliance>();
            Check(item != null && rack != null, "GDOs exist");
            if (item == null || rack == null) return;
            var provider = rack.Properties.OfType<CItemProvider>().Single();
            Check(provider.DefaultProvidedItem == item.ID, "serialized provider ID matches skates");
            using (var world = new World("IceSkates isolated attachment check"))
            using (var buffer = new EntityCommandBuffer(Allocator.TempJob))
            {
                var entity = world.EntityManager.CreateEntity();
                provider.Attach(world.EntityManager, buffer, entity);
                buffer.Playback(world.EntityManager);
                var attached = world.EntityManager.GetComponentData<CItemProvider>(entity);
                Check(attached.ProvidedItem == item.ID && attached.HasAvailableItems(), "actual ECS Attach preserves dispensable item");
                Check(attached.Maximum == 0 && !attached.PreventReturns, "unlimited supply and returns enabled");
                var parcel = PostHelpers.CreateApplianceParcel(world.EntityManager, Vector3.zero, rack.ID);
                Check(world.EntityManager.GetComponentData<CLetterAppliance>(parcel).ApplianceID == rack.ID,
                    "native parcel carries rack ID used for duplicate detection");
                Check(world.EntityManager.HasComponent<CPreventStartDayPost>(parcel), "rack delivery must be opened before service");
                CheckNativeTransfers(world, entity, item.ID);
            }
            Check(item.Properties.OfType<CEquippableTool>().Single().CanHoldItems, "native tool leaves hands free");
            Check(rack.PriceTier == PriceTier.Free && !rack.IsPurchasable && rack.SellOnlyAsUnique, "free unique rack config");
            Check(rack.Prefab != null && rack.HeldAppliancePrefab != null && item.Prefab != null, "all prefab references assigned");
            if (rack.Prefab == null || rack.HeldAppliancePrefab == null || item.Prefab == null) return;
            Check(ReferenceEquals(PrefabFactory.CreateSkatesItemPrefab(), PrefabFactory.CreateSkatesItemPrefab()), "prefabs cached");
            Check(rack.HeldAppliancePrefab.GetComponentsInChildren<Collider>(true).All(c => !c.enabled), "held rack has no active collider");
            Check(item.Prefab.GetComponentsInChildren<Renderer>(true).All(r => r.sharedMaterial != null && r.sharedMaterial.shader.isSupported), "skate materials supported");
            CheckPresentation();
            foreach (var target in new[] {
                AccessTools.Method(typeof(PlayerWalkingComponent), nameof(PlayerWalkingComponent.UpdateMovement)),
                AccessTools.Method(typeof(PlayerHoldingSubview), "UpdateData", new[] { typeof(PlayerHoldingSubview.ViewData) }),
                AccessTools.Method(typeof(PlayerView), "UpdateData", new[] { typeof(PlayerView.ViewData) }),
                AccessTools.Method(typeof(PlayerView), nameof(PlayerView.SetPosition)) })
            {
                var patches = Harmony.GetPatchInfo(target);
                Check(patches != null && patches.Prefixes.Concat(patches.Postfixes)
                    .Any(p => p.PatchMethod.DeclaringType?.Assembly == typeof(RuntimeChecks).Assembly),
                    "Harmony patch installed: " + target.DeclaringType?.Name + "." + target.Name);
            }
            Mod.LogInfo("SELFTEST PASS: GDOs, ECS provider/parcel/equip/return, tool properties, pricing, prefabs, materials, Harmony patches.");
        }
        catch (Exception error)
        {
            Mod.LogError("SELFTEST FAIL: " + error);
        }
    }

    private static void CheckPresentation()
    {
        var chef = new GameObject("Isolated feet-only visibility check");
        chef.SetActive(false);
        var view = chef.AddComponent<PlayerView>();
        view.enabled = false;
        var nativeTool = UnityEngine.Object.Instantiate(PrefabFactory.CreateSkatesItemPrefab(), chef.transform, false);
        var motion = chef.AddComponent<SkateMotion>();
        try
        {
            chef.SetActive(true);
            nativeTool.GetComponent<SkateItemVisibility>().Refresh();
            Check(nativeTool.GetComponentsInChildren<Renderer>(true).All(r => !r.enabled), "native hand tool geometry hidden on chef");
            nativeTool.transform.SetParent(null, false);
            nativeTool.GetComponent<SkateItemVisibility>().Refresh();
            Check(nativeTool.GetComponentsInChildren<Renderer>(true).All(r => r.enabled), "loose skates become visible after unparenting");
            motion.SetEquipped(true);
            Check(chef.GetComponentsInChildren<SkateItemVisibility>(true).Length == 0, "wearable model has no hand-item visibility component");
            Check(chef.GetComponentsInChildren<MeshRenderer>().Length > 0, "foot boots remain visible while equipped");
            var trails = chef.GetComponentsInChildren<TrailRenderer>();
            Check(trails.Length == 2 && trails.All(t => t.sharedMaterial.shader.isSupported && t.alignment == LineAlignment.TransformZ), "two supported ground-aligned trails");
            foreach (var trail in trails) trail.AddPosition(Vector3.one);
            motion.ResetMomentum();
            Check(trails.All(t => t.positionCount == 0 && !t.emitting), "momentum reset clears trail history");
            motion.SetEquipped(false);
            Check(chef.GetComponentsInChildren<MeshRenderer>().Length == 0, "unequip removes visible foot boots");
            Check(PrefabFactory.CreateWearablePrefab().GetComponentsInChildren<Renderer>(true)
                .All(r => r.sharedMaterial.shader.name == "Simple Flat" && r.sharedMaterial.HasProperty("_Color0")),
                "boot materials use actual native color property instead of counter material");
        }
        finally
        {
            UnityEngine.Object.Destroy(nativeTool);
            UnityEngine.Object.Destroy(chef);
        }
    }

    private static void CheckNativeTransfers(World world, Entity rack, int itemID)
    {
        var em = world.EntityManager;
        var ctx = new EntityContext(em);
        var take = world.GetOrCreateSystem<TakeFromProvider>();
        var equip = world.GetOrCreateSystem<AcceptIntoIntoToolUser>();
        var unequip = world.GetOrCreateSystem<TakeFromToolUser>();
        var accept = world.GetOrCreateSystem<AcceptIntoProvider>();
        foreach (var system in new GenericSystemBase[] { take, equip, unequip, accept }) system.PostInitialisation();
        var chef = em.CreateEntity(typeof(CToolUser), typeof(CItemHolder));
        var held = em.CreateEntity();
        ctx.Set(chef, new CItemHolder { HeldItem = held });
        TakeFromProvider.CreateProposal(take, ctx, rack, chef);
        using var proposals = em.CreateEntityQuery(typeof(CItemTransferProposal));
        Check(proposals.CalculateEntityCount() == 1, "native provider creates one transfer proposal");
        var transfer = proposals.GetSingletonEntity();
        var data = em.GetComponentData<CItemTransferProposal>(transfer);
        Check(em.GetComponentData<CItem>(data.Item).ID == itemID && em.HasComponent<CEquippableTool>(data.Item),
            "native provider creates equippable skate entity");
        equip.Update();
        using var acceptances = em.CreateEntityQuery(typeof(CItemTransferAccept));
        Check(acceptances.CalculateEntityCount() == 1, "native equip accepts skates with hands occupied");
        equip.AcceptTransfer(transfer, acceptances.GetSingletonEntity(), ctx, out _);
        take.SendTransfer(transfer, acceptances.GetSingletonEntity(), ctx);
        Check(em.GetComponentData<CToolUser>(chef).CurrentTool == data.Item &&
            em.GetComponentData<CItemHolder>(chef).HeldItem == held, "equip fills only tool slot");
        em.DestroyEntity(acceptances);
        data.Source = chef;
        data.Destination = rack;
        ctx.Set(transfer, data);
        accept.Update();
        Check(acceptances.CalculateEntityCount() == 1, "native rack accepts returned skates");
        unequip.SendTransfer(transfer, acceptances.GetSingletonEntity(), ctx);
        accept.AcceptTransfer(transfer, acceptances.GetSingletonEntity(), ctx, out _);
        Check(em.GetComponentData<CToolUser>(chef).CurrentTool == Entity.Null && !em.Exists(data.Item),
            "native return clears equipment and destroys returned pair");
        Check(em.GetComponentData<CItemProvider>(rack).HasAvailableItems(), "rack remains stocked after equip/return");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        Mod.LogInfo("SELFTEST OK: " + message);
    }
}

internal sealed class RuntimeCheckRunner : MonoBehaviour
{
    internal bool PreviousBackground;

    private System.Collections.IEnumerator Start()
    {
        // Wait for the game-data callback to finish assigning GameData.Main.
        yield return null;
        try
        {
            RuntimeChecks.Run();
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "--iceskates-visual-check") >= 0)
            {
                var visual = VisualChecks.Capture();
                try
                {
                    while (true)
                    {
                        bool next;
                        try { next = visual.MoveNext(); }
                        catch (Exception error) { Mod.LogError("VISUAL CHECK FAIL: " + error); break; }
                        if (!next) break;
                        yield return visual.Current;
                    }
                }
                finally { (visual as IDisposable)?.Dispose(); }
            }
        }
        finally
        {
            Application.runInBackground = PreviousBackground;
            Destroy(gameObject);
        }
    }
}
