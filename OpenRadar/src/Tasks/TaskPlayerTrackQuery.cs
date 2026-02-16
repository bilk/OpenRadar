namespace OpenRadar.Tasks;

public static class TaskPlayerTrackQuery
{
    public static void Enqueue(ulong contentId)
    {
        P.taskManager.Enqueue(() => QueryPlayerTrack(contentId));
    }

    private static void QueryPlayerTrack(ulong contentId)
    {
        if (contentId != 0)
        {
            Svc.Log.Debug($"1 - Querying PlayerTrack: {contentId}");
            var playerInfo = PlayerTrackInterop.Extract(contentId);
            if (playerInfo == null)
            {
                TaskLocalDataQuery.Enqueue(contentId);
            }
            else
            {
                Data.UpdatePlayerList(playerInfo);
            }
        }
    }
}