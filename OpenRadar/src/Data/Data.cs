using System.Collections.Generic;
using Lumina.Excel.Sheets;

namespace OpenRadar;

public static class Data
{
    public static List<ulong> ExtractedContentIds = Enumerable.Repeat<ulong>(0, 8).ToList();
    public static List<PlayerInfo?> ExtractedPlayers = Enumerable.Repeat<PlayerInfo?>(null, 8).ToList();

    public static void UpdatePlayerList(PlayerInfo? playerInfo)
    {
        if (playerInfo == null)
            return;
        int index = ExtractedContentIds.IndexOf(playerInfo.content_id);

        if (index >= 0 && index < ExtractedPlayers.Count)
        {
            ExtractedPlayers[index] = playerInfo;
        }
        else
        {
            Svc.Log.Debug($"ContentId {playerInfo.content_id} not found in ExtractedContentIds.");
        }
    }

    public static void ResetExtractedData()
    {
        ExtractedContentIds = Enumerable.Repeat<ulong>(0, 8).ToList();
        ExtractedPlayers = Enumerable.Repeat<PlayerInfo?>(null, 8).ToList();
    }

    public class PlayerInfo
    {
        public PlayerInfo(ulong c, string? n, ushort w)
        {
            content_id = c;
            name = n;
            world = w;
        }
        public ulong content_id { get; set; }
        public string? name { get; set; }
        public ushort world { get; set; }
    }

    public class ListingInformation
    {
        public ulong hostContentId { get; set; }
        public List<ClassJob> jobsPresent { get; set; } = new();
        public ContentFinderCondition duty { get; set; }
        public string? description { get; set; }
        public string? hostName { get; set; }
        public string? hostWorld { get; set; }
        public int slotCount { get; set; }
    }
}