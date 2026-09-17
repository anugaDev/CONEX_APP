namespace CONEX_APP.MainApplication.DTOs;

public class AppSettingsDto
{
    public int RenewalPeriodMonths { get; set; } = 12;
    public decimal RenewalCost { get; set; }
    public decimal ClassCost { get; set; }
}
