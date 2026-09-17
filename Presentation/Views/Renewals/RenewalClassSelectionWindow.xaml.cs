using System.Windows;
using CONEX_APP.Application.DTOs;
using CONEX_APP.Presentation.ViewModels.Renewals;

namespace CONEX_APP.Presentation.Views.Renewals;

public partial class RenewalClassSelectionWindow : Window
{
    public RenewalClassSelectionWindow(IEnumerable<ActivityScheduleDto> enrolledActivities)
    {
        InitializeComponent();
        RenewalClassSelectionViewModel vm = new RenewalClassSelectionViewModel(
            enrolledActivities,
            closeAction: () => this.Close());
        DataContext = vm;
    }

    /// <summary>El ViewModel tipado para leer el resultado después de ShowDialog().</summary>
    public RenewalClassSelectionViewModel ViewModel => (RenewalClassSelectionViewModel)DataContext;
}
