using System.IO;
using Microsoft.Data.Sqlite;

namespace CONEX_APP.Infrastructure.Data;

public enum DatabaseMode
{
    Test,
    Production
}

public static class DatabasePaths
{
    public static DatabaseMode Mode { get; set; } = DatabaseMode.Production;

    public const string DataDirectoryEnvironmentVariable = "CONEX_DATA_DIR";

    public static string DataDirectory =>
        Environment.GetEnvironmentVariable(DataDirectoryEnvironmentVariable) is { Length: > 0 } customDirectory
            ? customDirectory
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CONEX");

    public static string BackupDirectory => Path.Combine(DataDirectory, "backups");

    public static string FileName => GetFileName(Mode);

    public static string CurrentDatabasePath => Path.Combine(DataDirectory, FileName);

    public static string GetFileName(DatabaseMode mode) => mode switch
    {
        DatabaseMode.Test => "conex_app_test.db",
        _ => "conex_app.db"
    };

    public static string BuildConnectionString(string databasePath) =>
        new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            ForeignKeys = true
        }.ToString();
}
