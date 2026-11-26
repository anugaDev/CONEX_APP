using System.IO;
using System.Reflection;
using Microsoft.Data.Sqlite;

namespace CONEX_APP.Infrastructure.Data;

public static class DatabaseInitializer
{
    private const string SchemaResourceName = "CONEX_APP.Infrastructure.Data.Schema.schema.sql";

    private static readonly (string Table, string Column, string Definition)[] ColumnUpgrades =
    {
        ("Activities", "MaxStudents", "INTEGER NOT NULL DEFAULT 0"),
        ("Renewals", "NextRenewalDate", "TEXT NULL"),
        ("Renewals", "RenewalAmount", "TEXT NULL"),
    };

    public static void EnsureSchema(string databasePath)
    {
        using SqliteConnection connection = new SqliteConnection(DatabasePaths.BuildConnectionString(databasePath));
        connection.Open();

        using SqliteTransaction transaction = connection.BeginTransaction();

        Execute(connection, transaction, LoadSchemaSql());

        foreach ((string table, string column, string definition) in ColumnUpgrades)
        {
            if (!ColumnExists(connection, transaction, table, column))
                Execute(connection, transaction, $"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {definition};");
        }

        transaction.Commit();
    }

    private static string LoadSchemaSql()
    {
        using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(SchemaResourceName);
        if (stream == null)
            throw new InvalidOperationException($"No se encontró el recurso del esquema '{SchemaResourceName}'.");

        using StreamReader reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static bool ColumnExists(SqliteConnection connection, SqliteTransaction transaction, string table, string column)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT COUNT(*) FROM pragma_table_info($table) WHERE name = $column;";
        command.Parameters.AddWithValue("$table", table);
        command.Parameters.AddWithValue("$column", column);
        return Convert.ToInt64(command.ExecuteScalar()) > 0;
    }

    private static void Execute(SqliteConnection connection, SqliteTransaction transaction, string sql)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
