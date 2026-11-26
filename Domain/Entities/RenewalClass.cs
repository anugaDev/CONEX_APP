namespace CONEX_APP.Domain.Entities;

public class RenewalClass
{
    public int Id { get; set; }

    public int RenewalId { get; set; }

    public string ActivityName { get; set; } = string.Empty;

    public string Tutor { get; set; } = string.Empty;

    public string Schedule { get; set; } = string.Empty;

    public decimal Price { get; set; }
}
