using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Lumina.Excel.Sheets;
using System;
using System.Diagnostics;

namespace OpenRadar;

public static class Util
{
    public static ISharedImmediateTexture? GetJobIcon(byte? jobId)
    {
        if (jobId == 0)
            return null;
        if (jobId != null)
        {
            var jobTexture = Svc.Texture.GetFromGame($"ui/icon/062000/0621{jobId}.tex");
            return jobTexture;
        }
        return null;
    }

    public static IDalamudTextureWrap? JobFlagsToRoleTexture(JobFlags jobFlags)
    {
        bool isTank = (jobFlags & JobRoles.Tanks) != 0;
        bool isHealer = (jobFlags & JobRoles.Healers) != 0;
        bool isDps = (jobFlags & JobRoles.DPS) != 0;

        int roleMask = (isTank ? 1 : 0) | (isHealer ? 2 : 0) | (isDps ? 4 : 0);
        // this seems bad
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

        return Svc.PluginInterface.UiBuilder.LoadUld("ui/uld/lfgselectrole.uld").LoadTexturePart("ui/uld/LFG.tex", iconId);

        // ui/uld/lfg.tex
        // #6 tank, #7 healer, #8 dps, #9 default
        // #10 border
        // #19 tank/healer, #20 tank/dps, #21 healer/dps, #22 tank/healer/dps
    }

    public unsafe static void PrintData<T>(nint dataPtr, int totalRows, int infoPerRow) where T : unmanaged
    {
        T* ptr = (T*)dataPtr;

        for (int row = 0; row < totalRows; row++)
        {
            string packetInfoRow = $"{row}: ";
            for (int col = 0; col < infoPerRow; col++)
            {
                T dataPoint = *(ptr + row * infoPerRow + col);
                packetInfoRow += $"{dataPoint} ";
            }
            Svc.Log.Debug(packetInfoRow);
        }
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
    {
        var fullText = seString.TextValue;
        return fullText.Contains(part, System.StringComparison.OrdinalIgnoreCase);
    }

    public static string WorldIdToName(ushort worldId)
    {
        var world = Svc.Data.GetExcelSheet<World>().First(world => world.RowId == worldId).InternalName.ToString();
        return world;
    }

    public static string DutyIdToName(ushort dutyId)
    {
        var duty = Svc.Data.GetExcelSheet<ContentFinderCondition>().FirstOrDefault(duty => duty.RowId == dutyId);

        if (duty.Equals(default(ContentFinderCondition)) || string.IsNullOrEmpty(duty.Name.ToString()))
            return "Unknown Duty";
        var dutyName = duty.Name.ToString();
        return char.ToUpper(dutyName[0]) + dutyName.Substring(1);
    }

    /// <summary>
    /// Returns the FFLogs serverRegion string for a given world ID.
    /// Uses the world's data-centre name to determine region.
    /// </summary>
    public static string WorldToRegion(ushort worldId)
    {
        try
        {
            var world = Svc.Data.GetExcelSheet<World>().First(w => w.RowId == worldId);
            var dcRegion = world.DataCenter.Value.Region;

            return dcRegion switch
            {
                // Japan
                1 => "JP",
                // North America
                2 => "NA",
                // Europe
                3 => "EU",
                // Oceania
                4 => "OC",
                // Fallback — NA is the safest guess for unknown DCs
                _ => "NA"
            };
        }
        catch
        {
            return "NA";
        }
    }
}
