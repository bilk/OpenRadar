namespace OpenRadar.Tasks;

public static class TaskLocalDataQuery
{
    public static void Enqueue(ulong contentId)
    {
        P.taskManager.Enqueue(() => LocalDataQuery(contentId));
    }

    private static void LocalDataQuery(ulong contentId)
    {
        Svc.Log.Debug($"2 - Querying Local Database: {contentId}");
        var playerInfo = Database.GetPlayerByContentId(contentId);
        if (playerInfo == null)
        {
            TaskPlateInfoFetch.Enqueue(contentId);
        }
        else
        {
            Data.UpdatePlayerList(playerInfo);
        }
    }
}