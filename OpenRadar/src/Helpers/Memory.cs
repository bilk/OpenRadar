using System;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using OpenRadar.Tasks;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OpenRadar;

public unsafe class Memory : IDisposable
{

    private delegate void RequestPlateInfoDelegate(ulong* thisPtr, ulong contentId);
    [Signature("40 53 48 81 EC 80 0F 00 00 48 8B 05 ? ? ? ? 48 33 C4 48 89 84 24 70 0F 00 00 48 8B 0D ? ? ? ? 48 8B DA E8 ? ? ? ? 45 33 C9 C7 44 24 20 5E 03 00 00 45 33 C0 48 C7 44 24 28 20 00 00 00 48 8D 54 24 20 48 89 5C 24 40 48 8B C8 C7 44 24 48 01 00 00 00")]
    private RequestPlateInfoDelegate requestPlateInfoCall = null!;

    private delegate void FriendInfoPacketHandlerDelegate(ulong param_1, long* dataPtr, ulong param_3);
    [Signature("E8 ?? ?? ?? ?? 49 8D 9D ?? ?? ?? ?? BF", DetourName = nameof(FriendInfoPacketHandlerDetour))]
    private Hook<FriendInfoPacketHandlerDelegate> friendInfoPacketHandlerHook = null!;

    private delegate void ErrorPacketHandlerDelegate(ulong* param_1, int* param_2);
    [Signature("E8 ?? ?? ?? ?? 88 9E ?? ?? ?? ?? E9 ?? ?? ?? ?? 81 E9 ?? ?? ?? ?? 0F 84", DetourName = nameof(ErrorPacketHandlerDetour))]
    private Hook<ErrorPacketHandlerDelegate> errorPacketHandlerHook = null!;

    private delegate void CharaCardPacketHandlerDelegate(ulong param_1, AgentCharaCard.CharaCardPacket* packetPtr);
    [Signature("40 53 48 83 EC ?? 8B 05 ?? ?? ?? ?? 48 8B DA", DetourName = nameof(CharaCardPacketHandlerDetour))]
    private Hook<CharaCardPacketHandlerDelegate> charaCardPacketHandlerHook = null!;

    private Hook<AgentLookingForGroup.Delegates.PopulateListingData> populateListingHook = null!;
    
    public static event Action<PlayerInfo?>? OnFriendInfoReceived;
    private void FriendInfoPacketHandlerDetour(ulong param_1, long* dataPtr, ulong param_3)
    {
        OnFriendInfoReceived?.Invoke(new (*((ulong*)dataPtr+1), Util.ReadUtf8String((byte*)dataPtr+22), *((ushort*)dataPtr + 8)));
        friendInfoPacketHandlerHook.Original(param_1, dataPtr, param_3);
    }

    public static event Action<PlayerInfo?>? OnCharaCardReceived;
    private void ErrorPacketHandlerDetour(ulong* param_1, int* dataPtr)
    {
        // errorFlag is 0x27 when characard fail
        if (*(byte*)((long)dataPtr + 6) == 0x27)
            OnCharaCardReceived?.Invoke(null);
        errorPacketHandlerHook.Original(param_1, dataPtr);
    }

    private void CharaCardPacketHandlerDetour(ulong param_1, AgentCharaCard.CharaCardPacket* dataPtr)
    {
        OnCharaCardReceived?.Invoke(new (dataPtr->ContentId, dataPtr->NameString, dataPtr->WorldId));
        charaCardPacketHandlerHook.Original(param_1, dataPtr);
    }

    public void PopulateListingDataDetour(AgentLookingForGroup* thisPtr, AgentLookingForGroup.Detailed* listingData)
    {
        CurrentPost = *listingData;
        populateListingHook.Original(thisPtr, listingData);
    }

    public void RequestPlateInfo(ulong contentId)
        => requestPlateInfoCall((ulong*)0, contentId);

    public Memory()
    {
        Svc.Hook.InitializeFromAttributes(this);
        Svc.Hook.HookFromAddress(AgentLookingForGroup.Addresses.PopulateListingData.Value,
            new AgentLookingForGroup.Delegates.PopulateListingData(PopulateListingDataDetour));
        populateListingHook.Enable();
        friendInfoPacketHandlerHook.Enable();
        errorPacketHandlerHook.Enable();
        charaCardPacketHandlerHook.Enable();
    }

    public void Dispose()
    {
        friendInfoPacketHandlerHook.Disable();
        friendInfoPacketHandlerHook.Dispose();
        errorPacketHandlerHook.Disable();
        errorPacketHandlerHook.Dispose();
        charaCardPacketHandlerHook.Disable();
        charaCardPacketHandlerHook.Dispose();
        populateListingHook.Disable();
        populateListingHook.Dispose();
    }
}