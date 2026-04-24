using System.Windows;
using CONEX_APP.Presentation.ViewModels.Activities;

namespace CONEX_APP.Presentation.Views.Activities;

public partial class AddActivityWindow : Window
{
    public AddActivityWindow(AddActivityViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        viewModel.CloseAction = () => Close();
    }
}
