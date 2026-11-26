namespace CONEX_APP.MainApplication.DTOs;

public class RenewalReceiptDto
{
    public int RenewalId { get; set; }

    public string ReceiptNumber => $"REC-{RenewalId:D6}";

    public DateTime RenewalDate { get; set; }

    public DateTime ValidUntil { get; set; }

    public int PeriodMonths { get; set; }

    public decimal? RenewalAmount { get; set; }

    public List<ReceiptClassLineDto> Classes { get; set; } = new();

    public bool HasAmounts => RenewalAmount.HasValue;

    public decimal? Total => RenewalAmount.HasValue
        ? RenewalAmount.Value + Classes.Sum(c => c.Price)
        : null;
}

public class ReceiptClassLineDto
{
    public string ActivityName { get; set; } = string.Empty;
    public string Tutor { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
