using System;
using System.Windows.Input;
using CONEX_APP.Presentation.Commands;

namespace CONEX_APP.Presentation.ViewModels;

public class HomeViewModel : ViewModelBase
{
    public ICommand GoToUsersCommand { get; }
    public ICommand GoToClassesCommand { get; }
    public ICommand ExitCommand { get; }

    public HomeViewModel(Action navigateToUsers, Action navigateToClasses, Action exitAction)
    {
        GoToUsersCommand = new RelayCommand(_ => navigateToUsers());
        GoToClassesCommand = new RelayCommand(_ => navigateToClasses());
        ExitCommand = new RelayCommand(_ => exitAction());
    }
}
