using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OpenRadar.Tasks;

public static class TaskPlateInfoFetch
{
    public static void Enqueue(ulong contentId)
    {
            P.taskManager.Enqueue(() => PlateInfoFetch(contentId));
    }

    private unsafe static bool PlateInfoFetch(ulong contentId)
    {
        if (!EzThrottler.Throttle("PlateInfo", 1000))
            return false; 
        Svc.Log.Debug($"3 - Fetching and Parsing Player Packet {contentId}");
        // yes im opening the adventurer card and not showing it, idk how to simulate the zoneup packet without opening the addon
        var agentCharaCard = AgentCharaCard.Instance();
        agentCharaCard->OpenCharaCard(contentId);
        agentCharaCard->Hide();
        Network.FailedContentId = contentId;

        return true;
    }
}