using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Exceptions;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.MainApplication.DTOs;

namespace CONEX_APP.MainApplication.UseCases.Activities;

public class UpdateActivityUseCase
{
    private readonly IActivityRepository _activityRepository;

    public UpdateActivityUseCase(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task ExecuteAsync(UpdateActivityDto dto)
    {
        bool exists = await _activityRepository.ExistsAsync(dto.Name, excludeId: dto.Id);
        if (exists)
            throw new DuplicateEntityException(
                $"Ya existe otra actividad con el nombre \"{dto.Name}\".");

        Activity current = await _activityRepository.GetByIdAsync(dto.Id)
            ?? throw new DomainException("La clase ya no existe. Puede que se haya borrado.");

        if (dto.MaxStudents < 0)
            throw new DomainException("El aforo máximo no puede ser negativo.");

        if (dto.MaxStudents > 0 && dto.MaxStudents < current.Students.Count)
            throw new DomainException(
                $"El aforo no puede ser menor que el número de alumnos inscritos ({current.Students.Count}). " +
                "Quita alumnos de la clase antes de reducirlo.");

        Activity activity = new Activity
        {
            Id = dto.Id,
            Name = dto.Name,
            Tutor = dto.Tutor,
            Classroom = dto.Classroom,
            Date = dto.Date,
            MaxStudents = dto.MaxStudents
        };

        await _activityRepository.UpdateAsync(activity);
    }
}
