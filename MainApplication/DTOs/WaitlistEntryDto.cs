namespace CONEX_APP.MainApplication.DTOs;

public class WaitlistEntryDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }

    public int Position { get; set; }

    public string PositionLabel => $"#{Position}";
}
