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
using TaskMasterPro.Domain.Interfaces;
using Xunit;

namespace TaskMasterPro.UnitTests.Services;

public class BoardServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateBoardRequest>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateBoardRequest>> _updateValidatorMock;
    private readonly BoardService _boardService;

    public BoardServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _createValidatorMock = new Mock<IValidator<CreateBoardRequest>>();
        _updateValidatorMock = new Mock<IValidator<UpdateBoardRequest>>();

        _boardService = new BoardService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesBoardAndReturnsDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateBoardRequest("Test Board", "Description");
        
        _createValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
            
        var expectedBoard = new Board { Id = Guid.NewGuid(), Title = "Test Board", UserId = userId };
        var expectedDto = new BoardDto(expectedBoard.Id, "Test Board", "Description", userId, DateTime.UtcNow, DateTime.UtcNow);

        _unitOfWorkMock.Setup(u => u.Boards.AddAsync(It.IsAny<Board>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<BoardDto>(It.IsAny<Board>()))
            .Returns(expectedDto);

        // Act
        var result = await _boardService.CreateAsync(request, userId);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Board");
        _unitOfWorkMock.Verify(u => u.Boards.AddAsync(It.IsAny<Board>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_BoardNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        _unitOfWorkMock.Setup(u => u.Boards.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Board, bool>>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Board?)null);

        // Act
        Func<Task> act = async () => await _boardService.DeleteAsync(boardId, userId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*Board*");
    }

    [Fact]
    public async Task DeleteAsync_BoardExists_DeletesBoard()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var board = new Board { Id = boardId, UserId = userId };
        
        _unitOfWorkMock.Setup(u => u.Boards.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Board, bool>>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(board);

        // Act
        await _boardService.DeleteAsync(boardId, userId);

        // Assert
        _unitOfWorkMock.Verify(u => u.Boards.Remove(board), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
