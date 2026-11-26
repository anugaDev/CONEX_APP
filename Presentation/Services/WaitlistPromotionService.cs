using System.Windows;
using CONEX_APP.Domain.Exceptions;
using CONEX_APP.MainApplication.UseCases.Registrations;
using CONEX_APP.MainApplication.UseCases.Waitlists;

namespace CONEX_APP.Presentation.Services;

public class WaitlistPromotionService
{
    private readonly GetPromotionCandidateUseCase _getPromotionCandidateUseCase;
    private readonly EnrollUserUseCase _enrollUserUseCase;

    public WaitlistPromotionService(GetPromotionCandidateUseCase getPromotionCandidateUseCase, EnrollUserUseCase enrollUserUseCase)
    {
        _getPromotionCandidateUseCase = getPromotionCandidateUseCase;
        _enrollUserUseCase = enrollUserUseCase;
    }

    public async Task<bool> OfferPromotionsAsync(int activityId)
    {
        bool anyEnrolled = false;

        try
        {
            while (await _getPromotionCandidateUseCase.ExecuteAsync(activityId) is { } candidate)
            {
                string remaining = candidate.RemainingInWaitlist switch
                {
                    0 => "Es la única persona en espera.",
                    1 => "Después quedará 1 persona en espera.",
                    _ => $"Después quedarán {candidate.RemainingInWaitlist} personas en espera."
                };

                MessageBoxResult answer = MessageBox.Show(
                    $"Hay una plaza libre en \"{candidate.ActivityName}\".\n\n" +
                    $"¿Inscribir a {candidate.UserFullName} (1º en la lista de espera)?\n\n{remaining}",
                    "Plaza libre",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (answer != MessageBoxResult.Yes)
                    break;

                await _enrollUserUseCase.ExecuteAsync(candidate.UserId, activityId);
                anyEnrolled = true;
            }
        }
        catch (DomainException ex)
        {
            MessageBox.Show(ex.Message, "Lista de espera", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al inscribir desde la lista de espera: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return anyEnrolled;
    }

    public async Task<bool> OfferPromotionsAsync(IEnumerable<int> activityIds)
    {
        bool anyEnrolled = false;
        foreach (int activityId in activityIds.Distinct())
            anyEnrolled |= await OfferPromotionsAsync(activityId);
        return anyEnrolled;
    }
}
