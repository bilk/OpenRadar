using Dalamud.Game.Gui.PartyFinder.Types;
using static Dalamud.Game.Gui.PartyFinder.Types.JobFlags;

public static class JobRoles
{
    public const JobFlags Tanks = Paladin | Warrior | DarkKnight | Gunbreaker | Marauder | Gladiator;
    public const JobFlags Healers = WhiteMage | Scholar | Astrologian | Sage | Conjurer;
    public const JobFlags DPS = Monk | Dragoon | Bard | BlackMage | Summoner | Ninja | Machinist | Samurai | RedMage | Dancer | Reaper | Viper | Pictomancer | BlueMage | Lancer | Pugilist | Archer | Arcanist | Rogue;
}