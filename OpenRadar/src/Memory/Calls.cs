using System;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace OpenRadar;

public unsafe partial class Memory : IDisposable
{
    private delegate void RequestPlateInfoDelegate(ulong* thisPtr, ulong contentId);
    private RequestPlateInfoDelegate requestPlateInfoCall = null!;
    public void RequestPlateInfo(ulong contentId) => requestPlateInfoCall?.Invoke((ulong*)0, contentId);

    private void ResolveRequestCharaCard() // Sig for request does not work with dalamud's sigscanner, so using address for OpenCharaCard then offset
    {
        var openAddr = AgentCharaCard.Addresses.OpenCharaCardForContentId.Value;
        if (openAddr == IntPtr.Zero)
            throw new Exception("OpenCharaCardForContentId not found. Needs update?");

        var callAddr = openAddr + 0x58; // e8 93 ab e0 ff
        if (*(byte*)callAddr != 0xE8) // check if its actually a call
            throw new Exception("callAddr is not call");
        int rel = *(int*)(callAddr + 0x1); // 93 ab e0 ff is relative address to call
        var target = callAddr + 0x5 + rel; // add address after call and relative address for true function address: 140b63df0
        
        Util.Log($"Open: {openAddr:X} - Call: {callAddr:X} - Rel: {rel:X} - Target: {target:X}");

        requestPlateInfoCall = Marshal.GetDelegateForFunctionPointer<RequestPlateInfoDelegate>(target); // converts address to function delegate
    }
}