using System.IO;
using System.Windows;
using CONEX_APP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CONEX_APP;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // ============================================================
        // DATABASE MODE SELECTION
        // Change this to switch between databases:
        //   DatabaseMode.Test       → conex_app_test.db  (original, sin importar)
        //   DatabaseMode.Production → conex_app.db       (con usuarios importados)
        // ============================================================
        AppDbContext.Mode = DatabaseMode.Production;

        ImportUsersIfNeeded();
    }

    private void ImportUsersIfNeeded()
    {
        // Only import into the production database
        if (AppDbContext.Mode != DatabaseMode.Production)
            return;

        var sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "tools", "import_data.sql");
        sqlFile = Path.GetFullPath(sqlFile);

        var flagFile = sqlFile + ".imported";

        // Skip if already imported or SQL file doesn't exist
        if (File.Exists(flagFile) || !File.Exists(sqlFile))
            return;

        try
        {
            using var dbContext = new AppDbContext();
            var sql = File.ReadAllText(sqlFile);
            dbContext.Database.ExecuteSqlRaw(sql);

            // Create flag file so we don't re-import
            File.WriteAllText(flagFile, $"Imported on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            MessageBox.Show(
                "Se han importado los usuarios, actividades y sus relaciones correctamente.",
                "Importación completada",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al importar datos:\n{ex.Message}",
                "Error de importación",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}

