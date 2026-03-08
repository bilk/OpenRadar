using Dalamud.Game.Gui.PartyFinder.Types;
using static Dalamud.Game.Gui.PartyFinder.Types.JobFlags;

namespace OpenRadar;

public static partial class Data
{
    public record PlayerInfo
    (
        ulong contentId,
        string? name = null,
        ushort? world = null,
        byte? jobId = null,
        string? progPoint = null,
        FFLogsData? logData = null
    )
    {
        public PlayerInfo merge(PlayerInfo o) => this with
        {
            name = o.name ?? name,
            world = o.world ?? world,
            jobId = o.jobId ?? jobId,
            progPoint = o.progPoint ?? progPoint,
            logData = o.logData ?? logData
        };
    }

    public record FFLogsData
    (
        float? BestParse,
        float? MedianParse,
        int? Kills,
        bool IsHidden = false
    );
}

public static class JobRoles
{
    public static readonly JobFlags Tanks = Paladin | Warrior | DarkKnight | Gunbreaker | Marauder | Gladiator;
    public static readonly JobFlags Healers = WhiteMage | Scholar | Astrologian | Sage | Conjurer;
    public static readonly JobFlags DPS = Monk | Dragoon | Bard | BlackMage | Summoner | Ninja | Machinist | Samurai | RedMage | Dancer | Reaper | Viper | Pictomancer | BlueMage | Lancer | Pugilist | Archer | Arcanist | Rogue;
}