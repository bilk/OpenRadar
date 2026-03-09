using System;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Serilog;

namespace OpenRadar;

public unsafe partial class Memory : IDisposable
{
    private Hook<AgentLookingForGroup.Delegates.PopulateListingData> populateListingHook = null!;
    private Hook<RaptureLogModule.Delegates.ShowLogMessage> showLogMessageHook = null!;

    public void PopulateListingDataDetour(AgentLookingForGroup* thisPtr, AgentLookingForGroup.Detailed* listingData)
    {
        CurrentPost = *listingData;
        Tasker.Start();
        populateListingHook.Original(thisPtr, listingData);
    }

    private void showLogMessageDetour(RaptureLogModule* thisPtr, uint logMessageId)
    {
        if (logMessageId is > 5854 and < 5861) // logMessageIds of characard packet error, can prevent it showing and invoke event at same time
        {
            OnCharaCardReceived?.Invoke(null);
            return;
        }

        showLogMessageHook.Original(thisPtr, logMessageId);
    }
}