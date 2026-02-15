using System.IO;
using Microsoft.Data.Sqlite;

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
        var configDir = Svc.PluginInterface.ConfigDirectory.Parent;
        if (configDir == null)
            return null;

        return Path.Combine(configDir.FullName, "PlayerTrack", "data.db");
    }

    public static PlayerInfo? Extract(ulong contentId)
    {
        if (contentId == 0)
            return null;

        if (!Installed())
        {
            Svc.Log.Error($"PlayerTrack is not installed.");
            return null;
        }

        var dbPath = DatabasePath();
        if (dbPath == null || !File.Exists(dbPath))
        {
            Svc.Log.Error($"PlayerTrack Database could not be found - {dbPath}");
            return null;
        }


        using var dbConnection = new SqliteConnection($"Data Source={dbPath}");
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

        var playerInfo = new PlayerInfo
        {
            content_id = contentId,
            name = playerFound ? reader["name"].ToString() ?? "" : "",
            world = playerFound ? reader["world_id"].ToString() ?? "" : ""
        };

        if (!playerFound)
            Svc.Log.Debug("No player was found matching given content_id.");
        return playerInfo;
    }
}