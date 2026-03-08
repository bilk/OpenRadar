using System.Data.SQLite;
using System.IO;
using System;
using System.Threading.Tasks;
using ECommons.GameHelpers;

namespace OpenRadar;

public static partial class Database
{
    private static string Initialise()
    {
        var configDir = Svc.PluginInterface.ConfigDirectory;
        if (configDir == null) throw new Exception("Config Directory not found."); // Shouldn't happen, and if it does cba to work around it so just unload

        var path = Path.Combine(configDir.FullName, "Data.db");
        if (!File.Exists(path)) CreateDatabase(path);

        return path;
    }

    private static void CreateDatabase(string path)
    {
        using var command = Connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Players (
                ContentId INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                WorldId INTEGER NOT NULL,
                LodestoneId INTEGER
            );
        ";
        command.ExecuteNonQuery();

        try // In case a user does not have lodestoneId column (from previous version)
        {
            using var alterCmd = Connection.CreateCommand();
            alterCmd.CommandText = "ALTER TABLE Players ADD COLUMN LodestoneId INTEGER;";
            alterCmd.ExecuteNonQuery();
        }
        catch {}
    }

    public static async Task AddPlayerORAsync(PlayerInfo playerInfo)
    {
        Util.Log($"Adding Player: {playerInfo.name}");
        using var command = Connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Players (ContentId, Name, WorldId)
            VALUES ($contentId, $name, $worldId)
            ON CONFLICT(ContentId) DO UPDATE SET
                Name = excluded.Name,
                WorldId = excluded.WorldId;
        ";

        command.Parameters.AddWithValue("$contentId", (long)playerInfo.contentId);
        command.Parameters.AddWithValue("$name", playerInfo.name);
        command.Parameters.AddWithValue("$worldId", playerInfo.world);

        await command.ExecuteNonQueryAsync();
    }

    public static async Task<PlayerInfo?> GetPlayerORAsync(ulong contentId)
    {
        using var command = Connection.CreateCommand();
        command.CommandText = @"
            SELECT ContentId, Name, WorldId
            FROM Players
            WHERE ContentId = $contentId;
        ";
        command.Parameters.AddWithValue("$contentId", (long)contentId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return new PlayerInfo(contentId, reader.GetString(1), (ushort)reader.GetInt32(2));

        return null;
    }
 
    public static async Task<int?> GetLodestoneORAsync(ulong contentId)
    {
        using var command = Connection.CreateCommand();
        command.CommandText = @"
            SELECT ContentId, LodestoneId
            FROM Players
            WHERE ContentId = $contentId;
        ";
        command.Parameters.AddWithValue("$contentId", (long)contentId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync()) 
        {
            if (reader.IsDBNull(1)) return null;
            return reader.GetInt32(1);
        }

        return null;
    }
}