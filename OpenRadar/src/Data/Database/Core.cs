using System.Data.SQLite;

namespace OpenRadar;

public static partial class Database
{
    private static string? _dbPath;
    private static string DbPath => _dbPath ??= Initialise();
    private static SQLiteConnection? _connection;
    private static SQLiteConnection Connection => _connection ??= OpenConnection();

    private static SQLiteConnection OpenConnection()
    {
        var c = new SQLiteConnection($"Data Source={DbPath}");
        c.Open();
        return c;
    }

    public static void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
        _connection = null;

        _ptConnection?.Close();
        _ptConnection?.Dispose();
        _ptConnection = null;
    }
}