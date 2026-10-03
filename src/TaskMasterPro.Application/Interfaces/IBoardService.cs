using TaskMasterPro.Application.DTOs;

namespace TaskMasterPro.Application.Interfaces;

public interface IBoardService
{
    Task<IEnumerable<BoardSummaryDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<BoardDetailDto> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<BoardDto> CreateAsync(CreateBoardRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<BoardDto> UpdateAsync(Guid id, UpdateBoardRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
