using CONEX_APP.Domain.Entities;
using CONEX_APP.MainApplication.DTOs;

namespace CONEX_APP.MainApplication.Mapping;

public static class ActivityMapper
{
    public static ActivityScheduleDto ToDto(Activity activity, int waitlistCount) => new()
    {
        Id = activity.Id,
        Name = activity.Name,
        Tutor = activity.Tutor,
        Date = activity.Date,
        Classroom = activity.Classroom,
        MaxStudents = activity.MaxStudents,
        CreatedAt = activity.CreatedAt,
        EnrolledStudentsCount = activity.Students.Count,
        WaitlistCount = waitlistCount,
        EnrolledStudents = activity.Students
            .Select(s => new EnrolledStudentDto { Id = s.Id, FullName = FullName(s) })
            .OrderBy(s => s.FullName)
            .ToList(),
        EnrolledStudentNames = activity.Students.Select(s => $"{s.Name} {s.Surname}").ToList()
    };

    public static string FullName(User user) =>
        string.Join(" ", new[] { user.Name, user.Surname, user.SecondSurname }.Where(p => !string.IsNullOrWhiteSpace(p)));
}
