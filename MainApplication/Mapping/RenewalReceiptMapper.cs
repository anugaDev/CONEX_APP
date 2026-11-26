using CONEX_APP.Domain.Entities;
using CONEX_APP.MainApplication.DTOs;

namespace CONEX_APP.MainApplication.Mapping;

public static class RenewalReceiptMapper
{
    public static RenewalReceiptDto ToDto(Renewal renewal, AppSettings settings)
    {
        DateTime validUntil = renewal.NextRenewalDate ?? renewal.RenewalDate.AddMonths(settings.RenewalPeriodMonths);

        return new RenewalReceiptDto
        {
            RenewalId = renewal.Id,
            RenewalDate = renewal.RenewalDate,
            ValidUntil = validUntil,
            PeriodMonths = MonthsBetween(renewal.RenewalDate, validUntil),
            RenewalAmount = renewal.RenewalAmount,
            Classes = renewal.Classes
                .OrderBy(c => c.Id)
                .Select(c => new ReceiptClassLineDto
                {
                    ActivityName = c.ActivityName,
                    Tutor = c.Tutor,
                    Schedule = c.Schedule,
                    Price = c.Price
                })
                .ToList()
        };
    }

    private static int MonthsBetween(DateTime from, DateTime to) =>
        Math.Max(0, (to.Year - from.Year) * 12 + to.Month - from.Month);
}
