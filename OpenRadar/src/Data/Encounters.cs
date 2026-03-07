using System;
using Lumina.Excel.Sheets;

namespace OpenRadar;

public static class Encounters
{
    private static ushort[][] SavageRowIds =
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

    private static ushort[] Ultimates = [280, 539, 694, 788, 908, 1006];

    public record Info
    (
        string? category,
        string name,
        string expansion,
        string? savageParent = null
    );

    public static Info? DataQuery(ushort dutyId)
    {
        var duty = Svc.Data.GetExcelSheet<ContentFinderCondition>().FirstOrDefault(duty => duty.RowId == dutyId);
        if (duty.RowId == 0) return null;

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
            if (tier == null) return null;
                
            var savageParent = Util.DutyIdToName(tier.Last());
            return new Info(contentCategory, dutyNameClean, dutyExpansion, savageParent);
        }

        return new Info(contentCategory, dutyNameClean, dutyExpansion);
    }

    public static Vector4 ProgToColour(string prog, ushort dutyId)
    {
        // first decypher prog
        Vector4 fail = new Vector4(1f, 1f, 1f, 1f);
        string[] progParts = prog.Split(' ');
        bool ultimateOrDoor = false;
        float? progPercent = null;

        if (progParts.Length > 1) ultimateOrDoor = true;

        var cleaned = progParts[0].Replace("%", "");
        if (float.TryParse(cleaned, out float parsed))
            progPercent = parsed;

        if (progPercent == null) return fail;
            
        float percent = progPercent.Value;

        if (!ultimateOrDoor)
        {
            return percent switch
            {
                > 75f => Col.fGrey,
                > 50f => Col.fGreen,
                > 25f => Col.fBlue,
                > 10f => Col.fPurple,
                > 3f => Col.fOrange,
                _ => Col.fPink
            };
        }

        var part = progParts[1];

        if (!int.TryParse(part.AsSpan(1), out int phase)) return fail;

        if (part[0] == 'I') phase = -phase;

        if (!Ultimates.Contains(dutyId)) // door boss, some savageparents
        {
            return (percent, phase) switch
            {
                (>50f, 1) => Col.fGrey,
                (>20f, 1) => Col.fGreen,
                (_, 1) => Col.fBlue,
                (>50f, 2) => Col.fPurple,
                (>20f, 2) => Col.fOrange,
                (_, _) => Col.fPink
            };
        }
        return dutyId switch
        {
            1006 or 280 => phase switch // fru & ucob
            {
                1 => Col.fGrey,
                2 => Col.fGreen,
                3 => Col.fBlue,
                4 => Col.fPurple,
                _ when percent > 10f => Col.fOrange,
                _ => Col.fPink
            },
            908 => phase switch // top
            {
                1 => Col.fGrey,
                2 => Col.fGreen,
                3 => Col.fBlue,
                4 => Col.fPurple,
                5 => Col.fOrange,
                _ => Col.fPink
            },
            788 => phase switch // dsr
            {
                <=2 => Col.fGrey,
                3 => Col.fGreen,
                4 or -1 => Col.fBlue,
                5 => Col.fPurple,
                6 => Col.fOrange,
                _ => Col.fPink
            },
            694 => phase switch // tea
            {
                1 or -1 => Col.fGrey,
                2 => Col.fGreen,
                -2 => Col.fBlue,
                3 => Col.fPurple,
                _ when percent > 10f => Col.fOrange,
                _ => Col.fPink
            },
            539 => phase switch // uwu
            {
                1 => Col.fGrey,
                2 => Col.fGreen,
                3 => Col.fBlue,
                _ when percent > 50f => Col.fPurple,
                _ when percent > 10f => Col.fOrange,
                _ => Col.fPink
            },
            _ => fail
        };
    }
}