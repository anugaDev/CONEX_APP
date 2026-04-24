using System.Windows;
using CONEX_APP.Presentation.ViewModels.Users;

namespace CONEX_APP.Presentation.Views.Users;

public partial class AddUserWindow : Window
{
    public AddUserWindow(AddUserViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        viewModel.CloseAction = () => Close();
    }
}
