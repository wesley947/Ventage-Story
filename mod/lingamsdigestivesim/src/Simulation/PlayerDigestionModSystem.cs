using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using lingamsdigestivesim.Foods;

namespace lingamsdigestivesim.Simulation;

/// <summary>
/// First, narrow, SERVER-ONLY direct-food digestive loop.
/// Each player's pending meal closes after 30 seconds of eating inactivity.
/// Each source then produces gas over its own game-time delay/duration.
/// No items, recipes, satiety, vanilla behaviors, sounds or animals are changed.
/// </summary>
public sealed class PlayerDigestionModSystem : ModSystem
{
    private const string RootKey = "lingamsdigestivesim";
    private const string DigestiveJsonKey = "digestionStateJson";
    private const int DigestiveSchemaVersion = 1;
    private const double EatingInactivitySeconds = 30d;

    private ICoreServerAPI? api;
    private long listenerId = -1;
    // In-memory stopwatch origins are never saved: offline time cannot consume
    // the player's remaining eating-window countdown.
    private readonly Dictionary<string, long> lastSeenTicks = new();

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Server;

    public override void StartServerSide(ICoreServerAPI server)
    {
        base.StartServerSide(server);
        api = server;
        server.Event.PlayerNowPlaying += OnPlayerReady;
        server.Event.PlayerDisconnect += OnPlayerDisconnect;
        server.Event.GameWorldSave += OnWorldSave;
        listenerId = server.Event.RegisterGameTickListener(OnTick, 1000);
#if DEBUG
        // Passive notification only: the existing Harmony observer still owns
        // the actual, already-verified vanilla eating interception.
        DirectFoodIntakeHarmonyProbe.FoodConsumed += OnDirectFoodConsumed;
        server.Logger.Notification("[lingamsdigestivesim] DIGEST-TEST enabled for verified direct player foods (30-second inactivity session, game-time gas accrual).");
#else
        server.Logger.Notification("[lingamsdigestivesim] DIGEST-TEST has no direct-food observer in Release builds; DEBUG build required for intake testing.");
#endif
    }

    public override void Dispose()
    {
#if DEBUG
        DirectFoodIntakeHarmonyProbe.FoodConsumed -= OnDirectFoodConsumed;
#endif
        if (api != null)
        {
            api.Event.PlayerNowPlaying -= OnPlayerReady;
            api.Event.PlayerDisconnect -= OnPlayerDisconnect;
            api.Event.GameWorldSave -= OnWorldSave;
            if (listenerId >= 0) api.Event.UnregisterGameTickListener(listenerId);
            api = null;
        }
        lastSeenTicks.Clear();
        base.Dispose();
    }

    private sealed class PendingFood
    {
        public string ProfileId { get; set; } = "";
        public double Quantity { get; set; }
        public double GasPotential { get; set; }
        public double DelayHours { get; set; }
        public double ProductionHours { get; set; }
    }

    private sealed class ActiveFood
    {
        public string ProfileId { get; set; } = "";
        public double Quantity { get; set; }
        public double GasPotential { get; set; }
        public double DelayHours { get; set; }
        public double ProductionHours { get; set; }
        public double MealClosedHour { get; set; }
        public double ProducedSoFar { get; set; }
    }

    private sealed class DigestionState
    {
        public int Version { get; set; } = DigestiveSchemaVersion;
        public double RemainingSeconds { get; set; }
        public List<PendingFood> Pending { get; set; } = new();
        public List<ActiveFood> Active { get; set; } = new();
    }

    private static bool TryLoad(Entity entity, out ITreeAttribute? root, out DigestionState? state, out string error)
    {
        root = null;
        state = null;
        error = "";

        // PlayerGasStateModSystem initializes the authoritative gas store.
        // Never recreate or rewrite an absent/invalid version-1 gas store here.
        if (entity.Attributes.GetTreeAttribute(RootKey) is not ITreeAttribute gasRoot ||
            gasRoot.GetInt("schemaVersion", -1) != 1 ||
            gasRoot.TryGetDouble("gasUnits") is not double gas ||
            gasRoot.TryGetDouble("normalCapacityUnits") is not double capacity ||
            !double.IsFinite(gas) || gas < 0 ||
            !double.IsFinite(capacity) || capacity <= 0)
        {
            error = "Persistent player gas storage missing or invalid; digestion skipped.";
            return false;
        }

        root = gasRoot;
        string? json = root.GetString(DigestiveJsonKey);
        if (json == null)
        {
            if (root.HasAttribute(DigestiveJsonKey))
            {
                error = "Existing digestion data has an unexpected attribute type; not overwriting.";
                return false;
            }
            state = new DigestionState();
            return true;
        }

        try
        {
            if (json.Length > 1000000) throw new InvalidOperationException("Digestive data exceeds safety limit.");
            state = JsonSerializer.Deserialize<DigestionState>(json);
            if (state == null || state.Version != DigestiveSchemaVersion ||
                state.Pending == null || state.Active == null ||
                !double.IsFinite(state.RemainingSeconds) ||
                state.RemainingSeconds < 0 || state.RemainingSeconds > EatingInactivitySeconds + 0.001)
                throw new InvalidOperationException("Digestive schema or countdown invalid.");

            foreach (PendingFood food in state.Pending)
                if (food == null || !ValidFood(food.ProfileId, food.Quantity, food.GasPotential,
                        food.DelayHours, food.ProductionHours))
                    throw new InvalidOperationException("Invalid pending food data.");

            foreach (ActiveFood food in state.Active)
                if (food == null || !ValidFood(food.ProfileId, food.Quantity, food.GasPotential,
                        food.DelayHours, food.ProductionHours) ||
                    !double.IsFinite(food.MealClosedHour) ||
                    !double.IsFinite(food.ProducedSoFar) || food.ProducedSoFar < 0 ||
                    food.ProducedSoFar > food.GasPotential + 0.000001)
                    throw new InvalidOperationException("Invalid active food data.");

            if ((state.Pending.Count == 0 && state.RemainingSeconds != 0) ||
                (state.Pending.Count > 0 && state.RemainingSeconds <= 0))
                throw new InvalidOperationException("Inconsistent saved eating session.");
            return true;
        }
        catch (Exception ex)
        {
            error = "Saved digestion data invalid; left intact: " + ex.Message;
            root = null;
            state = null;
            return false;
        }
    }

    private static bool ValidFood(string id, double quantity, double gas, double delay, double duration) =>
        !string.IsNullOrWhiteSpace(id) &&
        double.IsFinite(quantity) && quantity > 0 &&
        double.IsFinite(gas) && gas >= 0 &&
        double.IsFinite(delay) && delay >= 0 &&
        double.IsFinite(duration) && duration > 0;

    private static void Save(ITreeAttribute root, DigestionState state) =>
        root.SetString(DigestiveJsonKey, JsonSerializer.Serialize(state));

    private void OnPlayerReady(IServerPlayer player)
    {
        // This deliberately resets the real-time tick origin on reconnect.
        lastSeenTicks[player.PlayerUID] = Stopwatch.GetTimestamp();
        if (player.Entity != null) Advance(player.Entity, 0, "join");
    }

    private void OnPlayerDisconnect(IServerPlayer player)
    {
        // Preserve the remaining seconds of a half-finished eating session.
        if (player.Entity != null) Advance(player.Entity, SecondsSinceLast(player.PlayerUID), "disconnect");
        lastSeenTicks.Remove(player.PlayerUID);
    }

    private double SecondsSinceLast(string uid)
    {
        long now = Stopwatch.GetTimestamp();
        if (!lastSeenTicks.TryGetValue(uid, out long old))
        {
            lastSeenTicks[uid] = now;
            return 0;
        }

        lastSeenTicks[uid] = now;
        return Math.Max(0d, Stopwatch.GetElapsedTime(old, now).TotalSeconds);
    }

    private void OnWorldSave()
    {
        if (api == null) return;
        foreach (IPlayer player in api.World.AllOnlinePlayers)
            if (player is IServerPlayer serverPlayer && serverPlayer.Entity != null &&
                lastSeenTicks.ContainsKey(serverPlayer.PlayerUID))
                Advance(serverPlayer.Entity, SecondsSinceLast(serverPlayer.PlayerUID), "save");
    }

    private void OnTick(float delta)
    {
        if (api == null) return;
        foreach (IPlayer player in api.World.AllOnlinePlayers)
            if (player is IServerPlayer serverPlayer && serverPlayer.Entity != null &&
                lastSeenTicks.ContainsKey(serverPlayer.PlayerUID))
                Advance(serverPlayer.Entity, SecondsSinceLast(serverPlayer.PlayerUID), "tick");
    }

    private void OnDirectFoodConsumed(EntityPlayer player, DigestiveFoodProfile profile)
    {
        try
        {
            // Only verified vanilla tryEatStop successes emit this event.
            if (api == null || !ValidFood(profile.Id, 1, profile.GasPotentialForQuantity(1),
                profile.DelayGameHours, profile.ProductionGameHours)) return;

            if (player.Player is not IServerPlayer serverPlayer) return;
            Advance(player, SecondsSinceLast(serverPlayer.PlayerUID), "eat-before");

            if (!TryLoad(player, out ITreeAttribute? root, out DigestionState? data, out string error))
            {
                api.Logger.Warning("[lingamsdigestivesim] DIGEST-TEST food not queued: {0}", error);
                return;
            }

            // Merge identical food profiles in the pending session without
            // losing their individual profile parameters or amount.
            double potential = profile.GasPotentialForQuantity(1);
            PendingFood? match = data!.Pending.Find(x =>
                x.ProfileId == profile.Id &&
                x.DelayHours == profile.DelayGameHours &&
                x.ProductionHours == profile.ProductionGameHours);

            if (match == null)
                data.Pending.Add(new PendingFood {
                    ProfileId = profile.Id, Quantity = 1, GasPotential = potential,
                    DelayHours = profile.DelayGameHours, ProductionHours = profile.ProductionGameHours
                });
            else
            {
                match.Quantity += 1;
                match.GasPotential += potential;
            }

            // RESET the countdown to thirty seconds; never ADD thirty seconds.
            data.RemainingSeconds = EatingInactivitySeconds;
            Save(root!, data);
            api.Logger.Notification(
                "[lingamsdigestivesim] DIGEST-TEST queued {0}; pending food types={1}, countdown RESET to 30s, no gas produced.",
                profile.Id, data.Pending.Count);
        }
        catch (Exception ex)
        {
            api?.Logger.Error("[lingamsdigestivesim] DIGEST-TEST intake failed (vanilla eating unaffected): {0}", ex);
        }
    }

    private void Advance(Entity entity, double elapsedRealSeconds, string reason)
    {
        try
        {
            if (!TryLoad(entity, out ITreeAttribute? root, out DigestionState? data, out string error))
            {
                // Do not flood logs for an uninitialized player or corrupt state.
                if (reason == "join") api?.Logger.Warning("[lingamsdigestivesim] DIGEST-TEST {0}", error);
                return;
            }

            bool changed = false;
            double nowHour = entity.World.Calendar.TotalHours;
            if (!double.IsFinite(nowHour)) return;

            if (data!.Pending.Count > 0)
            {
                double remaining = Math.Max(0, data.RemainingSeconds - elapsedRealSeconds);
                if (remaining != data.RemainingSeconds) { data.RemainingSeconds = remaining; changed = true; }
                if (remaining <= 0)
                {
                    // Closing a meal is the ONLY time it starts its game-hour clock.
                    foreach (PendingFood food in data.Pending)
                        data.Active.Add(new ActiveFood {
                            ProfileId = food.ProfileId, Quantity = food.Quantity,
                            GasPotential = food.GasPotential, DelayHours = food.DelayHours,
                            ProductionHours = food.ProductionHours, MealClosedHour = nowHour,
                            ProducedSoFar = 0
                        });
                    int types = data.Pending.Count;
                    data.Pending.Clear();
                    data.RemainingSeconds = 0;
                    changed = true;
                    api?.Logger.Notification(
                        "[lingamsdigestivesim] DIGEST-TEST meal closed: {0} food types now active at game hour {1:0.00}.",
                        types, nowHour);
                }
            }

            double newlyProduced = 0d;
            for (int i = data.Active.Count - 1; i >= 0; i--)
            {
                ActiveFood food = data.Active[i];
                double fraction = Math.Clamp(
                    (nowHour - food.MealClosedHour - food.DelayHours) / food.ProductionHours, 0d, 1d);
                double cumulative = food.GasPotential * fraction;
                double delta = Math.Max(0, cumulative - food.ProducedSoFar);
                if (delta > 0)
                {
                    food.ProducedSoFar = cumulative;
                    newlyProduced += delta;
                    changed = true;
                }

                if (food.ProducedSoFar >= food.GasPotential - 0.000000001 &&
                    nowHour >= food.MealClosedHour + food.DelayHours + food.ProductionHours)
                {
                    data.Active.RemoveAt(i);
                    changed = true;
                }
            }

            if (newlyProduced > 0d)
            {
                // Do not silently cap the reservoir at 100%; only future actual
                // release events are allowed to subtract stored gas.
                double previousGas = root!.GetDouble("gasUnits");
                double nextGas = previousGas + newlyProduced;
                if (!double.IsFinite(nextGas))
                    throw new InvalidOperationException("Produced gas would overflow the reservoir.");

                root.SetDouble("gasUnits", nextGas);
            }

            // Saved digestive progress and gas amount live in the SAME entity
            // attributes tree; a world save serializes them together.
            if (changed) Save(root!, data);
        }
        catch (Exception ex)
        {
            api?.Logger.Error("[lingamsdigestivesim] DIGEST-TEST update failed: {0}", ex);
        }
    }

    public static string Describe(Entity entity)
    {
        if (!TryLoad(entity, out _, out DigestionState? data, out string error))
            return "Digestion unavailable: " + error;
        if (data!.Pending.Count == 0 && data.Active.Count == 0)
            return "No pending or active foods.";
        return $"Pending: {data.Pending.Count} food type(s), {data.RemainingSeconds:0}s remaining; active: {data.Active.Count} food contribution(s).";
    }
}
