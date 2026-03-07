using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Lumina.Excel.Sheets;
using System;
using System.Diagnostics;

namespace OpenRadar;

public static partial class Util
{
    public static ISharedImmediateTexture? GetJobIcon(byte? jobId)
    {
        if (jobId == 0 || jobId == null) return null;

        return Svc.Texture.GetFromGameIcon(62100 + (uint)jobId);
    }

    public static IDalamudTextureWrap? JobFlagsToRoleTexture(JobFlags jobFlags)
    {
        bool isTank = (jobFlags & JobRoles.Tanks) != 0;
        bool isHealer = (jobFlags & JobRoles.Healers) != 0;
        bool isDps = (jobFlags & JobRoles.DPS) != 0;

        int roleMask = (isTank ? 1 : 0) | (isHealer ? 2 : 0) | (isDps ? 4 : 0);

        var iconId = roleMask switch
        {
            1 => 17,
            2 => 18,
            4 => 19,
            3 => 30,
            5 => 31,
            6 => 32,
            7 => 33,
            _ => 20
        };

        var uld = Svc.PluginInterface.UiBuilder.LoadUld("ui/uld/lfgselectrole.uld"); 

        if (uld == null) return null;

        return uld.LoadTexturePart("ui/uld/LFG.tex", iconId);
    }

    /// <summary>
    /// Prints data within given pointer to xllog. Allows for any type.
    /// </summary>
    public unsafe static void PrintData<T>(void* dataPtr, int totalRows = 10, int infoPerRow = 10) where T : unmanaged
    {
#if DEBUG
        if (dataPtr == null) return;
        T* ptr = (T*)dataPtr;

        Svc.Log.Debug("──────── Data Start ────────");
        for (int row = 0; row < totalRows; row++)
        {
            string packetInfoRow = $"{row}: ";
            for (int col = 0; col < infoPerRow; col++)
            {
                T dataPoint = ptr[row * infoPerRow + col];
                packetInfoRow += $"{dataPoint} ";
            }
            Svc.Log.Debug(packetInfoRow);
        }
        Svc.Log.Debug("──────── Data End ────────");
#endif
    }

    public unsafe static string ReadUtf8String(byte* b, int maxLength = 30, bool endAtNull = true)
    {
        int len = 0;
        if (endAtNull)
            while (len < maxLength && b[len] != 0)
                len++;
        else
            len = maxLength;

        return System.Text.Encoding.UTF8.GetString(b, len);
    }


    public static bool ContainsSeString(SeString seString, string part)
        => seString.TextValue.Contains(part, StringComparison.OrdinalIgnoreCase);

    public static string? WorldIdToName(ushort worldId)
    {
        var world = Svc.Data.GetExcelSheet<World>().FirstOrNull(w => w.RowId == worldId);
        if (world == null) return null;

        return world.Value.InternalName.ToString();
    }

    public static string DutyIdToName(ushort dutyId)
    {
        var duty = Svc.Data.GetExcelSheet<ContentFinderCondition>().FirstOrNull(duty => duty.RowId == dutyId);
        if (duty == null) return "Unknown Duty";

        var dutyName = duty.Value.Name.ToString();
        return char.ToUpper(dutyName[0]) + dutyName.Substring(1);
    }

    public static string? WorldToRegion(ushort worldId)
    {
        var world = Svc.Data.GetExcelSheet<World>().FirstOrNull(w => w.RowId == worldId);
        if (world == null) return null;

        return world.Value.DataCenter.Value.Region switch
        {
            1 => "JP",
            2 => "NA",
            3 => "EU",
            4 => "OC",
            _ => null
        };
    }
}
