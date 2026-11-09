using System;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace lingamsdigestivesim.Foods;

/// <summary>
/// DEBUG-ONLY, PASSIVE, SERVER-SIDE diagnostic of ordinary vanilla eating.
/// Harmony observes the existing CollectibleObject.tryEatStop method. It does
/// not mutate game assets, collectible behavior lists, nutrition, or inventory.
/// Harmony patches are removed by FoodProfilesModSystem.Dispose.
/// </summary>
public static class DirectFoodIntakeHarmonyProbe
{
    private const string PatchId = "lingamsdigestivesim.debug.directfoodintake";
    private static FoodProfileRegistry? registry;
    private static ICoreServerAPI? serverApi;

    private sealed record BeforeBite(string NativeCode, string ProfileId, string Key, int StackSize);

    public static Harmony? Install(ICoreServerAPI api, FoodProfileRegistry foodRegistry)
    {
        if (Harmony.HasAnyPatches(PatchId))
        {
            api.Logger.Warning("[lingamsdigestivesim] EAT-PROBE patch already exists; not installing twice.");
            return null;
        }

        // Pin the exact protected virtual native method and its 1.22 signature.
        // Missing or changed API -> diagnostic stays disabled, not a failed world load.
        MethodInfo? target = typeof(CollectibleObject).GetMethod(
            "tryEatStop",
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: new[] { typeof(float), typeof(ItemSlot), typeof(EntityAgent) },
            modifiers: null);

        if (target == null)
        {
            api.Logger.Warning("[lingamsdigestivesim] EAT-PROBE disabled: native tryEatStop(float, ItemSlot, EntityAgent) not found.");
            return null;
        }

        var harmony = new Harmony(PatchId);
        try
        {
            registry = foodRegistry;
            serverApi = api;

            MethodInfo before = typeof(DirectFoodIntakeHarmonyProbe).GetMethod(
                nameof(BeforeEat),
                BindingFlags.Static | BindingFlags.NonPublic)!;
            MethodInfo after = typeof(DirectFoodIntakeHarmonyProbe).GetMethod(
                nameof(AfterEat),
                BindingFlags.Static | BindingFlags.NonPublic)!;

            harmony.Patch(target, prefix: new HarmonyMethod(before), postfix: new HarmonyMethod(after));
            api.Logger.Notification(
                "[lingamsdigestivesim] EAT-PROBE Harmony observation active (DEBUG server only, vanilla tryEatStop; no item/recipe modifications).");
            return harmony;
        }
        catch (Exception error)
        {
            harmony.UnpatchAll(PatchId);
            registry = null;
            serverApi = null;
            api.Logger.Error("[lingamsdigestivesim] EAT-PROBE patch failed and was rolled back: {0}", error);
            return null;
        }
    }

    public static void Uninstall(Harmony? harmony)
    {
        try
        {
            // Only remove patches with *this* unique debug ID.
            harmony?.UnpatchAll(PatchId);
        }
        finally
        {
            registry = null;
            serverApi = null;
        }
    }

    // Called immediately before original method. No return value: original
    // vanilla eating logic must always run unmodified.
    private static void BeforeEat(
        float secondsUsed,
        ItemSlot slot,
        EntityAgent byEntity,
        out BeforeBite? __state)
    {
        __state = null;
        try
        {
            if (secondsUsed < 0.95f ||
                byEntity is not EntityPlayer ||
                byEntity.Api is not ICoreServerAPI ||
                registry == null ||
                slot?.Itemstack == null)
                return;

            ItemStack stack = slot.Itemstack;
            AssetLocation? code = stack.Collectible?.Code;
            if (code == null || stack.StackSize <= 0 ||
                !registry.TryGetByItemCode(code, out DigestiveFoodProfile profile))
                return;

            __state = new BeforeBite(code.ToString(), profile.Id, profile.Key, stack.StackSize);
        }
        catch (Exception error)
        {
            serverApi?.Logger.Error("[lingamsdigestivesim] EAT-PROBE prefix diagnostic failed: {0}", error);
        }
    }

    // Only the original method can consume food. We merely compare the held
    // slot before and after and report a verified single-unit reduction.
    private static void AfterEat(ItemSlot slot, BeforeBite? __state)
    {
        if (__state == null) return;
        try
        {
            ItemStack? after = slot?.Itemstack;
            bool sameFood = after?.Collectible?.Code?.ToString() == __state.NativeCode;

            // Normal stack: one native unit was removed.
            // Final item: stack disappeared or was replaced by an eatenStack
            // (e.g. bowl/container). Do not report arbitrary inventory swaps
            // as successful eating in larger stacks.
            bool oneUnitConsumed = sameFood
                ? after!.StackSize == __state.StackSize - 1
                : __state.StackSize == 1;

            if (!oneUnitConsumed) return;

            serverApi?.Logger.Notification(
                "[lingamsdigestivesim] EAT-PROBE direct: {0} ({1}), code={2}, consumed=1 native unit. Diagnostic only; no gas generated.",
                __state.Key, __state.ProfileId, __state.NativeCode);
        }
        catch (Exception error)
        {
            serverApi?.Logger.Error("[lingamsdigestivesim] EAT-PROBE postfix diagnostic failed: {0}", error);
        }
    }
}
