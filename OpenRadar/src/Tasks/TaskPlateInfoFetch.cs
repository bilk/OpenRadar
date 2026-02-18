using System.ComponentModel.Design;
using Dalamud.Game;
using Dalamud.Plugin.SelfTest;
using ECommons.EzHookManager;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OpenRadar.Tasks;

public static class TaskPlateInfoFetch
{
    public static void Enqueue(ulong contentId)
    {
        P.taskManager.Enqueue(() => PlateInfoFetch(contentId), "PlateInfo");
    }

    private unsafe static bool PlateInfoFetch(ulong contentId)
    {
        if (!EzThrottler.Throttle("PlateInfo", 200))
            return false; 
        Svc.Log.Debug($"3 - Fetching and Parsing Player Packet {contentId}");
        P.Memory.RequestPlateInfo(contentId);
        Network.FailedContentId = contentId;
        return true;
    }
}