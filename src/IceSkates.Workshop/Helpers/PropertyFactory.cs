using System.Collections.Generic;
using IceSkates.Core;
using Kitchen;
using KitchenData;
using UnityEngine;

namespace IceSkates.Workshop.Helpers;

internal static class PropertyFactory
{
    public static List<IApplianceProperty> CreateProviderProperties(Item item)
    {
        // Attach() restores the private default ID, NOT ProvidedItem. InfiniteItemProvider
        // only sets the latter and becomes an empty provider when attached.
        CItemProvider provider = CItemProvider.EditorCreateProvider(item);
        provider.SetAsItem(item.ID);
        provider.Maximum = 0;
        provider.Available = -1;
        provider.PreventReturns = false;
        return new() { provider };
    }

    public static List<IItemProperty> CreateItemProperties() => new()
    {
        new CEquippableTool { CanHoldItems = true }
    };

    public static ApplianceInfo CreateInfo()
    {
        var definition = IceSkatesDefinitionFactory.CreateApplianceDefinition();
        var info = ScriptableObject.CreateInstance<ApplianceInfo>();
        info.Name = definition.DisplayName;
        info.Description = "Faster feet, slippery turns. Take a pair; use the rack to return them.";
        return info;
    }
}
