using System.Windows.Controls;
using System.Windows.Input;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.Presentation.ViewModels.Users;

namespace CONEX_APP.Presentation.Views.Users;

public partial class UserListView : UserControl
{
    public UserListView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// SelectedItems no es enlazable en WPF, así que sincronizamos manualmente
    /// la selección múltiple (Shift/Ctrl + clic) con el ViewModel.
    /// </summary>
    private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not UserListViewModel viewModel || sender is not DataGrid grid)
            return;

        viewModel.SelectedUsers = grid.SelectedItems.OfType<UserDto>().ToList();
    }

    /// <summary>
    /// Permite deseleccionar un usuario haciendo clic sobre él cuando ya es el único seleccionado.
    /// El doble clic se gestiona aquí también para que siga abriendo la edición
    /// (el primer clic del doble clic habrá deseleccionado la fila).
    /// </summary>
    private void UserRow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGridRow row || DataContext is not UserListViewModel viewModel)
            return;

        var grid = ItemsControl.ItemsControlFromItemContainer(row) as DataGrid;
        if (grid == null)
            return;

        if (e.ClickCount == 2)
        {
            grid.SelectedItem = row.Item;
            if (viewModel.EditUserCommand.CanExecute(null))
                viewModel.EditUserCommand.Execute(null);
            e.Handled = true;
            return;
        }

        // Con Ctrl/Shift se mantiene el comportamiento estándar de selección múltiple
        if (Keyboard.Modifiers != ModifierKeys.None)
            return;

        if (row.IsSelected && grid.SelectedItems.Count == 1)
        {
            grid.UnselectAll();
            e.Handled = true;
        }
    }
}
