using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using System.Linq.Expressions;
using TaskMasterPro.Application.DTOs;
using TaskMasterPro.Application.Exceptions;
using TaskMasterPro.Application.Services;
using TaskMasterPro.Domain.Entities;
using TaskMasterPro.Domain.Enums;
using TaskMasterPro.Domain.Interfaces;
using Xunit;

namespace TaskMasterPro.UnitTests.Services;

public class TaskServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateTaskRequest>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateTaskRequest>> _updateValidatorMock;
    private readonly Mock<IValidator<MoveTaskRequest>> _moveValidatorMock;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _createValidatorMock = new Mock<IValidator<CreateTaskRequest>>();
        _updateValidatorMock = new Mock<IValidator<UpdateTaskRequest>>();
        _moveValidatorMock = new Mock<IValidator<MoveTaskRequest>>();

        _taskService = new TaskService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            _moveValidatorMock.Object);
    }

    [Fact]
    public async Task MoveAsync_SameColumn_ReordersCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var request = new MoveTaskRequest(columnId, 1);

        _moveValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var board = new Board { Id = boardId, UserId = userId };
        var column = new BoardColumn { Id = columnId, BoardId = boardId, Board = board };
        var taskToMove = new TaskItem { Id = taskId, BoardColumnId = columnId, BoardColumn = column, OrderIndex = 0 };
        var task2 = new TaskItem { Id = Guid.NewGuid(), BoardColumnId = columnId, BoardColumn = column, OrderIndex = 1 };

        _unitOfWorkMock.Setup(u => u.TaskItems.FirstOrDefaultAsync(It.IsAny<Expression<Func<TaskItem, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<TaskItem, object>>[]>()))
            .ReturnsAsync(taskToMove);

        _unitOfWorkMock.Setup(u => u.BoardColumns.FirstOrDefaultAsync(It.IsAny<Expression<Func<BoardColumn, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<BoardColumn, object>>[]>()))
            .ReturnsAsync(column);

        var tasks = new List<TaskItem> { taskToMove, task2 };
        _unitOfWorkMock.Setup(u => u.TaskItems.FindAsync(It.IsAny<Expression<Func<TaskItem, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<TaskItem, object>>[]>()))
            .ReturnsAsync(tasks);

        var expectedResponse = new TaskMoveResponseDto(new List<TaskDto>(), new List<TaskDto>());
        _mapperMock.Setup(m => m.Map<List<TaskDto>>(It.IsAny<List<TaskItem>>()))
            .Returns(new List<TaskDto>());

        // Act
        var result = await _taskService.MoveAsync(taskId, request, userId);

        // Assert
        result.Should().NotBeNull();
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MoveAsync_TaskNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new MoveTaskRequest(Guid.NewGuid(), 1);

        _moveValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _unitOfWorkMock.Setup(u => u.TaskItems.FirstOrDefaultAsync(It.IsAny<Expression<Func<TaskItem, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<TaskItem, object>>[]>()))
            .ReturnsAsync((TaskItem?)null);

        // Act
        Func<Task> act = async () => await _taskService.MoveAsync(Guid.NewGuid(), request, userId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task MoveAsync_Unauthorized_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();
        var request = new MoveTaskRequest(Guid.NewGuid(), 1);

        _moveValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var board = new Board { UserId = anotherUserId };
        var column = new BoardColumn { Board = board };
        var task = new TaskItem { BoardColumn = column };

        _unitOfWorkMock.Setup(u => u.TaskItems.FirstOrDefaultAsync(It.IsAny<Expression<Func<TaskItem, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<TaskItem, object>>[]>()))
            .ReturnsAsync(task);

        // Act
        Func<Task> act = async () => await _taskService.MoveAsync(Guid.NewGuid(), request, userId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
