namespace CONEX_APP.Domain.Entities;

public class Waitlist
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ActivityId { get; set; }
    public Activity Activity { get; set; } = null!;

    public DateTime JoinedAt { get; set; }
}
