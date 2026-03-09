using System;
using System.Collections.Generic;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace OpenRadar;

public unsafe partial class Memory : IDisposable
{
    public static event Action<PlayerInfo?>? OnFriendInfoReceived;
    public static event Action<PlayerInfo?>? OnCharaCardReceived;

    public Memory()
    {
        Svc.Hook.InitializeFromAttributes(this);
        populateListingHook = Svc.Hook.HookFromAddress(AgentLookingForGroup.Addresses.PopulateListingData.Value,
            new AgentLookingForGroup.Delegates.PopulateListingData(PopulateListingDataDetour));
        showLogMessageHook = Svc.Hook.HookFromAddress(RaptureLogModule.Addresses.ShowLogMessage.Value,
            new RaptureLogModule.Delegates.ShowLogMessage(showLogMessageDetour));
        ResolveRequestCharaCard();

        EnableHooks();
    }

    public void EnableHooks()
    {
        populateListingHook.Enable();
        showLogMessageHook.Enable();
        friendInfoPacketHandlerHook.Enable();
        charaCardPacketHandlerHook.Enable();
    }

    public void Dispose() // automatically disables
    {
        populateListingHook.Dispose();
        showLogMessageHook.Dispose();
        friendInfoPacketHandlerHook.Dispose();
        charaCardPacketHandlerHook.Dispose();
    }
}   