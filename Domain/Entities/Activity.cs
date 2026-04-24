namespace CONEX_APP.Domain.Entities;

public class Activity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Tutor { get; set; } = string.Empty;
    
    public string Classroom { get; set; } = string.Empty;
    
    public DateTime Date {get; set; }
    
    public DateTime CreatedAt { get; set; }
}