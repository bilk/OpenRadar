using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace OpenRadar;

public static partial class Database
{
    private static SQLiteConnection? _ptConnection;
    private static SQLiteConnection? PTConnection => _ptConnection ??= ConnectPT();

    private static SQLiteConnection? ConnectPT()
    {
        var parent = Svc.PluginInterface.ConfigDirectory.Parent;
        var path = parent == null ? null : Path.Combine(parent.FullName, "PlayerTrack", "data.db");

        if (!File.Exists(path)) return null; // .Exists(null) returns false, so dont need to nullcheck path
        // lazy initialisation causes ConnectPT to be called every LocalDB check fail, but is cheap enough to allow for this
        // especially as the user may install PlayerTrack while the plugin is mid operational, so its unnecessary to check anything            

        var conn = new SQLiteConnection($"Data Source={path};Mode=ReadOnly;Cache=Shared"); // apparently cannot write if second connection writing, so just readonly
        conn.Open();
        return conn;
    }

    public static async Task<PlayerInfo?> GetPlayerPTAsync(ulong contentId)
    {
        if (PTConnection is not { } con) return null;
            
        using var command = con.CreateCommand();
        command.CommandText = """
            SELECT content_id, name, world_id
            FROM players
            WHERE content_id = $id
            LIMIT 1
            """;

        command.Parameters.AddWithValue("$id", (long)contentId);

        using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync()
            ? new PlayerInfo(contentId, reader.GetString(1), (ushort)reader.GetInt32(2))
            : null;
    }
}