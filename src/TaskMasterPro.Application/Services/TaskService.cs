using AutoMapper;
using FluentValidation;
using TaskMasterPro.Application.DTOs;
using TaskMasterPro.Application.Exceptions;
using TaskMasterPro.Application.Interfaces;
using TaskMasterPro.Domain.Entities;
using TaskMasterPro.Domain.Enums;
using TaskMasterPro.Domain.Interfaces;

namespace TaskMasterPro.Application.Services;

public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateTaskRequest> _createValidator;
    private readonly IValidator<UpdateTaskRequest> _updateValidator;
    private readonly IValidator<MoveTaskRequest> _moveValidator;

    public TaskService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateTaskRequest> createValidator,
        IValidator<UpdateTaskRequest> updateValidator,
        IValidator<MoveTaskRequest> moveValidator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _moveValidator = moveValidator;
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var valResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid) throw new Exceptions.ValidationException(valResult.Errors);

        var column = await _unitOfWork.BoardColumns.FirstOrDefaultAsync(c => c.Id == request.ColumnId, cancellationToken, c => c.Board);
        if (column == null || column.Board.UserId != userId) throw new NotFoundException(nameof(BoardColumn), request.ColumnId);

        var existingTasks = await _unitOfWork.TaskItems.FindAsync(t => t.BoardColumnId == request.ColumnId, cancellationToken);
        int maxOrder = existingTasks.Any() ? existingTasks.Max(t => t.OrderIndex) : -1;

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            Priority = Enum.Parse<Priority>(request.Priority ?? "Medium", true),
            BoardColumnId = request.ColumnId,
            OrderIndex = maxOrder + 1,
            Color = request.Color,
            DueDate = request.DueDate
        };

        await _unitOfWork.TaskItems.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Setup empty comments for correct mapping
        task.Comments = new List<Comment>();

        return _mapper.Map<TaskDto>(task);
    }

    public async Task<TaskDetailDto> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = await _unitOfWork.TaskItems.FirstOrDefaultAsync(t => t.Id == id, cancellationToken, t => t.BoardColumn.Board, t => t.Comments);
        if (task == null || task.BoardColumn.Board.UserId != userId) throw new NotFoundException(nameof(TaskItem), id);

        return _mapper.Map<TaskDetailDto>(task);
    }

    public async Task<TaskDto> UpdateAsync(Guid id, UpdateTaskRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var valResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid) throw new Exceptions.ValidationException(valResult.Errors);

        var task = await _unitOfWork.TaskItems.FirstOrDefaultAsync(t => t.Id == id, cancellationToken, t => t.BoardColumn.Board, t => t.Comments);
        if (task == null || task.BoardColumn.Board.UserId != userId) throw new NotFoundException(nameof(TaskItem), id);

        task.Title = request.Title;
        task.Description = request.Description;
        task.Priority = Enum.Parse<Priority>(request.Priority ?? "Medium", true);
        task.Color = request.Color;
        task.DueDate = request.DueDate;

        _unitOfWork.TaskItems.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TaskDto>(task);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var task = await _unitOfWork.TaskItems.FirstOrDefaultAsync(t => t.Id == id, cancellationToken, t => t.BoardColumn.Board);
            if (task == null || task.BoardColumn.Board.UserId != userId) throw new NotFoundException(nameof(TaskItem), id);

            var columnId = task.BoardColumnId;
            _unitOfWork.TaskItems.Remove(task);

            var remainingTasks = (await _unitOfWork.TaskItems.FindAsync(t => t.BoardColumnId == columnId && t.Id != id, cancellationToken))
                .OrderBy(t => t.OrderIndex).ToList();

            for (int i = 0; i < remainingTasks.Count; i++)
            {
                remainingTasks[i].OrderIndex = i;
                _unitOfWork.TaskItems.Update(remainingTasks[i]);
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

    public async Task<TaskMoveResponseDto> MoveAsync(Guid id, MoveTaskRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var valResult = await _moveValidator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid) throw new Exceptions.ValidationException(valResult.Errors);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var taskToMove = await _unitOfWork.TaskItems.FirstOrDefaultAsync(t => t.Id == id, cancellationToken, t => t.BoardColumn.Board);
            if (taskToMove == null || taskToMove.BoardColumn.Board.UserId != userId) 
                throw new NotFoundException(nameof(TaskItem), id);

            var targetColumn = await _unitOfWork.BoardColumns.FirstOrDefaultAsync(c => c.Id == request.TargetColumnId, cancellationToken, c => c.Board);
            if (targetColumn == null || targetColumn.Board.UserId != userId) 
                throw new NotFoundException(nameof(BoardColumn), request.TargetColumnId);
            
            // Check if boards match just in case
            if (taskToMove.BoardColumn.BoardId != targetColumn.BoardId)
                throw new UnauthorizedException("Cannot move task to a different board.");

            var sourceColId = taskToMove.BoardColumnId;
            var targetColId = request.TargetColumnId;
            var isSameColumn = sourceColId == targetColId;

            var sourceTasks = (await _unitOfWork.TaskItems.FindAsync(t => t.BoardColumnId == sourceColId, cancellationToken, t => t.Comments)).OrderBy(t => t.OrderIndex).ToList();
            List<TaskItem> targetTasks;

            if (isSameColumn)
            {
                targetTasks = sourceTasks;
                sourceTasks.Remove(taskToMove);
                
                var newIndex = request.NewIndex;
                if (newIndex < 0) newIndex = 0;
                if (newIndex > sourceTasks.Count) newIndex = sourceTasks.Count;
                
                sourceTasks.Insert(newIndex, taskToMove);
                
                for (int i = 0; i < sourceTasks.Count; i++)
                {
                    sourceTasks[i].OrderIndex = i;
                    _unitOfWork.TaskItems.Update(sourceTasks[i]);
                }
            }
            else
            {
                targetTasks = (await _unitOfWork.TaskItems.FindAsync(t => t.BoardColumnId == targetColId, cancellationToken, t => t.Comments)).OrderBy(t => t.OrderIndex).ToList();
                
                sourceTasks.Remove(taskToMove);
                for (int i = 0; i < sourceTasks.Count; i++)
                {
                    sourceTasks[i].OrderIndex = i;
                    _unitOfWork.TaskItems.Update(sourceTasks[i]);
                }

                taskToMove.BoardColumnId = targetColId;
                
                var newIndex = request.NewIndex;
                if (newIndex < 0) newIndex = 0;
                if (newIndex > targetTasks.Count) newIndex = targetTasks.Count;
                
                targetTasks.Insert(newIndex, taskToMove);
                for (int i = 0; i < targetTasks.Count; i++)
                {
                    targetTasks[i].OrderIndex = i;
                    _unitOfWork.TaskItems.Update(targetTasks[i]);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new TaskMoveResponseDto {
                SourceColumn = _mapper.Map<List<TaskDto>>(sourceTasks),
                TargetColumn = _mapper.Map<List<TaskDto>>(targetTasks)
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
