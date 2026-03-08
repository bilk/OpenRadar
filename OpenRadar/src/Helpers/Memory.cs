using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OpenRadar;

public unsafe class Memory : IDisposable
{
    
    private delegate void RequestPlateInfoDelegate(ulong* thisPtr, ulong contentId);
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
        Tasker.Start();
        populateListingHook.Original(thisPtr, listingData);
    }

    public void RequestPlateInfo(ulong contentId)
        => requestPlateInfoCall?.Invoke((ulong*)0, contentId);

    private void ResolveRequestCharaCard() // Sig for request does not work with dalamud's sigscanner, so using address for OpenCharaCard then offset
    {
        var openAddr = AgentCharaCard.Addresses.OpenCharaCardForContentId.Value;
        if (openAddr == IntPtr.Zero)
            throw new Exception("OpenCharaCardForContentId not found");

        var callAddr = openAddr + 0x58;
        

        int rel = *(int*)(callAddr + 0x1);
        var target = callAddr + 0x5 + rel;
        if (*(byte*)callAddr != 0xE8)
            throw new Exception("CALL instruction not found");
        Util.Log($"Open: {openAddr:X} - Call: {callAddr:X} - Target: {target:X} - Rel: {rel:X}");

        requestPlateInfoCall = Marshal.GetDelegateForFunctionPointer<RequestPlateInfoDelegate>(target);
    }

    public Memory()
    {
        Svc.Hook.InitializeFromAttributes(this);
        populateListingHook = Svc.Hook.HookFromAddress(AgentLookingForGroup.Addresses.PopulateListingData.Value,
            new AgentLookingForGroup.Delegates.PopulateListingData(PopulateListingDataDetour));

        ResolveRequestCharaCard();
        populateListingHook.Enable();
        friendInfoPacketHandlerHook.Enable();
        errorPacketHandlerHook.Enable();
        charaCardPacketHandlerHook.Enable();
    }

    public void Dispose()
    {
        populateListingHook.Dispose();
        friendInfoPacketHandlerHook.Dispose();
        errorPacketHandlerHook.Dispose();
        charaCardPacketHandlerHook.Dispose();
    }
}