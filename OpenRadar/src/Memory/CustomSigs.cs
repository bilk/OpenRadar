using System;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using InteropGenerator.Runtime.Attributes;

namespace OpenRadar;

public unsafe partial class Memory : IDisposable
{
    private delegate void FriendInfoPacketHandlerDelegate(InfoModule* infoModulePtr, long* dataPtr, ulong param_3);
    [Signature("E8 ?? ?? ?? ?? 49 8D 9D ?? ?? ?? ?? BF", DetourName = nameof(FriendInfoPacketHandlerDetour))]
    private Hook<FriendInfoPacketHandlerDelegate> friendInfoPacketHandlerHook = null!;

    private void FriendInfoPacketHandlerDetour(InfoModule* infoModulePtr, long* dataPtr, ulong param_3)
    {
        OnFriendInfoReceived?.Invoke(new (*((ulong*)dataPtr+1), Util.ReadUtf8String((byte*)dataPtr+22), *((ushort*)dataPtr + 8)));
        friendInfoPacketHandlerHook.Original(infoModulePtr, dataPtr, param_3);
    }
}