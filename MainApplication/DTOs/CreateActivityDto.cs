namespace CONEX_APP.MainApplication.DTOs;

public class CreateActivityDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Tutor { get; set; } = string.Empty;
    
    public string Classroom { get; set; } = string.Empty;
    
    public DateTime Date {get; set; }
    
    public DateTime CreatedAt { get; set; }
}
