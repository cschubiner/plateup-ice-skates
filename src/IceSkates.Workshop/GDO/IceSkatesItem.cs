using System.Collections.Generic;
using IceSkates.Core;
using IceSkates.Workshop.Helpers;
using Kitchen;
using KitchenData;
using KitchenIceSkates;
using KitchenLib.Customs;
using UnityEngine;

namespace IceSkates.Workshop.GDO;

public sealed class IceSkatesItem : CustomItem
{
    public override string UniqueNameID => IceSkatesDefinitionFactory.CreateItemDefinition().UniqueNameId;
    public override GameObject Prefab => PrefabFactory.CreateSkatesItemPrefab();
    public override GameObject SidePrefab => PrefabFactory.CreateSkatesItemPrefab();
    public override ItemCategory ItemCategory { get; protected set; } = ItemCategory.Generic;
    public override ToolAttachPoint HoldPose => ToolAttachPoint.NoPose;
    public override List<IItemProperty> Properties => PropertyFactory.CreateItemProperties();

    public override void OnRegister(Item gameDataObject)
    {
        base.OnRegister(gameDataObject);
        Mod.LogInfo($"Registered Ice Skates item GDO: {gameDataObject.ID}");
    }
}
