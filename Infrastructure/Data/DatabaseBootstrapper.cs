using System.IO;

namespace CONEX_APP.Infrastructure.Data;

public sealed record DatabaseStartupResult(string DatabasePath, string? MigratedFrom);

public static class DatabaseBootstrapper
{
    private const int BackupsToKeep = 10;
    private const string ProjectFileName = "CONEX_APP.csproj";
    private const int MaxParentLevelsToSearch = 6;
    private static readonly string[] SqliteSidecarSuffixes = { "-wal", "-shm" };

    public static DatabaseStartupResult Initialize()
    {
        Directory.CreateDirectory(DatabasePaths.DataDirectory);
        string databasePath = DatabasePaths.CurrentDatabasePath;

        string? migratedFrom = null;
        if (!File.Exists(databasePath))
            migratedFrom = CopyLegacyDatabase(databasePath);

        if (File.Exists(databasePath))
            CreateDailyBackup(databasePath);

        DatabaseInitializer.EnsureSchema(databasePath);

        return new DatabaseStartupResult(databasePath, migratedFrom);
    }

    private static string? CopyLegacyDatabase(string targetPath)
    {
        string fileName = Path.GetFileName(targetPath);

        string? newest = GetLegacyCandidates(fileName)
            .Where(File.Exists)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();

        if (newest == null)
            return null;

        File.Copy(newest, targetPath);
        foreach (string suffix in SqliteSidecarSuffixes)
        {
            if (File.Exists(newest + suffix))
                File.Copy(newest + suffix, targetPath + suffix);
        }

        return newest;
    }

    private static IEnumerable<string> GetLegacyCandidates(string fileName)
    {
        // Antes la BD se abría con una ruta relativa, es decir, junto al .exe (bin\Debug\net8.0-windows)
        yield return Path.Combine(AppContext.BaseDirectory, fileName);

        // ...y rebuild_db.ps1 también escribía una copia en la raíz del proyecto
        string? projectRoot = FindProjectRoot();
        if (projectRoot != null)
            yield return Path.Combine(projectRoot, fileName);
    }

    private static string? FindProjectRoot()
    {
        DirectoryInfo? dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < MaxParentLevelsToSearch && dir != null; i++, dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, ProjectFileName)))
                return dir.FullName;
        }
        return null;
    }

    private static void CreateDailyBackup(string databasePath)
    {
        Directory.CreateDirectory(DatabasePaths.BackupDirectory);

        string baseName = Path.GetFileNameWithoutExtension(databasePath);
        string backupPath = Path.Combine(DatabasePaths.BackupDirectory, $"{baseName}_{DateTime.Now:yyyyMMdd}.db");

        if (!File.Exists(backupPath))
            File.Copy(databasePath, backupPath);

        // El nombre lleva la fecha en formato yyyyMMdd, así que el orden alfabético es el cronológico
        IEnumerable<string> oldBackups = Directory
            .GetFiles(DatabasePaths.BackupDirectory, $"{baseName}_????????.db")
            .OrderByDescending(Path.GetFileName, StringComparer.Ordinal)
            .Skip(BackupsToKeep);

        foreach (string oldBackup in oldBackups)
            File.Delete(oldBackup);
    }
}
