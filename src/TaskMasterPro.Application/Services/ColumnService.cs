using AutoMapper;
using FluentValidation;
using TaskMasterPro.Application.DTOs;
using TaskMasterPro.Application.Exceptions;
using TaskMasterPro.Application.Interfaces;
using TaskMasterPro.Domain.Entities;
using TaskMasterPro.Domain.Interfaces;

namespace TaskMasterPro.Application.Services;

public class ColumnService : IColumnService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateColumnRequest> _createValidator;
    private readonly IValidator<UpdateColumnRequest> _updateValidator;

    public ColumnService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateColumnRequest> createValidator,
        IValidator<UpdateColumnRequest> updateValidator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ColumnDto> CreateAsync(Guid boardId, CreateColumnRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var valResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid) throw new Exceptions.ValidationException(valResult.Errors);

        var board = await _unitOfWork.Boards.FirstOrDefaultAsync(b => b.Id == boardId && b.UserId == userId, cancellationToken);
        if (board == null) throw new NotFoundException(nameof(Board), boardId);

        var existingColumns = await _unitOfWork.BoardColumns.FindAsync(c => c.BoardId == boardId, cancellationToken);
        int maxOrder = existingColumns.Any() ? existingColumns.Max(c => c.OrderIndex) : -1;

        var column = new BoardColumn
        {
            Title = request.Title,
            BoardId = boardId,
            OrderIndex = maxOrder + 1
        };

        await _unitOfWork.BoardColumns.AddAsync(column, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ColumnDto>(column);
    }

    public async Task<ColumnDto> UpdateAsync(Guid id, UpdateColumnRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var valResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid) throw new Exceptions.ValidationException(valResult.Errors);

        var column = await _unitOfWork.BoardColumns.FirstOrDefaultAsync(c => c.Id == id, cancellationToken, c => c.Board);
        if (column == null || column.Board.UserId != userId) throw new NotFoundException(nameof(BoardColumn), id);

        column.Title = request.Title;
        _unitOfWork.BoardColumns.Update(column);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ColumnDto>(column);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var column = await _unitOfWork.BoardColumns.FirstOrDefaultAsync(c => c.Id == id, cancellationToken, c => c.Board);
            if (column == null || column.Board.UserId != userId) throw new NotFoundException(nameof(BoardColumn), id);

            var boardId = column.BoardId;
            _unitOfWork.BoardColumns.Remove(column);

            var remainingColumns = (await _unitOfWork.BoardColumns.FindAsync(c => c.BoardId == boardId && c.Id != id, cancellationToken))
                .OrderBy(c => c.OrderIndex).ToList();

            for (int i = 0; i < remainingColumns.Count; i++)
            {
                remainingColumns[i].OrderIndex = i;
                _unitOfWork.BoardColumns.Update(remainingColumns[i]);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IEnumerable<ColumnDto>> MoveAsync(Guid id, int newIndex, Guid userId, CancellationToken cancellationToken = default)
    {
        var column = await _unitOfWork.BoardColumns.FirstOrDefaultAsync(c => c.Id == id, cancellationToken, c => c.Board);
        if (column == null || column.Board.UserId != userId) throw new NotFoundException(nameof(BoardColumn), id);

        var allColumns = (await _unitOfWork.BoardColumns.FindAsync(c => c.BoardId == column.BoardId, cancellationToken)).OrderBy(c => c.OrderIndex).ToList();
        
        allColumns.Remove(column);
        
        if (newIndex < 0) newIndex = 0;
        if (newIndex > allColumns.Count) newIndex = allColumns.Count;
        
        allColumns.Insert(newIndex, column);
        
        for (int i = 0; i < allColumns.Count; i++)
        {
            allColumns[i].OrderIndex = i;
            _unitOfWork.BoardColumns.Update(allColumns[i]);
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<IEnumerable<ColumnDto>>(allColumns);
    }
}
