using System.Collections.Generic;
using Dalamud.Game.Gui.PartyFinder.Types;

namespace OpenRadar;

public static class Network
{
    public static List<PlayerInfo?> RecentExtractedPlayers = new();
    public static ulong FailedContentId = 0;

    public static void ListingHostExtract(IPartyFinderListing listing, IPartyFinderListingEventArgs args)
    {
        var playerInfo = new PlayerInfo(listing.ContentId, listing.Name.TextValue, (ushort)listing.HomeWorld.RowId);
        _ = Database.AddPlayerORAsync(playerInfo); 
    }
}
