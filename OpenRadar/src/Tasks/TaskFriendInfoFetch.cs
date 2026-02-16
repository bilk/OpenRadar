using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OpenRadar.Tasks;

public static class TaskFriendInfoFetch
{
    public static void Enqueue(ulong contentId)
    {
            P.taskManager.Insert(() => FriendInfoFetch(contentId));
    }

    private unsafe static bool FriendInfoFetch(ulong contentId)
    {
        if (!EzThrottler.Throttle("FriendInfo", 3700))
            return false; 
        Svc.Log.Debug($"4 - Fetching and Parsing Friend Packet: {contentId}");
        // apparently you can request player's info without being a friend through RequestFriendInfo 
        AgentFriendlist.Instance()->RequestFriendInfo(contentId);

        return true;
    }
}