using System.Windows.Controls;
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
}
