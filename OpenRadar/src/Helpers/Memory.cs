using System;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using ECommons.EzHookManager;

namespace OpenRadar;



internal unsafe class Memory
{
    private delegate void RequestPlateInfoDelegate(ulong param_1, ulong contentId);
    [Signature("40 53 48 81 EC 80 0F 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 70 0F 00 00 48 8B 0D ?? ?? ?? ?? 48 8B DA E8 ?? ?? ?? ?? 45 33 C9 C7 44 24 20 B0 00 00 00 45 33 C0 48 C7 44 24 28 20 00 00 00 48 8D 54 24 20 48 89 5C 24 40 48 8B C8 C7 44 24 48 01 00 00 00")]
    private RequestPlateInfoDelegate getRequestPlateInfo = null!;

    public void RequestPlateInfo(ulong contentId)
    {
        getRequestPlateInfo(0, contentId);
    }

    internal Memory()
    {
        Svc.Hook.InitializeFromAttributes(this);
        EzSignatureHelper.Initialize(this);
    }
}