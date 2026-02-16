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
        if (!EzThrottler.Throttle("PlateInfo", 700))
            return false; 

        Svc.Log.Debug("3 - Fetching and Parsing Player Packet");
        // yes im opening the adventurer card and not showing it LOL
        var agentCharaCard = AgentCharaCard.Instance();
        agentCharaCard->OpenCharaCard(contentId);
        agentCharaCard->Hide();

        return true;
    }
}