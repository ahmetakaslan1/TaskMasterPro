using TaskMasterPro.Application.DTOs;

namespace TaskMasterPro.Application.Interfaces;

public interface ICommentService
{
    Task<IEnumerable<CommentDto>> GetByTaskIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);
    Task<CommentDto> CreateAsync(Guid taskId, CreateCommentRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<CommentDto> UpdateAsync(Guid id, UpdateCommentRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
