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

        AppDbContext.Mode = DatabaseMode.Production;

        ImportUsersIfNeeded();
    }

    private void ImportUsersIfNeeded()
    {
        if (AppDbContext.Mode != DatabaseMode.Production)
            return;

        string sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "tools", "import_data.sql");
        sqlFile = Path.GetFullPath(sqlFile);

        string flagFile = sqlFile + ".imported";

        if (File.Exists(flagFile) || !File.Exists(sqlFile))
            return;

        try
        {
            using AppDbContext dbContext = new AppDbContext();
            string sql = File.ReadAllText(sqlFile);
            dbContext.Database.ExecuteSqlRaw(sql);

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

