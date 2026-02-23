using System;
using System.Collections.Generic;
using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Hooking;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Utility.Signatures;
using OpenRadar.Tasks;

namespace OpenRadar;

public unsafe class Memory : IDisposable
{
    private delegate void RequestPlateInfoDelegate(ulong param_1, ulong contentId);
    [Signature("40 53 48 81 EC 80 0F 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 70 0F 00 00 48 8B 0D ?? ?? ?? ?? 48 8B DA E8 ?? ?? ?? ?? 45 33 C9 C7 44 24 20 B0 00 00 00 45 33 C0 48 C7 44 24 28 20 00 00 00 48 8D 54 24 20 48 89 5C 24 40 48 8B C8 C7 44 24 48 01 00 00 00")]
    private RequestPlateInfoDelegate callRequestPlateInfo = null!;

    private delegate void ListingPacketHandlerDelegate(ulong param_1, ulong* param_2);
    [Signature("48 89 5C 24 18 55 48 8D AC 24 60 FC FF FF", DetourName = nameof(ListingPacketHandlerDetour))]
    private Hook<ListingPacketHandlerDelegate> listingPacketHandlerHook = null!;

    private delegate void FriendInfoPacketHandlerDelegate(ulong param_1, long* dataPtr, ulong param_3);
    [Signature("E8 ?? ?? ?? ?? 49 8D 9D ?? ?? ?? ?? BF", DetourName = nameof(FriendInfoPacketHandlerDetour))]
    private Hook<FriendInfoPacketHandlerDelegate> friendInfoPacketHandlerHook = null!;

    private delegate void ErrorPacketHandlerDelegate(ulong* param_1, int* param_2);
    [Signature("E8 ?? ?? ?? ?? 88 9E ?? ?? ?? ?? E9 ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 0F 84 ?? ?? ?? ?? 49 83 C6", DetourName = nameof(ErrorPacketHandlerDetour))]
    private Hook<ErrorPacketHandlerDelegate> errorPacketHandlerHook = null!;

    private delegate void CharaCardPacketHandlerDelegate(ulong param_1, long param_2);
    [Signature("40 53 48 83 EC ?? 8B 05 ?? ?? ?? ?? 48 8B DA", DetourName = nameof(CharaCardPacketHandlerDetour))]
    private Hook<CharaCardPacketHandlerDelegate> charaCardPacketHandlerHook = null!;

    private void ListingPacketHandlerDetour(ulong param_1, ulong* dataPtr)
    {
        // ghidra's packet extractor, lot of these mean something, not sure
        // var local448 = *(uint*)(dataPtr + 8);
        // var local444 = *(uint*)((long)dataPtr + 0x44);
        // var local440 = *(uint*)(dataPtr + 9);
        // var local43c = *(uint*)((long)dataPtr + 0x4c);
        // var local438 = *(ushort*)(dataPtr + 10);
        // var local436 = *(ushort*)((long)dataPtr + 0x52);
        // var local434 = *(ushort*)((long)dataPtr + 0x54);
        // var local432 = *(byte*)((long)dataPtr + 0x56);
        // var local431 = *(byte*)((long)dataPtr + 0x57);
        // var local430 = *(byte*)(dataPtr + 0xb);
        // var local42f = *(byte*)((long)dataPtr + 0x59);
        // var local42e = *(byte*)((long)dataPtr + 0x5a);
        var privateByte = *(byte*)((long)dataPtr + 0x5b);
        // var local42c = *(byte*)((long)dataPtr + 0x5c);
        // var local18 = *(byte*)(dataPtr + 0x39);
        bool isPrivate = privateByte == 3 || privateByte == 35;
        ushort dutyId = *((ushort*)dataPtr + 20);
        CurrentPost = new PostInfo(dutyId, isPrivate, new List<ISharedImmediateTexture?>(), new List<IDalamudTextureWrap?>(), new List<ulong>());
        for (int i = 0; i < 8; i++)
        {
            ulong content_id = *((ulong*)dataPtr + i + 12);
            byte jobId = *((byte*)dataPtr + i + 224);
            ulong slotAccepting = *((ulong*)dataPtr + i + 20);

            CurrentPost.contentIds.Add(content_id);
            CurrentPost.jobIcons.Add(Util.GetJobIcon(jobId));
            CurrentPost.roleIcons.Add(Util.JobFlagsToRoleTexture((JobFlags)slotAccepting));

            TaskLocalDataQuery.Enqueue(content_id);
        }
        listingPacketHandlerHook.Original(param_1, dataPtr);
    }

    private void FriendInfoPacketHandlerDetour(ulong param_1, long* dataPtr, ulong param_3)
    {
        ulong contentId = *((ulong*)dataPtr+1);
        string name = Util.ReadUtf8String((byte*)dataPtr+22);
        ushort worldId = *((ushort*)dataPtr + 8);
        PlayerInfo playerInfo = new PlayerInfo(contentId, name, worldId);

        Database.AddPlayer(playerInfo);
        Data.UpdatePlayerList(playerInfo);
        friendInfoPacketHandlerHook.Original(param_1, dataPtr, param_3);
    }

    private void ErrorPacketHandlerDetour(ulong* param_1, int* param_2)
    {
        // This is ERROR packet handler, used if pf/plate fail (among other network errors)
        var errorFlag = *(byte*)((long)param_2 + 6);
        // errorFlag is 10 dec when pf fail and 0x27 when plate fail
        // case 10 uses FUN_141cd94d0
        // 0x27 isnt case, falls to FUN_140962ce0 (probably with case 10 too)
        // these params are on the heap
        // errorFlag seems unstable, will see if changes per update
        if (errorFlag != 10 && C.RequestPackets)
        {
            TaskFriendInfoFetch.Enqueue(Network.FailedContentId);
        }
        errorPacketHandlerHook.Original(param_1, param_2);
    }

    private void CharaCardPacketHandlerDetour(ulong param_1, long dataPtr)
    {
        ToastHandler.handled = false;
        ChatHandler.handled = false;
        var contentId = *((ulong*)dataPtr+2);
        var playerName = Util.ReadUtf8String((byte*)dataPtr + 421);
        ushort worldId = *((ushort*)dataPtr + 16);

        var playerInfo = new PlayerInfo(contentId, playerName, worldId);
        if (playerInfo!=null)
        {
            Database.AddPlayer(playerInfo);           
            Data.UpdatePlayerList(playerInfo);
        }

        charaCardPacketHandlerHook.Original(param_1, dataPtr);
    }


    public void RequestPlateInfo(ulong contentId)
    {
        callRequestPlateInfo(0, contentId);
    }

    public Memory()
    {
        Svc.Hook.InitializeFromAttributes(this);
        listingPacketHandlerHook.Enable();
        friendInfoPacketHandlerHook.Enable();
        errorPacketHandlerHook.Enable();
        charaCardPacketHandlerHook.Enable();
    }

    public void Dispose()
    {
        listingPacketHandlerHook.Disable();
        listingPacketHandlerHook.Dispose();
        friendInfoPacketHandlerHook.Disable();
        friendInfoPacketHandlerHook.Dispose();
        errorPacketHandlerHook.Disable();
        errorPacketHandlerHook.Dispose();
        charaCardPacketHandlerHook.Disable();
        charaCardPacketHandlerHook.Dispose();
    }
}