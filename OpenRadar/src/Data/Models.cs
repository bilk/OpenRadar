namespace OpenRadar;

public static partial class Data
{
    public record PlayerInfo
    (
        ulong contentId,
        string? name = null,
        ushort? world = null,
        byte? jobId = null,
        string? progPoint = null,
        FFLogsData? logData = null
    )
    {
        public PlayerInfo merge(PlayerInfo o) => this with
        {
            name = o.name ?? name,
            world = o.world ?? world,
            jobId = o.jobId ?? jobId,
            progPoint = o.progPoint ?? progPoint,
            logData = o.logData ?? logData
        };
    }

    public record FFLogsData
    (
        float? BestParse,
        float? MedianParse,
        int? Kills,
        bool IsHidden = false
    );
}