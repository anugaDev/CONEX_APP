using System.Collections.ObjectModel;
using System.Windows.Input;
using CONEX_APP.Application.DTOs;
using CONEX_APP.Presentation.Commands;
using CONEX_APP.Presentation.ViewModels;

namespace CONEX_APP.Presentation.ViewModels.Renewals;

/// <summary>Un ítem de clase con su estado de selección para el diálogo de renovación.</summary>
public class SelectableActivityItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tutor { get; set; } = string.Empty;
    public bool IsSelected { get; set; } = true;
}

public class RenewalClassSelectionViewModel : ViewModelBase
{
    public ObservableCollection<SelectableActivityItem> Activities { get; } = new();

    public bool Confirmed { get; private set; }

    /// <summary>Las clases marcadas tras confirmar.</summary>
    public IReadOnlyList<ActivityScheduleDto> SelectedActivities { get; private set; } = [];

    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand { get; }

    public RenewalClassSelectionViewModel(
        IEnumerable<ActivityScheduleDto> enrolledActivities,
        Action closeAction)
    {
        foreach (ActivityScheduleDto a in enrolledActivities)
            Activities.Add(new SelectableActivityItem { Id = a.Id, Name = a.Name, Tutor = a.Tutor, IsSelected = true });

        ConfirmCommand = new RelayCommand(_ =>
        {
            SelectedActivities = enrolledActivities
                .Where(a => Activities.First(i => i.Id == a.Id).IsSelected)
                .ToList();
            Confirmed = true;
            closeAction();
        });

        CancelCommand = new RelayCommand(_ => closeAction());
    }
}
