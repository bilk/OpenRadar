using System.Globalization;
using System.IO;
using Lumina.Excel.Sheets;
using Serilog.Filters;

namespace OpenRadar;

public static class Encounters
{
    private static readonly ushort[][] SavageRowIds =
    {
        [103, 104, 105, 106],       // aar

        [116, 117, 118, 119],       // heavensward t1
        [147, 148, 149, 150],       // heavensward  t2
        [190, 191, 192, 193],       // heavensward   t3

        [256, 257, 258, 259],       // stormblood t1
        [292, 293, 294, 295],       // stormblood  t2
        [591, 592, 593, 594],       // stormblood   t3

        [654, 683, 685, 690],       // shadowbringers t1
        [716, 720, 727, 729],       // shadowbringers  t2
        [748, 750, 752, 759],       // shadowbringers   t3

        [809, 811, 807, 801],       // endwalker t1
        [873, 881, 877, 884],       // endwalker  t2
        [937, 939, 941, 943],       // endwalker   t3

        [986, 988, 990, 992],       // dawntrail t1
        [1020, 1022, 1024, 1026],   // dawntrail  t2
        [1069, 1071, 1073, 1075]    // dawntrail   t3
    };

    public record Info
    (
        string? category,
        string name,
        string expansion,
        //string? savageChild = null,
        string? savageParent = null
    );

    public static Info? DataQuery(ushort dutyId)
    {
        var duty = Svc.Data.GetExcelSheet<ContentFinderCondition>().FirstOrDefault(duty => duty.RowId == dutyId);
        if (duty.RowId == 0)
        {
            return null;
        }

        string? contentCategory = duty.ContentUICategory.Value.Name.ToString() switch
        {
            var category when category.StartsWith("Savage") => "savage",
            var category when category.StartsWith("High-end Trials") => "trials",
            _ when duty.ContentType.RowId == 28 => "ultimate",
            _ => null
        };

        var dutyName = duty.Name.ToString();
        var dutyNameClean = char.ToUpper(dutyName[0]) + dutyName.Substring(1);
        var dutyExpansion = duty.RequiredExVersion.Value.Name.ToString();
        
        if (contentCategory == "savage")
        {
            var tier = SavageRowIds.FirstOrDefault(row => row.Contains(dutyId));
            if (tier == null)
                return null;

            var savageParent = Util.DutyIdToName(tier.Last());
            return new Info(contentCategory, dutyNameClean, dutyExpansion, savageParent);
        }

        return new Info(contentCategory, dutyNameClean, dutyExpansion);
    }

    public static Vector4 ProgToColour(string prog, string dutyName)
    {
        // first decypher prog
        Vector4 fail = new Vector4(1f, 1f, 1f, 1f);
        string[] progParts = prog.Split(' ');
        bool ultimate = false;
        float? progPercent = null;
        if (progParts.Length > 1)
        {
            ultimate = true;
        }
        var cleaned = progParts[0].Replace("%", "");
        if (float.TryParse(cleaned, out float parsed))
        {
            progPercent = parsed;
        }
        if (progPercent == null || ultimate)
        {
            return fail;
        }
        float p = progPercent.Value;

        return p switch
        {
            >= 75f => new Vector4(0.333f, 0.333f, 0.333f, 1f),    // Grey
            >= 50f => new Vector4(0.118f, 1f, 0f, 1f),          // Green
            >= 25f => new Vector4(0f, 0.439f, 1f, 1f),        // Blue
            >= 10f => new Vector4(0.639f, 0.208f, 0.933f, 1f),        // Purple
            >= 3f => new Vector4(1f, 0.502f, 0f, 1f),         // Orange
            _      => new Vector4(0.887f, 0.408f, 0.659f, 1f)         // Pink
        };
    }
}