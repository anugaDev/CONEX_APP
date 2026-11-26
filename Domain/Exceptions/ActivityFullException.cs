namespace CONEX_APP.Domain.Exceptions;

public class ActivityFullException : DomainException
{
    public int ActivityId { get; }

    public string ActivityName { get; }

    public ActivityFullException(int activityId, string activityName)
        : base($"La clase \"{activityName}\" está completa. Puedes apuntar al usuario a la lista de espera.")
    {
        ActivityId = activityId;
        ActivityName = activityName;
    }
}
