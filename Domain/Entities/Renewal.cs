namespace CONEX_APP.Domain.Entities;

public class Renewal
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public DateTime RenewalDate { get; set; }
}
