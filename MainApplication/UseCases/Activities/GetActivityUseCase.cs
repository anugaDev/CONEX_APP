using CONEX_APP.Application.DTOs;
using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;

namespace CONEX_APP.MainApplication.UseCases.Users;

public class GetActivityUseCase
{
    private readonly IActivityRepository _activityRepository;

    public GetActivityUseCase(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<IEnumerable<ActivityScheduleDto>> ExecuteAsync()
    {
        IEnumerable<Activity> activities = await _activityRepository.GetAllAsync();
        
        return activities.Select(activity => new ActivityScheduleDto 
        {
            Id = activity.Id,
            Name = activity.Name,
            Tutor = activity.Tutor,
            Date = activity.Date,
            Classroom = activity.Classroom,
            MaxStudents = activity.MaxStudents,
            EnrolledStudentsCount = activity.Students.Count,
            EnrolledStudents = activity.Students.Select(s => new EnrolledStudentDto
            {
                Id = s.Id,
                FullName = string.Join(" ", new[] { s.Name, s.Surname, s.SecondSurname }.Where(p => !string.IsNullOrWhiteSpace(p)))
            }).ToList(),
            EnrolledStudentNames = activity.Students.Select(s => $"{s.Name} {s.Surname}").ToList()
        });
    }
}
