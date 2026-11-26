using System.Windows;
using CONEX_APP.Infrastructure.Data;
using CONEX_APP.Presentation.Views;

namespace CONEX_APP;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Production = BD con los datos reales. Test = BD vacía para desarrollo.
        DatabasePaths.Mode = DatabaseMode.Production;

        DatabaseStartupResult database;
        try
        {
            database = DatabaseBootstrapper.Initialize();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No se ha podido preparar la base de datos:\n\n{ex.Message}\n\nRuta: {DatabasePaths.CurrentDatabasePath}",
                "Error de base de datos",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        if (database.MigratedFrom != null)
        {
            MessageBox.Show(
                "La base de datos se ha trasladado a su nueva ubicación:\n\n" +
                $"{database.DatabasePath}\n\n" +
                "Se ha copiado desde:\n\n" +
                $"{database.MigratedFrom}\n\n" +
                "El fichero antiguo se conserva, pero la aplicación ya no lo usa.",
                "Base de datos trasladada",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        MainWindow window = new MainWindow();
        MainWindow = window;
        window.Show();
    }
}
