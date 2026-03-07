using System.Threading.Tasks;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OpenRadar;

public static class Tasker
{
    public static void Start()
    {
        if (CurrentPost is not { } post) return;

        ListingPlayers = new PlayerInfo?[8];

        for (int i = 0; i < 8; i++)
            ListingPlayers[i] = new PlayerInfo(post.MemberContentIds[i]);

        //_ = PopulateAllPlayersAsync();
        // Find locally, check if some missing, if so send requests
    }


    private static async Task<PlayerInfo?> TryGetPlayerFromOpenRadarDB(ulong contentId)
    {
        // Try call async from database
    }

    private static async Task<PlayerInfo?> TryGetPlayerFromPlayerTrackDB(ulong contentId)
    {
        // Try call async from database
    }

    private static async Task<PlayerInfo?> RequestCharaCardAsync(ulong contentId)
    {
        await TaskThrottle("RequestCharaCard", 900);

        var tcs = new TaskCompletionSource<PlayerInfo?>();

        void Handler(PlayerInfo? info)
        {
            if (info == null || info.contentId == contentId)
            {
                Memory.OnCharaCardReceived -= Handler;
                tcs.TrySetResult(info);
            }
        }

        Memory.OnCharaCardReceived += Handler;

        await Svc.Framework.RunOnFrameworkThread(() => P.Memory.RequestPlateInfo(contentId));

        return await tcs.Task;
    }

    private static async Task<PlayerInfo?> RequestFriendInfoAsync(ulong contentId)
    {
        await TaskThrottle("RequestFriendInfo", 3700);

        var tcs = new TaskCompletionSource<PlayerInfo?>();

        void Handler(PlayerInfo? info)
        {
            if (info == null || info.contentId == contentId)
            {
                Memory.OnFriendInfoReceived -= Handler;
                tcs.TrySetResult(info);
            }
        }

        Memory.OnFriendInfoReceived += Handler;

        await Svc.Framework.RunOnFrameworkThread(() =>
        {   
            unsafe {
                var instance = AgentFriendlist.Instance();
                if (instance != null) instance->RequestFriendInfo(contentId);
            }
        });

        return await tcs.Task;
    }

    private static async Task TaskThrottle(string name, int ms)
    {
        while (!EzThrottler.Throttle(name, ms))
            await Task.Delay(50);
    }
}