using System;
using System.Windows.Input;
using CONEX_APP.Presentation.Commands;

namespace CONEX_APP.Presentation.ViewModels.Classes;

public class ClassListViewModel : ViewModelBase
{
    public ICommand GoBackCommand { get; }

    public ClassListViewModel(Action goBack)
    {
        GoBackCommand = new RelayCommand(_ => goBack());
    }
}
