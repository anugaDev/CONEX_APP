namespace CONEX_APP.Domain.Entities;

public class AppSettings
{
    public int Id { get; set; }

    /// <summary>Número de meses entre cada renovación.</summary>
    public int RenewalPeriodMonths { get; set; } = 12;

    /// <summary>Coste de la cuota de renovación (en euros).</summary>
    public decimal RenewalCost { get; set; }

    /// <summary>Coste por clase (en euros).</summary>
    public decimal ClassCost { get; set; }
}
