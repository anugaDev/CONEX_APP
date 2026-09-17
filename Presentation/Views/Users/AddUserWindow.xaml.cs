using System.Windows;
using System.Windows.Controls;
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

    private void SelectedClassesListBox_LostFocus(object sender, RoutedEventArgs e)
    {
        // Si el foco se mueve al botón de quitar, no limpiamos la selección
        // para que el Command pueda ejecutarse con el ítem correcto.
        if (RemoveClassButton.IsKeyboardFocusWithin || RemoveClassButton.IsMouseOver)
            return;

        SelectedClassesListBox.SelectedItem = null;
    }
}
