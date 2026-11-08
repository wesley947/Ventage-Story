using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace lingamsdigestivesim.Foods;

/// <summary>
/// One-time, server-only diagnostics to discover *real*, registered vanilla item/block codes.
/// This does NOT auto-bind any candidate to a food profile: all matches are suggestions
/// which must be reviewed before food ingestion is assigned a gas potential.
/// </summary>
public static class VanillaFoodCodeAudit
{
    public static void Write(ICoreServerAPI api, FoodProfileRegistry registry)
    {
        try
        {
            string root = api.GetOrCreateDataPath("ModData");
            string folder = Path.Combine(root, "lingamsdigestivesim");
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, "vanilla-food-code-audit.csv");

            var profiles = registry.All.ToArray();
            var collectibles = api.World.Collectibles
                .Where(obj => obj != null && obj.Code != null &&
                              string.Equals(obj.Code.Domain, "game", StringComparison.OrdinalIgnoreCase))
                .OrderBy(obj => obj.Code.ToString(), StringComparer.OrdinalIgnoreCase)
                .ThenBy(obj => obj is Block ? 0 : 1)
                .ToArray();

            var report = new StringBuilder();
            report.AppendLine("CollectibleType,NativeItemCode,NativeFoodCategory,NativeSatiety,SuggestedProfileIds,SuggestedProfileNames,ReviewStatus");

            int rowsWithSuggestions = 0;
            var suggestedProfileIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (CollectibleObject obj in collectibles)
            {
                string code = obj.Code.ToString();
                string nativeName = obj.Code.Path.ToLowerInvariant();
                string nativeCompact = Compact(nativeName);
                var suggestions = new List<DigestiveFoodProfile>();

                foreach (var profile in profiles)
                {
                    if (SeemsRelated(nativeName, nativeCompact, profile))
                    {
                        suggestions.Add(profile);
                        suggestedProfileIds.Add(profile.Id);
                    }
                }

                string family = obj.NutritionProps?.FoodCategory.ToString() ?? "";
                string satiety = obj.NutritionProps?.Satiety.ToString(
                    System.Globalization.CultureInfo.InvariantCulture) ?? "";

                report.AppendLine(string.Join(",",
                    Escape(obj is Block ? "Block" : "Item"),
                    Escape(code),
                    Escape(family),
                    Escape(satiety),
                    Escape(string.Join("|", suggestions.Select(p => p.Id))),
                    Escape(string.Join("|", suggestions.Select(p => p.Name))),
                    Escape(suggestions.Count == 0 ? "Unmatched / inspect if edible" : "Suggested only - VERIFY")));

                if (suggestions.Count > 0) rowsWithSuggestions++;
            }

            // Also include profiles with no matching name guess so missing foods are visible.
            report.AppendLine();
            report.AppendLine("UnmatchedProfileId,ProfileKey,ProfileName,Reason");
            foreach (var profile in profiles.Where(p => !suggestedProfileIds.Contains(p.Id)))
            {
                report.AppendLine(string.Join(",",
                    Escape(profile.Id), Escape(profile.Key),
                    Escape(profile.Name), Escape("No suggested match in vanilla collectible code; manually audit")));
            }

            File.WriteAllText(path, report.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            api.Logger.Notification(
                "[lingamsdigestivesim] Food-code audit saved: {0} vanilla collectibles, {1} native-code candidate rows, {2}/{3} profiles with name-based suggestions. File: {4}",
                collectibles.Length, rowsWithSuggestions, suggestedProfileIds.Count, profiles.Length, path);
            api.Logger.Notification(
                "[lingamsdigestivesim] Audit is informational only: zero newly verified/bound codes. Never treat suggested names as proven food identities.");
        }
        catch (Exception ex)
        {
            api.Logger.Error("[lingamsdigestivesim] Unable to create vanilla food-code audit: {0}", ex);
        }
    }

    private static bool SeemsRelated(string nativeName, string nativeCompact, DigestiveFoodProfile profile)
    {
        string keyCompact = Compact(profile.Key);
        if (keyCompact.Length >= 4 && nativeCompact.Contains(keyCompact, StringComparison.Ordinal))
            return true;

        // A conservative secondary suggestion for reordered compound codes
        // (e.g., 'grain-rice' vs 'rice-grain'). Each meaningful word must be present.
        var words = profile.Key
            .Split(new[] { '-', '_', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(s => s.Length >= 4)
            .Select(s => s.ToLowerInvariant())
            .ToArray();
        if (words.Length >= 2 && words.All(w => nativeCompact.Contains(w, StringComparison.Ordinal)))
            return true;

        return false;
    }

    private static string Compact(string text)
    {
        var chars = text.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray();
        return new string(chars);
    }

    private static string Escape(string value)
    {
        return "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
    }
}
