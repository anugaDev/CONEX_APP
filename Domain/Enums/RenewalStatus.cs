namespace CONEX_APP.Domain.Enums;

public enum RenewalStatus
{
    /// <summary>La renovación está al día.</summary>
    Active,

    /// <summary>La renovación vence en los próximos 30 días.</summary>
    ExpiringSoon,

    /// <summary>La renovación ya ha vencido.</summary>
    Expired
}
