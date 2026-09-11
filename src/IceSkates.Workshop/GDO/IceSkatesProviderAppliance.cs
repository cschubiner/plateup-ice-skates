using System.Collections.Generic;
using IceSkates.Core;
using IceSkates.Workshop.Helpers;
using Kitchen;
using KitchenData;
using KitchenIceSkates;
using KitchenLib.Customs;
using KitchenLib.Utils;
using UnityEngine;

namespace IceSkates.Workshop.GDO;

public sealed class IceSkatesProviderAppliance : CustomAppliance
{
    private static readonly IceSkatesApplianceDefinition Definition = IceSkatesDefinitionFactory.CreateApplianceDefinition();

    public override string UniqueNameID => Definition.UniqueNameId;
    public override List<(Locale, ApplianceInfo)> InfoList => new() { (Locale.English, PropertyFactory.CreateInfo()) };
    public override GameObject Prefab => PrefabFactory.CreateProviderPrefab();
    public override GameObject HeldAppliancePrefab => PrefabFactory.CreateProviderPrefab(held: true);
    public override bool IsPurchasable { get; protected set; } = false;
    public override bool StapleWhenMissing { get; protected set; } = false;
    public override bool SellOnlyAsUnique { get; protected set; } = true;
    public override bool PreventSale { get; protected set; } = true;
    public override PriceTier PriceTier { get; protected set; } = PriceTier.Free;
    public override int PurchaseCostOverride { get; protected set; } = 0;
    public override List<IApplianceProperty> Properties { get; protected set; } = new();

    public override void AttachDependentProperties(GameData gameData, GameDataObject gameDataObject)
    {
        Item? item = GDOUtils.GetCastedGDO<Item, IceSkatesItem>();
        if (item != null)
        {
            Properties = PropertyFactory.CreateProviderProperties(item);
        }
        else
        {
            throw new System.InvalidOperationException("Ice Skates item GDO missing during provider attachment.");
        }

        base.AttachDependentProperties(gameData, gameDataObject);
    }

    public override void OnRegister(Appliance gameDataObject)
    {
        base.OnRegister(gameDataObject);
        Mod.LogInfo($"Registered Ice Skates provider appliance GDO: {gameDataObject.ID}");
    }
}
