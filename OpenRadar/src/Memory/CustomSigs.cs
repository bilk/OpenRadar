using System;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace OpenRadar;

public unsafe partial class Memory : IDisposable
{
    private delegate void FriendInfoPacketHandlerDelegate(ulong param_1, long* dataPtr, ulong param_3);
    [Signature("E8 ?? ?? ?? ?? 49 8D 9D ?? ?? ?? ?? BF", DetourName = nameof(FriendInfoPacketHandlerDetour))]
    private Hook<FriendInfoPacketHandlerDelegate> friendInfoPacketHandlerHook = null!;

    private delegate void CharaCardPacketHandlerDelegate(ulong param_1, AgentCharaCard.CharaCardPacket* packetPtr);
    [Signature("40 53 48 83 EC ?? 8B 05 ?? ?? ?? ?? 48 8B DA", DetourName = nameof(CharaCardPacketHandlerDetour))]
    private Hook<CharaCardPacketHandlerDelegate> charaCardPacketHandlerHook = null!;

    private void FriendInfoPacketHandlerDetour(ulong param_1, long* dataPtr, ulong param_3)
    {
        OnFriendInfoReceived?.Invoke(new (*((ulong*)dataPtr+1), Util.ReadUtf8String((byte*)dataPtr+22), *((ushort*)dataPtr + 8)));
        friendInfoPacketHandlerHook.Original(param_1, dataPtr, param_3);
    }

    private void CharaCardPacketHandlerDetour(ulong param_1, AgentCharaCard.CharaCardPacket* dataPtr)
    {
        OnCharaCardReceived?.Invoke(new (dataPtr->ContentId, dataPtr->NameString, dataPtr->WorldId));
        charaCardPacketHandlerHook.Original(param_1, dataPtr);
    }
}