using System.Threading.Tasks;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OpenRadar.Tasks;


public static class FindPlayer
{
    private static ulong _contentIdCache;



    public static void Locate(ulong contentId)
        => Tasker.Enqueue(TryGetPlayerFromOpenRadarDB, contentId);

    private static void TryGetPlayerFromOpenRadarDB(ulong contentId)
    {
        if (Database.GetPlayerByContentId(contentId) is { } playerInfo)
            PopulateListingPlayers(playerInfo);
        else
            Tasker.Enqueue(TryGetPlayerFromPlayerTrackDB, contentId);
    }

    private static void TryGetPlayerFromPlayerTrackDB(ulong contentId)
    {
        if (PlayerTrackInterop.Extract(contentId) is { } playerInfo)
            PopulateListingPlayers(playerInfo);
        else
            Tasker.Enqueue(RequestCharaCard, contentId);
    }

    private static bool RequestCharaCard(ulong contentId)
    {
        if (!EzThrottler.Throttle("RequestCharaCard", 900)) return false; 
        P.Memory.RequestPlateInfo(contentId);
        _contentIdCache = contentId;
        return true;
    }

    private unsafe static bool RequestFriendInfo(ulong contentId)
    {
        if (!EzThrottler.Throttle("RequestFriendInfo", 3700)) return false; 
        AgentFriendlist.Instance()->RequestFriendInfo(contentId);
        return true;
    }

    public static void ResponseCharaCard(PlayerInfo playerInfo)
        => PopulateListingPlayers(playerInfo);

    public static void ResponseCharaCard(ulong contentId)
        => Tasker.Enqueue(RequestFriendInfo, _contentIdCache);

    public static void ResponseFriendInfo(PlayerInfo playerInfo)
        => PopulateListingPlayers(playerInfo);

    private static void OnSuccess(PlayerInfo playerInfo)
    {
        PopulateListingPlayers(playerInfo);
        // Request Tomestone
    }
}