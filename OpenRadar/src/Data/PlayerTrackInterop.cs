using System.IO;
using System.Data.SQLite;

namespace OpenRadar;

public static class PlayerTrackInterop
{
    public static bool Installed()
    {
        return Svc.PluginInterface.InstalledPlugins.Any(
            x => x.InternalName == "PlayerTrack");
    }

    private static string? DatabasePath()
    {
        var configDirParent = Svc.PluginInterface.ConfigDirectory.Parent;
        if (configDirParent == null)
            return null;

        return Path.Combine(configDirParent.FullName, "PlayerTrack", "data.db");
    }

    public static PlayerInfo? Extract(ulong contentId)
    {
        if (contentId == 0)
            return null;

        if (!Installed())
        {
            Svc.Log.Warning($"PlayerTrack is not installed.");
            return null;
        }

        var dbPath = DatabasePath();
        if (dbPath == null || !File.Exists(dbPath))
        {
            Svc.Log.Error($"PlayerTrack Database could not be found - {dbPath}");
            return null;
        }


        using var dbConnection = new SQLiteConnection($"Data Source={dbPath}");
        dbConnection.Open();

        using var command = dbConnection.CreateCommand();
        command.CommandText = @"
            SELECT content_id, name, world_id
            FROM players
            WHERE content_id = $id
            LIMIT 1";
        command.Parameters.AddWithValue("$id", contentId);

        using var reader = command.ExecuteReader();

        var playerFound = reader.Read();

        if (!playerFound)
        {
            return null;
        }
        return new PlayerInfo 
            (
                contentId, 
                reader.GetString(reader.GetOrdinal("name")), 
                (ushort)reader.GetInt32(reader.GetOrdinal("world_id"))
            );
    }
}