using System.Collections.Generic;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Openradar;

namespace OpenRadar;

public static class Data
{
    public static PostInfo CurrentPost = new PostInfo(0, false, new List<ISharedImmediateTexture?>(), new List<IDalamudTextureWrap?>(), new List<ulong>());
    public static List<PlayerInfo?> ExtractedPlayers = Enumerable.Repeat<PlayerInfo?>(null, 8).ToList();
    public static List<string?> ProgPoints = Enumerable.Repeat<string?>(null, 8).ToList();

    public static void UpdatePlayerList(PlayerInfo? playerInfo)
    {
        if (playerInfo == null)
            return;
        int index = CurrentPost.contentIds.IndexOf(playerInfo.content_id);

        if (index >= 0 && index < ExtractedPlayers.Count)
        {
            ExtractedPlayers[index] = playerInfo;
            if (C.QueryTomestone)
            {
                Tomestone.GetPlayerProg(playerInfo, index);
            }
        }
        else
        {
            Svc.Log.Debug($"ContentId {playerInfo.content_id} not found in ExtractedContentIds.");
        }
    }

    public static void ResetExtractedData()
    {
        //ExtractedContentIds = Enumerable.Repeat<ulong>(0, 8).ToList();
        CurrentPost = new PostInfo(0, false, new List<ISharedImmediateTexture?>(), new List<IDalamudTextureWrap?>(), new List<ulong>());
        ExtractedPlayers = Enumerable.Repeat<PlayerInfo?>(null, 8).ToList();
        ProgPoints = Enumerable.Repeat<string?>(null, 8).ToList();
    }

    public record PlayerInfo
    (
        ulong content_id,
        string? name,
        ushort world
    );

    public record PostInfo
    (
        ushort dutyId,
        bool isPrivate,
        //List<byte> jobIds,
        List<ISharedImmediateTexture?> jobIcons,
        List<IDalamudTextureWrap?> roleIcons,
        //List<JobFlags> acceptingJobs,
        List<ulong> contentIds
    );
}