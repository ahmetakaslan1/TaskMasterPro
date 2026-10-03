using TaskMasterPro.Application.DTOs;
using TaskMasterPro.Domain.Entities;

namespace TaskMasterPro.Application.Interfaces;

public interface ITaskService
{
    Task<TaskDto> CreateAsync(CreateTaskRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<TaskDetailDto> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<TaskDto> UpdateAsync(Guid id, UpdateTaskRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<TaskMoveResponseDto> MoveAsync(Guid id, MoveTaskRequest request, Guid userId, CancellationToken cancellationToken = default);
}
