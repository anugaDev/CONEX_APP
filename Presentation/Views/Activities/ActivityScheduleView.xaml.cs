using System.Windows.Controls;
using System.Windows.Input;
using CONEX_APP.Presentation.ViewModels.Activities;

namespace CONEX_APP.Presentation.Views.Activities;

public partial class ActivityScheduleView : UserControl
{
    public ActivityScheduleView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Permite deseleccionar una clase haciendo clic sobre ella cuando ya está seleccionada.
    /// El doble clic se gestiona aquí también para que siga abriendo la edición
    /// (el primer clic del doble clic habrá deseleccionado la fila).
    /// </summary>
    private void ActivityRow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGridRow row || DataContext is not ActivityScheduleViewModel viewModel)
            return;

        if (ItemsControl.ItemsControlFromItemContainer(row) is not DataGrid grid)
            return;

        if (e.ClickCount == 2)
        {
            grid.SelectedItem = row.Item;
            if (viewModel.EditActivityCommand.CanExecute(null))
                viewModel.EditActivityCommand.Execute(null);
            e.Handled = true;
            return;
        }

        if (Keyboard.Modifiers != ModifierKeys.None)
            return;

        if (row.IsSelected)
        {
            grid.UnselectAll();
            e.Handled = true;
        }
    }
}
