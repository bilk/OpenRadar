using ECommons.Throttlers;

namespace OpenRadar.Tasks;

public static class TaskPlateInfoFetch
{
    public static void Enqueue(ulong contentId)
    {
        P.taskManager.Enqueue(() => PlateInfoFetch(contentId), "PlateInfo");
    }

    private unsafe static bool PlateInfoFetch(ulong contentId)
    {
        if (!EzThrottler.Throttle("PlateInfo", 900))
            return false; 
        ToastHandler.handled = true;
        ChatHandler.handled = true;
        Svc.Log.Debug($"3 - Fetching and Parsing Player Packet {contentId}");
        P.Memory.RequestPlateInfo(contentId);
        Network.FailedContentId = contentId;
        return true;
    }
}