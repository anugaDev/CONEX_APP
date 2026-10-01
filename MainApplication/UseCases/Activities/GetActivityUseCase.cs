using CONEX_APP.Application.DTOs;
using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;

namespace CONEX_APP.MainApplication.UseCases.Users;

public class GetActivityUseCase
{
    private readonly IActivityRepository _activityRepository;
    private readonly IWaitlistRepository _waitlistRepository;

    public GetActivityUseCase(IActivityRepository activityRepository, IWaitlistRepository waitlistRepository)
    {
        _activityRepository = activityRepository;
        _waitlistRepository = waitlistRepository;
    }

    public async Task<IEnumerable<ActivityScheduleDto>> ExecuteAsync()
    {
        IEnumerable<Activity> activities = await _activityRepository.GetAllAsync();

        List<ActivityScheduleDto> result = new();

        foreach (Activity activity in activities)
        {
            IEnumerable<Waitlist> waitlist = await _waitlistRepository.GetByActivityAsync(activity.Id);

            result.Add(new ActivityScheduleDto
            {
                Id = activity.Id,
                Name = activity.Name,
                Tutor = activity.Tutor,
                Date = activity.Date,
                Classroom = activity.Classroom,
                MaxStudents = activity.MaxStudents,
                EnrolledStudentsCount = activity.Students.Count,
                WaitlistCount = waitlist.Count(),
                EnrolledStudents = activity.Students.Select(s => new EnrolledStudentDto
                {
                    Id = s.Id,
                    FullName = string.Join(" ", new[] { s.Name, s.Surname, s.SecondSurname }.Where(p => !string.IsNullOrWhiteSpace(p)))
                }).ToList(),
                EnrolledStudentNames = activity.Students.Select(s => $"{s.Name} {s.Surname}").ToList()
            });
        }

        return result;
    }
}
