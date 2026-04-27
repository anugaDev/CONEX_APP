namespace CONEX_APP.Application.DTOs;

public class ActivityScheduleDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Tutor { get; set; } = string.Empty;
    
    public string Classroom { get; set; } = string.Empty;
    
    public DateTime Date {get; set; }
    
    public int MaxStudents { get; set; }
    
    public int EnrolledStudentsCount { get; set; }
    
    public string Occupancy => $"{EnrolledStudentsCount} / {MaxStudents}";
    
    public DateTime CreatedAt { get; set; }

}