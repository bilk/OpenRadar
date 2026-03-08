using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OpenRadar;

public static partial class Data
{
    public static PlayerInfo?[] ListingPlayers = new PlayerInfo?[8];
    public static AgentLookingForGroup.Detailed? CurrentPost = null;
    public static void PopulateListingPlayers(PlayerInfo? playerInfo, bool addPlayer = true)
    {
        if (CurrentPost == null || playerInfo == null) return;

        var index = Array.FindIndex(ListingPlayers, p => p?.contentId == playerInfo.contentId);
        
        if (index != -1 && ListingPlayers[index] is { } p)
            ListingPlayers[index] = p.merge(playerInfo);
        if (addPlayer) _ = Database.AddPlayerORAsync(playerInfo);
    }

    public static void ResetExtractedData()
    {
        CurrentPost = new();
        ListingPlayers = new PlayerInfo?[8];
    }
}
