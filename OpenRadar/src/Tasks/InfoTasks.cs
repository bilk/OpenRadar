namespace OpenRadar.Tasks;

public static class Info
{
    public static bool ExtractedContentIds()
    {
        if (CurrentPost is not { } post) return false;

        ListingPlayers = new PlayerInfo?[8];

        for (int i = 0; i < 8; i++)
            ListingPlayers[i] = new PlayerInfo(post.MemberContentIds[i]);

        return true;
    }

    public static bool FindPlayers()
    {
        if (ListingPlayers is not { } players) return false;

        foreach (var player in players)
        {
            if (player == null) return false;
            FindPlayer.Locate(player.contentId);
        }
        return true;
    }
}