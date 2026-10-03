using TaskMasterPro.Application.DTOs;

namespace TaskMasterPro.Application.Interfaces;

public interface IColumnService
{
    Task<ColumnDto> CreateAsync(Guid boardId, CreateColumnRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<ColumnDto> UpdateAsync(Guid id, UpdateColumnRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ColumnDto>> MoveAsync(Guid id, int newIndex, Guid userId, CancellationToken cancellationToken = default);
}
