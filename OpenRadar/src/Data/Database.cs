using System.IO;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Microsoft.Data.Sqlite;

namespace OpenRadar;

public static class Database
{
    // Should probably initialise database on plugin startup tbh, but it works smile

    private static string? GetPath()
    {
        var configDir = Svc.PluginInterface.ConfigDirectory;
        if (configDir == null)
        {
            Svc.Log.Error("Failed to find Config Directory.");
            return null;
        }

        var path = Path.Combine(configDir.FullName, "Data.db");

        if (!File.Exists(path))
        {
            CreateDatabase(path);
        }
        return path;
    }

    private static void CreateDatabase(string path)
    {
        using var connection = new SqliteConnection($"Data Source={path}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
        @"
            CREATE TABLE IF NOT EXISTS Players (
                ContentId INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                WorldId INTEGER NOT NULL
            );
        ";

        command.ExecuteNonQuery();
    }

    public static void AddPlayer(PlayerInfo playerInfo)
    {
        var path = GetPath();
        if (path.IsNullOrEmpty())
        {
            return;
        }
        using var connection = new SqliteConnection($"Data Source={path}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
        @"
            INSERT INTO Players (ContentId, Name, WorldId)
            VALUES ($contentId, $name, $worldId)
            ON CONFLICT(ContentId) DO UPDATE SET
                Name = excluded.Name,
                WorldId = excluded.WorldId;
        ";

        command.Parameters.AddWithValue("$contentId", (long)playerInfo.content_id);
        command.Parameters.AddWithValue("$name", playerInfo.name);
        command.Parameters.AddWithValue("$worldId", playerInfo.world);

        command.ExecuteNonQuery();
    }

    public static PlayerInfo? GetPlayerByContentId(ulong contentId)
    {
        var path = GetPath();
        if (path.IsNullOrEmpty())
        {
            return null;
        }

        using var connection = new SqliteConnection($"Data Source={path}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
        @"
            SELECT ContentId, Name, WorldId
            FROM Players
            WHERE ContentId = $contentId;
        ";

        command.Parameters.AddWithValue("$contentId", (long)contentId);

        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            return new PlayerInfo(contentId, reader.GetString(1), (ushort)reader.GetInt32(2));
        }
        return null;
    }
}