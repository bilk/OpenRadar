using Dalamud.Game.Gui.PartyFinder.Types;
using static Dalamud.Game.Gui.PartyFinder.Types.JobFlags;

public static class JobRoles
{
    public static readonly JobFlags Tanks = Paladin | Warrior | DarkKnight | Gunbreaker | Marauder | Gladiator;
    public static readonly JobFlags Healers = WhiteMage | Scholar | Astrologian | Sage | Conjurer;
    public static readonly JobFlags DPS = Monk | Dragoon | Bard | BlackMage | Summoner | Ninja | Machinist | Samurai | RedMage | Dancer | Reaper | Viper | Pictomancer | BlueMage | Lancer | Pugilist | Archer | Arcanist | Rogue;
}