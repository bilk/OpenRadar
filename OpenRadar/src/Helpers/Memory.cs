using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Hooking;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Utility.Signatures;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.UI;
using OpenRadar.Tasks;
using SQLitePCL;





namespace OpenRadar;

public unsafe class Memory : IDisposable
{
    private delegate void RequestPlateInfoDelegate(ulong param_1, ulong contentId);
    [Signature("40 53 48 81 EC 80 0F 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 70 0F 00 00 48 8B 0D ?? ?? ?? ?? 48 8B DA E8 ?? ?? ?? ?? 45 33 C9 C7 44 24 20 B0 00 00 00 45 33 C0 48 C7 44 24 28 20 00 00 00 48 8D 54 24 20 48 89 5C 24 40 48 8B C8 C7 44 24 48 01 00 00 00")]
    private RequestPlateInfoDelegate getRequestPlateInfo = null!;

    private delegate void OnPostPacketReceiveDelegate(ulong param_1, long* param_2);
    [Signature("48 89 5C 24 18 55 48 8D AC 24 60 FC FF FF", DetourName = nameof(OnPostReceiveDetour))]
    private Hook<OnPostPacketReceiveDelegate> onPostPacketReceiveHook = null!;

    private delegate void OnFriendInfoPacketReceiveDelegate(ulong param_1, long* dataPtr, ulong param_3);
    [Signature("E8 ?? ?? ?? ?? 49 8D 9D ?? ?? ?? ?? BF", DetourName = nameof(OnFriendInfoPacketReceiveDetour))]
    private Hook<OnFriendInfoPacketReceiveDelegate> onFriendInfoPacketReceiveHook = null!;

    private delegate void OnPlateInfoPacketFailDelegate(ulong* param_1, int* param_2);
    [Signature("E8 ?? ?? ?? ?? 88 9E ?? ?? ?? ?? E9 ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 0F 84 ?? ?? ?? ?? 49 83 C6", DetourName = nameof(OnPlateInfoPacketFailDetour))]
    private Hook<OnPlateInfoPacketFailDelegate> onPlateInfoPacketFailHook = null!;

    private delegate void HandleCurrentCharaCardDataPacketDelegate(ulong param_1, long param_2);
    [Signature("40 53 48 83 EC ?? 8B 05 ?? ?? ?? ?? 48 8B DA", DetourName = nameof(HandleCurrentCharaCardDataPacketDetour))]
    private Hook<HandleCurrentCharaCardDataPacketDelegate> handleCurrentCharaCardDataPacketHook = null!;

    private void HandleCurrentCharaCardDataPacketDetour(ulong param_1, long dataPtr)
    {
        var contentId = *((ulong*)dataPtr+2);
        var playerName = Util.ReadUtf8String((byte*)dataPtr + 421);
        ushort worldId = *((ushort*)dataPtr + 16);

        var playerInfo = new PlayerInfo(contentId, playerName, worldId);
        if (playerInfo!=null)
        {
            Database.AddPlayer(playerInfo);           
            Data.UpdatePlayerList(playerInfo);
        }

        handleCurrentCharaCardDataPacketHook.Original(param_1, dataPtr);
    }

    private void OnPlateInfoPacketFailDetour(ulong* param_1, int* param_2)
    {
        TaskFriendInfoFetch.Enqueue(Network.FailedContentId);
        onPlateInfoPacketFailHook.Original(param_1, param_2);
    }

    private void OnFriendInfoPacketReceiveDetour(ulong param_1, long* dataPtr, ulong param_3)
    {
        ulong contentId = *((ulong*)dataPtr+1);
        string name = Util.ReadUtf8String((byte*)dataPtr+22);
        ushort worldId = *((ushort*)dataPtr + 8);
        PlayerInfo playerInfo = new PlayerInfo(contentId, name, worldId);

        Database.AddPlayer(playerInfo);
        Data.UpdatePlayerList(playerInfo);
        onFriendInfoPacketReceiveHook.Original(param_1, dataPtr, param_3);
    }


    private void OnPostReceiveDetour(ulong param_1, long* packetPtr)
    {
        ushort dutyId = *((ushort*)packetPtr + 20);
        CurrentPost = new PostInfo(dutyId, new List<ISharedImmediateTexture?>(), new List<IDalamudTextureWrap?>(), new List<ulong>());
        for (int i = 0; i < 8; i++)
        {
            ulong content_id = *((ulong*)packetPtr + i + 12);
            byte jobId = *((byte*)packetPtr + i + 224);
            ulong slotAccepting = *((ulong*)packetPtr + i + 20);

            CurrentPost.contentIds.Add(content_id);
            CurrentPost.jobIcons.Add(Util.GetJobIcon(jobId));
            CurrentPost.roleIcons.Add(Util.JobFlagsToRoleTexture((JobFlags)slotAccepting));

            TaskLocalDataQuery.Enqueue(content_id);
        }
        onPostPacketReceiveHook.Original(param_1, packetPtr);
    }

    public void RequestPlateInfo(ulong contentId)
    {
        getRequestPlateInfo(0, contentId);
    }

    public Memory()
    {
        Svc.Hook.InitializeFromAttributes(this);
        onPostPacketReceiveHook.Enable();
        onFriendInfoPacketReceiveHook.Enable();
        onPlateInfoPacketFailHook.Enable();
        handleCurrentCharaCardDataPacketHook.Enable();
    }

    public void Dispose()
    {
        onPostPacketReceiveHook.Disable();
        onPostPacketReceiveHook.Dispose();
        onFriendInfoPacketReceiveHook.Disable();
        onFriendInfoPacketReceiveHook.Dispose();
        onPlateInfoPacketFailHook.Disable();
        onPlateInfoPacketFailHook.Dispose();
        handleCurrentCharaCardDataPacketHook.Disable();
        handleCurrentCharaCardDataPacketHook.Dispose();
    }
}