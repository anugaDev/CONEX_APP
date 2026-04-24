namespace CONEX_APP.Presentation.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase _currentViewModel = null!;
    
    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public MainViewModel()
    {
    }
}