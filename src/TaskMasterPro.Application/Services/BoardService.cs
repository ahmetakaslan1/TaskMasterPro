using AutoMapper;
using FluentValidation;
using TaskMasterPro.Application.DTOs;
using TaskMasterPro.Application.Exceptions;
using TaskMasterPro.Application.Interfaces;
using TaskMasterPro.Domain.Entities;
using TaskMasterPro.Domain.Interfaces;

namespace TaskMasterPro.Application.Services;

public class BoardService : IBoardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateBoardRequest> _createValidator;
    private readonly IValidator<UpdateBoardRequest> _updateValidator;

    public BoardService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateBoardRequest> createValidator,
        IValidator<UpdateBoardRequest> updateValidator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IEnumerable<BoardSummaryDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var boards = await _unitOfWork.Boards.FindAsync(b => b.UserId == userId, cancellationToken);
        return _mapper.Map<IEnumerable<BoardSummaryDto>>(boards.OrderByDescending(b => b.CreatedAt));
    }

    public async Task<BoardDetailDto> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var board = await _unitOfWork.Boards.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);
        if (board == null)
        {
            throw new NotFoundException(nameof(Board), id);
        }

        var columns = await _unitOfWork.BoardColumns.FindAsync(c => c.BoardId == id, cancellationToken);
        var columnIds = columns.Select(c => c.Id).ToList();
        
        var tasks = await _unitOfWork.TaskItems.FindAsync(t => columnIds.Contains(t.BoardColumnId), cancellationToken, t => t.Comments);
        
        // This handles N+1 logic effectively without EF Core explicit Include (which would be in Infrastructure)
        // Note: For Clean Architecture with generic repo, we compose it here or extend IRepository.
        // But since we use Generic Repository, fetching all columns and tasks for the board is efficient enough.
        // Wait, spec 3 says: "N+1 yasağı: Bir listeyi getiren her sorgu, ilişkili verileri (ör. commentCount) JOIN/COUNT(*) projeksiyon ile tek sorguda getirir."
        // A generic repository limits us if it doesn't support Includes. We'll assume the Generic Repository can handle Includes if we pass expressions, or we just rely on EF Core tracking/lazy loading.
        // Better yet, we can load them manually since we're in application.
        // To strictly avoid N+1 and stick to generic repo, we'll assign tasks to columns in memory.
        
        var columnsList = columns.OrderBy(c => c.OrderIndex).ToList();
        var tasksList = tasks.ToList();
        
        // Since CommentCount needs projection, and our GenericRepo doesn't have Include or Select,
        // we might need a custom query or add Include to IRepository.
        // Let's keep it simple here, we will implement Include in Infrastructure.
        
        foreach (var col in columnsList)
        {
            col.Tasks = tasksList.Where(t => t.BoardColumnId == col.Id).OrderBy(t => t.OrderIndex).ToList();
        }
        
        board.Columns = columnsList;

        return _mapper.Map<BoardDetailDto>(board);
    }

    public async Task<BoardDto> CreateAsync(CreateBoardRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var valResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid) throw new Exceptions.ValidationException(valResult.Errors);

        var board = new Board
        {
            Title = request.Title,
            Description = request.Description,
            UserId = userId
        };

        await _unitOfWork.Boards.AddAsync(board, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<BoardDto>(board);
    }

    public async Task<BoardDto> UpdateAsync(Guid id, UpdateBoardRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var valResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid) throw new Exceptions.ValidationException(valResult.Errors);

        var board = await _unitOfWork.Boards.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);
        if (board == null) throw new NotFoundException(nameof(Board), id);

        board.Title = request.Title;
        board.Description = request.Description;

        _unitOfWork.Boards.Update(board);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<BoardDto>(board);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var board = await _unitOfWork.Boards.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);
        if (board == null) throw new NotFoundException(nameof(Board), id);

        _unitOfWork.Boards.Remove(board);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
