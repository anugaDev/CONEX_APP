using System.Globalization;

namespace CONEX_APP.MainApplication.DTOs;

public class EnrolledStudentDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
}

public class ActivityScheduleDto
{
    private static readonly CultureInfo SpanishCulture = CultureInfo.GetCultureInfo("es-ES");

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Tutor { get; set; } = string.Empty;

    public string Classroom { get; set; } = string.Empty;

    public DateTime Date {get; set; }

    public int MaxStudents { get; set; }

    public int EnrolledStudentsCount { get; set; }

    public int WaitlistCount { get; set; }

    public bool HasCapacityLimit => MaxStudents > 0;

    public string Occupancy => HasCapacityLimit
        ? $"{EnrolledStudentsCount} / {MaxStudents}"
        : $"{EnrolledStudentsCount} / sin límite";

    public bool IsFull => HasCapacityLimit && EnrolledStudentsCount >= MaxStudents;

    public string Schedule => Date.ToString("dddd HH:mm", SpanishCulture);

    public string DisplayLabel => IsFull
        ? $"{Name} — COMPLETA ({WaitlistCount} en espera)"
        : $"{Name} ({Occupancy})";

    public List<EnrolledStudentDto> EnrolledStudents { get; set; } = new();

    public List<string> EnrolledStudentNames { get; set; } = new();

    public DateTime CreatedAt { get; set; }
}
