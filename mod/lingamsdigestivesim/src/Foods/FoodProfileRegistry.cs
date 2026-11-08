using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace lingamsdigestivesim.Foods;

/// <summary>
/// A compact, read-only-at-runtime catalog of food digestion profiles.
/// The CSV remains the human-editable source; foodprofiles.json is the game asset.
/// </summary>
public sealed class FoodProfileRegistry
{
    private readonly Dictionary<string, DigestiveFoodProfile> byKey =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DigestiveFoodProfile> byId =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DigestiveFoodProfile> byItemCode =
        new(StringComparer.OrdinalIgnoreCase);

    public int Count => byKey.Count;
    public int BoundItemCodeCount => byItemCode.Count;

    public IEnumerable<DigestiveFoodProfile> All => byKey.Values;

    public bool TryGetByKey(string key, out DigestiveFoodProfile profile) =>
        byKey.TryGetValue(key, out profile!);

    public bool TryGetById(string id, out DigestiveFoodProfile profile) =>
        byId.TryGetValue(id, out profile!);

    /// <summary>
    /// Only verified native item codes belong in this lookup. The first CSV has
    /// no verified codes; ingestion adapters must not guess matches by display name.
    /// </summary>
    public bool TryGetByItemCode(AssetLocation itemCode, out DigestiveFoodProfile profile) =>
        byItemCode.TryGetValue(itemCode.ToString(), out profile!);

    public static FoodProfileRegistry FromCatalog(DigestiveFoodCatalog catalog)
    {
        if (catalog.SchemaVersion != 1)
        {
            throw new InvalidOperationException($"Unsupported food profile schema {catalog.SchemaVersion} (expected 1).");
        }

        var registry = new FoodProfileRegistry();
        foreach (DigestiveFoodProfile profile in catalog.Foods ?? new List<DigestiveFoodProfile>())
        {
            if (string.IsNullOrWhiteSpace(profile.Id) || string.IsNullOrWhiteSpace(profile.Key)
                || profile.PortionQuantity <= 0 || profile.DelayGameHours < 0
                || profile.ProductionGameHours <= 0 || profile.GasUnitsPerPortion < 0)
            {
                throw new InvalidOperationException($"Invalid food profile '{profile.Id}' / '{profile.Key}'.");
            }

            if (!registry.byKey.TryAdd(profile.Key, profile) ||
                !registry.byId.TryAdd(profile.Id, profile))
            {
                throw new InvalidOperationException($"Duplicate food profile key or ID: {profile.Id} / {profile.Key}.");
            }

            if (!string.IsNullOrWhiteSpace(profile.ItemCode) &&
                !registry.byItemCode.TryAdd(profile.ItemCode, profile))
            {
                throw new InvalidOperationException($"Duplicate native item-code binding: {profile.ItemCode}.");
            }
        }

        if (registry.Count == 0)
        {
            throw new InvalidOperationException("Food profile catalog is empty.");
        }

        return registry;
    }
}

/// <summary>Deserialized from assets/lingamsdigestivesim/config/foodprofiles.json.</summary>
public sealed class DigestiveFoodCatalog
{
    public int SchemaVersion { get; set; }
    public string ProfileSet { get; set; } = "";
    public List<DigestiveFoodProfile> Foods { get; set; } = new();
    public List<DigestiveCompositeMeal> CompositeMeals { get; set; } = new();
    public List<DigestiveExcludedIntermediate> ExcludedIntermediates { get; set; } = new();
}

public sealed class DigestiveFoodProfile
{
    public string Id { get; set; } = "";
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public string NutritionCategory { get; set; } = "";
    public string DigestiveFamily { get; set; } = "";
    public string Preparation { get; set; } = "";
    public double PortionQuantity { get; set; }
    public string PortionUnit { get; set; } = "";
    public double DelayGameHours { get; set; }
    public double ProductionGameHours { get; set; }
    public double GasUnitsPerPortion { get; set; }
    public bool IngredientOnly { get; set; }

    /// <summary>Set only after the actual Vintage Story collectible code is verified.</summary>
    public string? ItemCode { get; set; }

    /// <summary>Gas potential corresponding to the actual quantity eaten, in PortionUnit.</summary>
    public double GasPotentialForQuantity(double consumedQuantity) =>
        Math.Max(0, consumedQuantity) / PortionQuantity * GasUnitsPerPortion;

    /// <summary>
    /// Total potential gas produced through a given game hour.
    /// The stomach is ONLY a delay; production begins afterwards and is linear.
    /// Save the last accounted amount and add ONLY the positive difference.
    /// No gas is consumed by a food contribution expiring.
    /// </summary>
    public double CumulativeGasAt(double mealClosedGameHour, double nowGameHour, double consumedQuantity)
    {
        if (ProductionGameHours <= 0) return 0;
        double hoursProducing = nowGameHour - mealClosedGameHour - DelayGameHours;
        double fraction = Math.Clamp(hoursProducing / ProductionGameHours, 0, 1);
        return GasPotentialForQuantity(consumedQuantity) * fraction;
    }
}

public sealed class DigestiveCompositeMeal
{
    public string Id { get; set; } = "";
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public string Resolution { get; set; } = "";
    public bool CountOnlyActuallyConsumedServing { get; set; }
}

public sealed class DigestiveExcludedIntermediate
{
    public string Id { get; set; } = "";
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
}