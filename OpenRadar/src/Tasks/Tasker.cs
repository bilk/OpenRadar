using System;
using System.Threading;
using System.Threading.Tasks;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Serilog;

namespace OpenRadar;

public static class Tasker
{
    public static void Start()
    {
        if (CurrentPost is not { } post) return;
        if (post.JoinConditionFlags.HasFlag(AgentLookingForGroup.JoinCondition.PrivateParty)) return;

        ListingPlayers = new PlayerInfo?[8];

        for (int i = 0; i < 8; i++)
            ListingPlayers[i] = new PlayerInfo(post.MemberContentIds[i], jobId: post.Jobs[i]);

        _ = FindAndPopulatePlayers();
    }

    private static async Task FindAndPopulatePlayers()
        => await Task.WhenAll(ListingPlayers.Where(p => p != null && p.contentId != 0).Select(p => PopulatePlayer(p!)));

    private static async Task PopulatePlayer(PlayerInfo player)
    {
        Util.Log($"Searching ORDB: {player.contentId}");
        if (await TryGetPlayerFromOpenRadarDB(player.contentId))
            return;

        Util.Log($"Searching PTDB: {player.contentId}");
        if (await TryGetPlayerFromPlayerTrackDB(player.contentId))
            return;

        await RequestPlayerInfo(player.contentId);
    }

    private static readonly SemaphoreSlim RequestGate = new(1, 1);

    private static async Task RequestPlayerInfo(ulong contentId)
    {
        await RequestGate.WaitAsync();

        try
        {
            var info = await RequestCharaCardAsync(contentId);
            info ??= await RequestFriendInfoAsync(contentId);

            PopulateListingPlayers(info);
        }
        finally
        {
            RequestGate.Release();
        }
    }

    private static async Task<bool> TryGetPlayerFromOpenRadarDB(ulong contentId)
    {
        if (await Database.GetPlayerORAsync(contentId) is { } player)
        {
            PopulateListingPlayers(player, false);
            return true;
        }
        return false;
    }

    private static async Task<bool> TryGetPlayerFromPlayerTrackDB(ulong contentId)
    {
        if (await Database.GetPlayerPTAsync(contentId) is { } player)
        {
            PopulateListingPlayers(player);
            return true;
        }
        return false;
    }

    private static async Task<PlayerInfo?> RequestCharaCardAsync(ulong contentId)
    {
        await Throttle("RequestCharaCard", 900);
        var tcs = new TaskCompletionSource<PlayerInfo?>(TaskCreationOptions.RunContinuationsAsynchronously);

        void Handler(PlayerInfo? info)
        {
            if (info == null || info.contentId == contentId)
            {
                tcs.TrySetResult(info);
            }
        }


        Memory.OnCharaCardReceived += Handler;
        Util.Log($"Requesting Characard: {contentId}");
        await Svc.Framework.RunOnFrameworkThread(() => P.Memory.RequestPlateInfo(contentId));
        
        try
        {   
            return await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            Memory.OnCharaCardReceived -= Handler;
        }
    }

    private static async Task<PlayerInfo?> RequestFriendInfoAsync(ulong contentId)
    {
        await Throttle("RequestFriendInfo", 3700);

        var tcs = new TaskCompletionSource<PlayerInfo?>();

        void Handler(PlayerInfo? info)
        {
            if (info == null || info.contentId == contentId)
            {
                tcs.TrySetResult(info);
            }
        }

        Memory.OnFriendInfoReceived += Handler;

        Util.Log($"Requesting FriendInfo: {contentId}");
        await Svc.Framework.RunOnFrameworkThread(() =>
        {   
            unsafe {
                var instance = AgentFriendlist.Instance();
                if (instance != null) instance->RequestFriendInfo(contentId);
            }
        });

        try
        {
            return await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            Memory.OnCharaCardReceived -= Handler;
        }
    }

    private static async Task Throttle(string name, int ms)
    {
        while (!EzThrottler.Throttle(name, ms))
            await Task.Delay(50);
    }
}