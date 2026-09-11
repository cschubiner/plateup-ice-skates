using System;

namespace IceSkates.Core;

public sealed record IceSkatesApplianceDefinition
{
    public string UniqueNameId { get; init; } = "clay.ice_skates.provider";
    public string DisplayName { get; init; } = "Ice Skates";
    public string Description { get; init; } = "Equip for faster, slippery movement. Interact again to remove.";
    public bool IsStarter { get; init; } = true;
    public bool IsFree { get; init; } = true;
    public bool IsUnique { get; init; } = true;
    public bool MovableDuringPrep { get; init; } = true;
    public bool LockedDuringDay { get; init; } = true;
    public bool IsInteractable { get; init; } = true;
    public int PurchaseCost { get; init; } = 0;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(UniqueNameId))
        {
            throw new ArgumentException("A stable appliance UniqueNameId is required.", nameof(UniqueNameId));
        }

        if (!IsStarter || !IsFree || !IsUnique)
        {
            throw new ArgumentException("v1 Ice Skates are intended to be starter, free, and unique.");
        }

        if (!MovableDuringPrep || !LockedDuringDay || !IsInteractable)
        {
            throw new ArgumentException("The provider must be prep-movable, day-locked, and interactable.");
        }

        if (PurchaseCost != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(PurchaseCost), "Starter Ice Skates should be free.");
        }
    }
}

public sealed record IceSkatesItemDefinition
{
    public string UniqueNameId { get; init; } = "clay.ice_skates.item";
    public string DisplayName { get; init; } = "Ice Skates";
    public string Description { get; init; } = "A personal movement tool for one slippery chef.";
    public bool AffectsOnlyHolder { get; init; } = true;
    public bool PersistentAcrossResets { get; init; } = false;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(UniqueNameId))
        {
            throw new ArgumentException("A stable item UniqueNameId is required.", nameof(UniqueNameId));
        }

        if (!AffectsOnlyHolder || PersistentAcrossResets)
        {
            throw new ArgumentException("Skates must be per-player and reset cleanly.");
        }
    }
}

public static class IceSkatesDefinitionFactory
{
    public static IceSkatesApplianceDefinition CreateApplianceDefinition()
    {
        IceSkatesApplianceDefinition definition = new();
        definition.Validate();
        return definition;
    }

    public static IceSkatesItemDefinition CreateItemDefinition()
    {
        IceSkatesItemDefinition definition = new();
        definition.Validate();
        return definition;
    }
}
