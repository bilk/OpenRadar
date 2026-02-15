using System.Collections.Generic;
using Lumina.Excel.Sheets;

namespace OpenRadar;

public static class Data
{
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

    public class PlayerInfo
    {
        public ulong content_id { get; set; }
        public string? name { get; set; }
        public string? world { get; set; }
    }
}