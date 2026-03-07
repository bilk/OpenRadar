using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OpenRadar.Tasks;

public static class TaskFriendInfoFetch
{
    public static void Enqueue(ulong contentId)
    {
        // Doesnt seem to put the task right to the beginning. Puts it behind the task that is currently being throttled.
        // Unsure if it can be changed but would reduce the overall time to fetch players.
        P.taskManager.Insert(() => FriendInfoFetch(contentId), "FriendInfo");
    }

    private unsafe static bool FriendInfoFetch(ulong contentId)
    {
        if (!C.RequestPackets)
            return true;
        if (!EzThrottler.Throttle("FriendInfo", 3700))
            return false; 
        if (contentId != 0)
        {
            Svc.Log.Debug($"4 - Fetching and Parsing Friend Packet: {contentId}");
            // apparently you can request player's info without being a friend through RequestFriendInfo 
            AgentFriendlist.Instance()->RequestFriendInfo(contentId);
        }

        return true;
    }
}