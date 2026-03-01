using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;

namespace OpenRadar;

/// <summary>
/// Maps FFXIV duty names to FFLogs globally-unique encounter IDs.
///
/// All lookups are name-based via ContentFinderCondition.Name so that RowIds shifting
/// between patches (new savage tiers, unreal rotations) don't require a code update.
///
/// </summary>
public static class FFLogsEncounterMapping
{
    private static readonly Dictionary<string, int> NameMap = new()
    {
        // ═══════════════════════════════════════════════════════════════════
        // DAWNTRAIL SAVAGE
        // ═══════════════════════════════════════════════════════════════════
        ["AAC Light-heavyweight M1 (Savage)"] = 93,   // M1S  — Black Cat
        ["AAC Light-heavyweight M2 (Savage)"] = 94,   // M2S  — Honey B. Lovely
        ["AAC Light-heavyweight M3 (Savage)"] = 95,   // M3S  — Brute Bomber
        ["AAC Light-heavyweight M4 (Savage)"] = 96,   // M4S  — Wicked Thunder

        ["AAC Cruiserweight M1 (Savage)"] = 97,   // M5S  — Dancing Green
        ["AAC Cruiserweight M2 (Savage)"] = 98,   // M6S  — Sugar Riot
        ["AAC Cruiserweight M3 (Savage)"] = 99,   // M7S  — Brute Abombinator
        ["AAC Cruiserweight M4 (Savage)"] = 100,  // M8S  — Howling Blade

        ["AAC Heavyweight M1 (Savage)"] = 101,  // M9S  — Vamp Fatale
        ["AAC Heavyweight M2 (Savage)"] = 102,  // M10S — Red Hot and Deep Blue
        ["AAC Heavyweight M3 (Savage)"] = 103,  // M11S — The Tyrant
        ["AAC Heavyweight M4 (Savage)"] = 105,  // M12S — Lindwurm II  (104 = door boss)

        // ═══════════════════════════════════════════════════════════════════
        // ENDWALKER SAVAGE
        // ═══════════════════════════════════════════════════════════════════
        ["Asphodelos: The First Circle (Savage)"] = 71,  // P1S
        ["Asphodelos: The Second Circle (Savage)"] = 72,  // P2S
        ["Asphodelos: The Third Circle (Savage)"] = 73,  // P3S
        ["Asphodelos: The Fourth Circle (Savage)"] = 75,  // P4S  (74 = door boss)

        ["Abyssos: The Fifth Circle (Savage)"] = 77,  // P5S
        ["Abyssos: The Sixth Circle (Savage)"] = 78,  // P6S
        ["Abyssos: The Seventh Circle (Savage)"] = 79,  // P7S
        ["Abyssos: The Eighth Circle (Savage)"] = 81,  // P8S  (80 = door boss)

        ["Anabaseios: The Ninth Circle (Savage)"] = 83,  // P9S
        ["Anabaseios: The Tenth Circle (Savage)"] = 84,  // P10S
        ["Anabaseios: The Eleventh Circle (Savage)"] = 85,  // P11S
        ["Anabaseios: The Twelfth Circle (Savage)"] = 87,  // P12S (86 = door boss)

        // ═══════════════════════════════════════════════════════════════════
        // ULTIMATES
        // ═══════════════════════════════════════════════════════════════════
        ["The Unending Coil of Bahamut (Ultimate)"] = 1073, // UCoB
        ["The Weapon's Refrain (Ultimate)"] = 1074, // UwU
        ["The Epic of Alexander (Ultimate)"] = 1075, // TEA
        ["Dragonsong's Reprise (Ultimate)"] = 1076, // DSR
        ["The Omega Protocol (Ultimate)"] = 1077, // TOP
        ["Futures Rewritten (Ultimate)"] = 1079, // FRU

        // ═══════════════════════════════════════════════════════════════════
        // DAWNTRAIL EXTREME TRIALS
        // ═══════════════════════════════════════════════════════════════════
        ["Worqor Lar Dor (Extreme)"] = 1071, // Valigarmanda
        ["Everkeep (Extreme)"] = 1072, // Zoraal Ja
        ["The Minstrel's Ballad: Sphene's Burden"] = 1078, // Queen Eternal

        ["Recollection (Extreme)"] = 1080, // Zelenia
        ["The Minstrel's Ballad: Necron's Embrace"] = 1081, // Necron
        ["The Windward Wilds (Extreme)"] = 1082, // Guardian Arkveld

        ["Hell on Rails (Extreme)"] = 1083, // Doomtrain

        // ═══════════════════════════════════════════════════════════════════
        // CHAOTIC
        // ═══════════════════════════════════════════════════════════════════
        ["The Cloud of Darkness (Chaotic)"] = 2061,

        // ═══════════════════════════════════════════════════════════════════
        // UNREAL
        // ═══════════════════════════════════════════════════════════════════
        ["Tsukuyomi's Pain (Unreal)"] = 3012, // Tsukuyomi  (7.4)
    };

    // Cached at first use — sheet scan only happens once per session.
    private static Dictionary<ushort, int>? _cache;

    // NameMap with all apostrophe variants normalized to ASCII ' for lookup.
    private static readonly Dictionary<string, int> NormalizedNameMap =
        new(NameMap.Count, StringComparer.OrdinalIgnoreCase);

    static FFLogsEncounterMapping()
    {
        foreach (var (key, value) in NameMap)
            NormalizedNameMap[NormalizeApostrophes(key)] = value;
    }

    private static string NormalizeApostrophes(string s)
        => s.Replace('\u2019', '\'')
            .Replace('\u2018', '\'')
            .Replace('\u02BC', '\'')
            .Replace('\u0060', '\''); 

    /// <summary>
    /// Returns the FFLogs encounter ID for a given FFXIV duty RowId, or null if unmapped.
    /// Resolves the RowId to a duty name via the game sheet, then looks up the FFLogs ID.
    /// </summary>
    public static int? GetFFLogsEncounterId(ushort ffxivDutyId)
    {
        if (_cache == null)
        {
            _cache = new Dictionary<ushort, int>();
            var sheet = Svc.Data.GetExcelSheet<ContentFinderCondition>();
            if (sheet != null)
            {
                foreach (var row in sheet)
                {
                    var name = NormalizeApostrophes(row.Name.ToString());
                    if (NormalizedNameMap.TryGetValue(name, out var fflogsId))
                        _cache[(ushort)row.RowId] = fflogsId;
                }
            }
        }

        return _cache.TryGetValue(ffxivDutyId, out var result) ? result : null;
    }
    public static void InvalidateCache() => _cache = null;
}
