namespace CONEX_APP.Domain.Entities;

public class Renewal
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public DateTime RenewalDate { get; set; }

    /// <summary>
    /// Fecha en la que vence esta renovación (próxima renovación).
    /// Se rellena al registrar la renovación; en los datos importados de Conex corresponde a "Venciment".
    /// Si es null, se calcula como RenewalDate + AppSettings.RenewalPeriodMonths.
    /// </summary>
    public DateTime? NextRenewalDate { get; set; }
}
