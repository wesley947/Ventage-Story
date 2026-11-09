using System;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace lingamsdigestivesim.Foods;

/// <summary>
/// DEBUG ONLY: observes ordinary vanilla held-item eating without replacing
/// the game's nutrition, interaction, or item-removal logic.
///
/// OnHeldInteractStop is invoked by the base CollectibleObject before its
/// default tryEatStop. A next-tick check ensures the held stack was actually
/// reduced before we log a likely successful direct-food bite.
///
/// This does not catch meals/pies that override OnHeldInteractStop, fluids,
/// or special consumption paths. It never queues meals or creates gas.
/// </summary>
public sealed class DirectFoodIntakeProbeBehavior : CollectibleBehavior
{
    private readonly ICoreServerAPI serverApi;
    private readonly FoodProfileRegistry registry;

    public DirectFoodIntakeProbeBehavior(
        CollectibleObject collectible,
        ICoreServerAPI api,
        FoodProfileRegistry foodRegistry) : base(collectible)
    {
        serverApi = api;
        registry = foodRegistry;
    }

    public override void OnHeldInteractStop(
        float secondsUsed,
        ItemSlot slot,
        EntityAgent byEntity,
        BlockSelection blockSel,
        EntitySelection entitySel,
        ref EnumHandling handling)
    {
        // Passive: do NOT set handling, block vanilla interaction, or change inventory.
        // The default eating path requires >=0.95 seconds of interaction.
        if (secondsUsed < 0.95f || byEntity is not EntityPlayer || slot == null)
            return;

        try
        {
            ItemStack? before = slot.Itemstack;
            AssetLocation? code = before?.Collectible?.Code;
            if (before == null || code == null || before.StackSize <= 0 ||
                !registry.TryGetByItemCode(code, out DigestiveFoodProfile profile))
                return;

            // Only attempt a direct-food observation if vanilla recognizes
            // nutrition on the actual held stack (including its attributes).
            FoodNutritionProperties? nutrition =
                before.Collectible.GetNutritionProperties(byEntity.World, before, byEntity);
            if (nutrition == null) return;

            int previousSize = before.StackSize;
            string nativeCode = code.ToString();
            string profileId = profile.Id;
            string profileKey = profile.Key;

            // The default CollectibleObject.tryEatStop runs AFTER this passive
            // behavior returns. Inspect the same held slot next server tick.
            byEntity.World.RegisterCallback(_ =>
            {
                try
                {
                    ItemStack? after = slot.Itemstack;
                    int remainingSameCode =
                        after?.Collectible?.Code?.ToString() == nativeCode
                            ? after.StackSize
                            : 0;
                    int removed = previousSize - remainingSameCode;

                    // In normal vanilla direct eating one food unit is removed.
                    // An empty/replaced stack counts only if the original was
                    // exactly one item, minimizing false positives from slot swaps.
                    bool verified =
                        removed == 1 &&
                        (after == null ||
                         after.Collectible?.Code?.ToString() == nativeCode ||
                         previousSize == 1);
                    if (!verified) return;

                    serverApi.Logger.Notification(
                        "[lingamsdigestivesim] EAT-PROBE direct: {0} ({1}), code={2}, consumed=1 native unit. Diagnostic only; no gas generated.",
                        profileKey, profileId, nativeCode);
                }
                catch (Exception checkError)
                {
                    serverApi.Logger.Error(
                        "[lingamsdigestivesim] EAT-PROBE post-interaction check failed: {0}",
                        checkError);
                }
            }, 1);
        }
        catch (Exception captureError)
        {
            // Never cause a vanilla eating interaction to fail just for diagnostics.
            serverApi.Logger.Error(
                "[lingamsdigestivesim] EAT-PROBE capture failed: {0}",
                captureError);
        }
    }

    public static int Attach(ICoreServerAPI api, FoodProfileRegistry registry)
    {
        int attached = 0;
        foreach (CollectibleObject collectible in api.World.Collectibles)
        {
            if (collectible?.Code == null ||
                !registry.TryGetByItemCode(collectible.Code, out _) ||
                collectible.NutritionProps == null ||
                collectible.CollectibleBehaviors.Any(b => b is DirectFoodIntakeProbeBehavior))
                continue;

            var oldBehaviors = collectible.CollectibleBehaviors;
            var newBehaviors = new CollectibleBehavior[oldBehaviors.Length + 1];
            Array.Copy(oldBehaviors, newBehaviors, oldBehaviors.Length);
            newBehaviors[oldBehaviors.Length] =
                new DirectFoodIntakeProbeBehavior(collectible, api, registry);
            collectible.CollectibleBehaviors = newBehaviors;
            attached++;
        }

        api.Logger.Notification(
            "[lingamsdigestivesim] EAT-PROBE enabled on {0} recognized direct-food collectibles (DEBUG only, server-side; no nutrition or gas changes).",
            attached);
        return attached;
    }
}
