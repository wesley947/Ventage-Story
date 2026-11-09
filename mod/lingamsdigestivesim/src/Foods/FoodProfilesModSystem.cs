using System;
using System.Text.Json;
#if DEBUG
using HarmonyLib;
#endif
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace lingamsdigestivesim.Foods;

/// <summary>
/// Auto-discovered alongside the existing template ModSystem. Loading is intentionally
/// read-only: actual consumption tracking is a separate next development step.
/// </summary>
public sealed class FoodProfilesModSystem : ModSystem
{
    public FoodProfileRegistry? Registry { get; private set; }

#if DEBUG
    // Owned only by the server-side ModSystem instance. Never patch client
    // item definitions or serialize a custom collectible behavior.
    private Harmony? directFoodProbe;
#endif

    // Diagnostic only. Debug builds generate an audit of the actual registered vanilla
    // item/block codes so no one has to guess thousands of native food identifiers.
    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
#if DEBUG
        if (Registry != null)
        {
            VanillaFoodCodeAudit.Write(api, Registry);
            // Runtime observation only; no alterations to registered item types.
            directFoodProbe = DirectFoodIntakeHarmonyProbe.Install(api, Registry);
        }
#endif
    }

    public override void Dispose()
    {
#if DEBUG
        // In singleplayer client and server ModSystems share a process. Only
        // the server instance that owns the Harmony patch may unpatch it.
        if (directFoodProbe != null)
        {
            DirectFoodIntakeHarmonyProbe.Uninstall(directFoodProbe);
            directFoodProbe = null;
        }
#endif
        base.Dispose();
    }

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

            // An optional, independently reviewed native-code lookup can attach
            // many different game collectible variants to one digestive profile.
            // A bad binding file must not prevent the base 195 profiles from loading.
            IAsset? mappingAsset = api.Assets.TryGet(
                new AssetLocation("lingamsdigestivesim", "config/foodcodebindings.json"));
            if (mappingAsset != null)
            {
                try
                {
                    FoodCodeBindingCatalog? bindings = JsonSerializer.Deserialize<FoodCodeBindingCatalog>(
                        mappingAsset.ToText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (bindings == null) throw new InvalidOperationException("Empty native food-code bindings.");
                    Registry.AddVerifiedNativeCodes(bindings);
                }
                catch (Exception bindingError)
                {
                    api.Logger.Error("[lingamsdigestivesim] Native food-code bindings were not applied: {0}",
                        bindingError);
                }
            }

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