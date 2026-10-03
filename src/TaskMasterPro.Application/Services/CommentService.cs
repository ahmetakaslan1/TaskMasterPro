using AutoMapper;
using FluentValidation;
using TaskMasterPro.Application.DTOs;
using TaskMasterPro.Application.Exceptions;
using TaskMasterPro.Application.Interfaces;
using TaskMasterPro.Domain.Entities;
using TaskMasterPro.Domain.Interfaces;

namespace TaskMasterPro.Application.Services;

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCommentRequest> _createValidator;
    private readonly IValidator<UpdateCommentRequest> _updateValidator;

    public CommentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateCommentRequest> createValidator,
        IValidator<UpdateCommentRequest> updateValidator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IEnumerable<CommentDto>> GetByTaskIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        // First check if the user has access to this task via Board ownership
        var task = await _unitOfWork.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken, t => t.BoardColumn.Board);
        if (task == null || task.BoardColumn.Board.UserId != userId)
            throw new NotFoundException(nameof(TaskItem), taskId);

        var comments = await _unitOfWork.Comments.FindAsync(c => c.TaskItemId == taskId, cancellationToken, c => c.User);
        return _mapper.Map<IEnumerable<CommentDto>>(comments.OrderBy(c => c.CreatedAt));
    }

    public async Task<CommentDto> CreateAsync(Guid taskId, CreateCommentRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var valResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid) throw new Exceptions.ValidationException(valResult.Errors);

        // Check access to task
        var task = await _unitOfWork.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken, t => t.BoardColumn.Board);
        if (task == null || task.BoardColumn.Board.UserId != userId)
            throw new NotFoundException(nameof(TaskItem), taskId);

        var comment = new Comment
        {
            Content = request.Content,
            UserId = userId,
            TaskItemId = taskId
        };

        await _unitOfWork.Comments.AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Load User for DTO mapping
        var savedComment = await _unitOfWork.Comments.FirstOrDefaultAsync(c => c.Id == comment.Id, cancellationToken, c => c.User);

        return _mapper.Map<CommentDto>(savedComment);
    }

    public async Task<CommentDto> UpdateAsync(Guid id, UpdateCommentRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var valResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!valResult.IsValid) throw new Exceptions.ValidationException(valResult.Errors);

        var comment = await _unitOfWork.Comments.FirstOrDefaultAsync(c => c.Id == id, cancellationToken, c => c.User, c => c.TaskItem.BoardColumn.Board);
        
        // Ownership check: must be the creator of the comment AND have access to the board
        if (comment == null || comment.TaskItem.BoardColumn.Board.UserId != userId)
            throw new NotFoundException(nameof(Comment), id);
            
        if (comment.UserId != userId)
            throw new UnauthorizedException("You can only edit your own comments.");

        comment.Content = request.Content;
        
        _unitOfWork.Comments.Update(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CommentDto>(comment);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var comment = await _unitOfWork.Comments.FirstOrDefaultAsync(c => c.Id == id, cancellationToken, c => c.TaskItem.BoardColumn.Board);
        
        // Only comment author can delete (and must be in an accessible board)
        if (comment == null || comment.TaskItem.BoardColumn.Board.UserId != userId)
            throw new NotFoundException(nameof(Comment), id);
            
        if (comment.UserId != userId)
            throw new UnauthorizedException("You can only delete your own comments.");

        _unitOfWork.Comments.Remove(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
