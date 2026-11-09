using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace lingamsdigestivesim.Simulation;

/// <summary>
/// Server-only player gas persistence and administrator diagnostics.
/// The separate digestion system fills the saved reservoir. These commands
/// only test native trumpet playback and deliberate, gas-consuming releases.
/// Nothing here changes vanilla entities, items, recipes or food observation.
/// </summary>
public sealed class PlayerGasStateModSystem : ModSystem
{
    private const string StateKey = "lingamsdigestivesim";
    private const int SchemaVersion = 1;
    private const double PlayerNormalFullGasUnits = 100d; // Test baseline, not final balance.
    private const double TestReleaseFraction = 0.60d; // Temporary manual release ONLY; balance is not final.
    private static readonly AssetLocation TrumpetSound = new("game", "sounds/voice/trumpet");

    private ICoreServerAPI? serverApi;

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Server;

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
        serverApi = api;

        // This event occurs after the player has joined and their client is loaded.
        api.Event.PlayerNowPlaying += OnPlayerNowPlaying;

        // Diagnostic commands are restricted to server administrators.
        api.ChatCommands.Create("gas")
            .WithDescription("Lingam's digestive gas persistence diagnostics")
            .RequiresPlayer()
            .RequiresPrivilege(Privilege.controlserver)
            .BeginSubCommand("status")
                .WithDescription("Show your saved gas amount and normal-full capacity")
                .HandleWith(OnStatus)
            .EndSubCommand()
            .BeginSubCommand("set")
                .WithDescription("Set your gas pressure percentage for persistence testing")
                .WithArgs(api.ChatCommands.Parsers.DoubleRange("percent", 0d, 1000d))
                .HandleWith(OnSet)
            .EndSubCommand()
            .BeginSubCommand("sound")
                .WithDescription("Play a vanilla trumpet sound with pitch variation; use no gas")
                .HandleWith(OnSound)
            .EndSubCommand()
            .BeginSubCommand("fart")
                .WithDescription("Test a trumpet release that consumes 60% of current stored gas")
                .HandleWith(OnFart)
            .EndSubCommand();

        api.Logger.Notification("[lingamsdigestivesim] Player gas storage plus manual trumpet sound/release diagnostics active (no automatic release).");
    }

    public override void Dispose()
    {
        if (serverApi != null)
        {
            serverApi.Event.PlayerNowPlaying -= OnPlayerNowPlaying;
            serverApi = null;
        }
        base.Dispose();
    }

    private void OnPlayerNowPlaying(IServerPlayer player)
    {
        if (player.Entity == null) return;

        try
        {
            if (!TryGetState(player.Entity, out ITreeAttribute? state, out bool created, out string error))
            {
                serverApi?.Logger.Warning("[lingamsdigestivesim] Player gas state unavailable: {0}", error);
                return;
            }

            if (created)
            {
                serverApi?.Logger.Notification(
                    "[lingamsdigestivesim] Created initial player gas state: {0:0.00}% (saved in entity attributes).",
                    100d * state!.GetDouble("gasUnits") / state.GetDouble("normalCapacityUnits"));
            }
        }
        catch (Exception ex)
        {
            serverApi?.Logger.Error("[lingamsdigestivesim] Player gas initialization failed: {0}", ex);
        }
    }

    private static bool TryGetState(
        Entity entity, out ITreeAttribute? state, out bool created, out string error)
    {
        state = null;
        created = false;
        error = "";

        // A present tree with gas=0 is an existing, valid state, NOT an absent one.
        // Malformed or future-version data must never be silently overwritten.
        if (entity.Attributes.HasAttribute(StateKey))
        {
            if (entity.Attributes[StateKey] is not ITreeAttribute existing)
            {
                error = "Existing mod attribute is not a tree. No changes made.";
                return false;
            }

            if (existing.GetInt("schemaVersion", -1) != SchemaVersion)
            {
                error = "Unknown saved gas-state version. No changes made.";
                return false;
            }

            double? gas = existing.TryGetDouble("gasUnits");
            double? capacity = existing.TryGetDouble("normalCapacityUnits");
            if (!gas.HasValue || !capacity.HasValue || !double.IsFinite(gas.Value) ||
                !double.IsFinite(capacity.Value) || gas.Value < 0d || capacity.Value <= 0d)
            {
                error = "Existing gas-state values are invalid. No changes made.";
                return false;
            }

            state = existing;
            return true;
        }

        // First attachment only. 15-20% of the player's current normal-full amount.
        state = entity.Attributes.GetOrAddTreeAttribute(StateKey);
        state.SetInt("schemaVersion", SchemaVersion);
        state.SetDouble("normalCapacityUnits", PlayerNormalFullGasUnits);
        state.SetDouble("gasUnits", PlayerNormalFullGasUnits * (0.15d + Random.Shared.NextDouble() * 0.05d));
        state.SetDouble("lastUpdateTotalHours", entity.World.Calendar.TotalHours);
        created = true;
        return true;
    }

    private static bool TryGetPlayerState(
        TextCommandCallingArgs args, out ITreeAttribute? state, out string error)
    {
        state = null;
        error = "";
        if (args.Caller.Player is not IServerPlayer player || player.Entity == null)
        {
            error = "This command requires a loaded player entity.";
            return false;
        }

        return TryGetState(player.Entity, out state, out _, out error);
    }

    /// <summary>
    /// Server-originated positional audio: callers and nearby vanilla clients
    /// can receive the built-in game asset. No client mod files are required.
    /// This is a dispatch only, not proof that any listener heard the sound.
    /// </summary>
    private void DispatchTrumpet(Entity entity, float pitch, float volume)
    {
        if (serverApi == null)
            throw new InvalidOperationException("Server sound API is unavailable.");

        // Location includes sounds/ prefix, excludes .ogg extension.
        // Pass null to avoid excluding the invoking player from the broadcast.
        serverApi.World.PlaySoundAt(
            TrumpetSound, entity, null, pitch, 20f, volume);
    }

    private static float NextPitch(float minimum, float maximum) =>
        minimum + (float)Random.Shared.NextDouble() * (maximum - minimum);

    private TextCommandResult OnSound(TextCommandCallingArgs args)
    {
        try
        {
            if (args.Caller.Player is not IServerPlayer player || player.Entity == null)
                return TextCommandResult.Error("A loaded player is required.");

            float pitch = NextPitch(0.86f, 1.14f);
            DispatchTrumpet(player.Entity, pitch, 0.85f);
            return TextCommandResult.Success(
                $"Vanilla trumpet sound requested at your position (pitch {pitch:0.00}, volume 0.85). No gas consumed. Confirm it is audible.");
        }
        catch (Exception ex)
        {
            serverApi?.Logger.Error("[lingamsdigestivesim] /gas sound failed: {0}", ex);
            return TextCommandResult.Error("Trumpet sound dispatch failed; gas unchanged. See server log.");
        }
    }

    private TextCommandResult OnFart(TextCommandCallingArgs args)
    {
        try
        {
            if (!TryGetPlayerState(args, out ITreeAttribute? state, out string error))
                return TextCommandResult.Error(error);
            if (args.Caller.Player is not IServerPlayer player || player.Entity == null)
                return TextCommandResult.Error("A loaded player is required.");

            double gas = state!.GetDouble("gasUnits");
            if (gas <= 0d)
                return TextCommandResult.Success("Your gas reservoir is empty. No release and no sound.");

            double capacity = state.GetDouble("normalCapacityUnits");
            double released = gas * TestReleaseFraction;
            double remaining = Math.Max(0d, gas - released);

            // For this one-off manual test, high pressure gets a deeper trumpet.
            // No preset release tiers, crouch triggers, background odds, or
            // animal releases are introduced here.
            float pitch = gas >= capacity
                ? NextPitch(0.75f, 0.93f)
                : NextPitch(0.95f, 1.10f);
            DispatchTrumpet(player.Entity, pitch, 0.90f);

            // Change saved gas only for the real release, after sound dispatch.
            state.SetDouble("gasUnits", remaining);
            return TextCommandResult.Success(
                $"Manual trumpet release: {released:0.00} gas units expelled; {remaining:0.00} remaining ({100d * remaining / capacity:0.00}%). Pitch {pitch:0.00}. Test-only 60% release.");
        }
        catch (Exception ex)
        {
            serverApi?.Logger.Error("[lingamsdigestivesim] /gas fart failed: {0}", ex);
            return TextCommandResult.Error("Manual gas release failed; check server log.");
        }
    }

    private TextCommandResult OnStatus(TextCommandCallingArgs args)
    {
        try
        {
            if (!TryGetPlayerState(args, out ITreeAttribute? state, out string error))
                return TextCommandResult.Error(error);

            double gas = state!.GetDouble("gasUnits");
            double capacity = state.GetDouble("normalCapacityUnits");
            return TextCommandResult.Success(
                $"Gas: {100d * gas / capacity:0.00}% ({gas:0.00} / {capacity:0.00} units). Saved state version {SchemaVersion}. {PlayerDigestionModSystem.Describe(((IServerPlayer)args.Caller.Player).Entity)}");
        }
        catch (Exception ex)
        {
            serverApi?.Logger.Error("[lingamsdigestivesim] /gas status failed: {0}", ex);
            return TextCommandResult.Error("Unable to read gas state; check server log.");
        }
    }

    private TextCommandResult OnSet(TextCommandCallingArgs args)
    {
        try
        {
            if (!TryGetPlayerState(args, out ITreeAttribute? state, out string error))
                return TextCommandResult.Error(error);

            double percent = (double)args[0];
            if (!double.IsFinite(percent) || percent < 0d || percent > 1000d)
                return TextCommandResult.Error("Percent must be between 0 and 1000.");

            double capacity = state!.GetDouble("normalCapacityUnits");
            state.SetDouble("gasUnits", capacity * percent / 100d);

            return TextCommandResult.Success(
                $"Gas set to {percent:0.00}% ({state.GetDouble("gasUnits"):0.00} units). Save, exit and reload to verify persistence.");
        }
        catch (Exception ex)
        {
            serverApi?.Logger.Error("[lingamsdigestivesim] /gas set failed: {0}", ex);
            return TextCommandResult.Error("Unable to set gas state; check server log.");
        }
    }
}
