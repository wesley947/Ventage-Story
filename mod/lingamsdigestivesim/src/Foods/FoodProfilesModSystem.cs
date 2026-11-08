using System;
using System.Text.Json;
using Vintagestory.API.Common;

namespace lingamsdigestivesim.Foods;

/// <summary>
/// Auto-discovered alongside the existing template ModSystem. Loading is intentionally
/// read-only: actual consumption tracking is a separate next development step.
/// </summary>
public sealed class FoodProfilesModSystem : ModSystem
{
    public FoodProfileRegistry? Registry { get; private set; }

    public override void AssetsLoaded(ICoreAPI api)
    {
        base.AssetsLoaded(api);
        AssetLocation path = new("lingamsdigestivesim", "config/foodprofiles.json");
        IAsset? asset = api.Assets.TryGet(path);
        if (asset == null)
        {
            api.Logger.Error("[lingamsdigestivesim] Missing food profile asset {0}. Check the build's assets folder.", path);
            return;
        }

        try
        {
            DigestiveFoodCatalog? catalog = JsonSerializer.Deserialize<DigestiveFoodCatalog>(
                asset.ToText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (catalog == null) throw new InvalidOperationException("Catalog JSON is empty or invalid.");
            Registry = FoodProfileRegistry.FromCatalog(catalog);
            api.Logger.Notification(
                "[lingamsdigestivesim] Loaded {0} food profiles, {1} composite meal rules and {2} excluded intermediates ({3} verified item-code bindings).",
                Registry.Count, catalog.CompositeMeals.Count, catalog.ExcludedIntermediates.Count,
                Registry.BoundItemCodeCount);
        }
        catch (Exception error)
        {
            Registry = null;
            api.Logger.Error("[lingamsdigestivesim] Unable to load food profiles: {0}", error);
        }
    }
}