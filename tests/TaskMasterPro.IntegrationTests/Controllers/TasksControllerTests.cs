using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TaskMasterPro.Application.DTOs;
using TaskMasterPro.Domain.Entities;
using TaskMasterPro.Domain.Enums;
using TaskMasterPro.Infrastructure.Persistence;
using TaskMasterPro.IntegrationTests.Helpers;
using Xunit;

namespace TaskMasterPro.IntegrationTests.Controllers;

public class TasksControllerTests : IClassFixture<TaskMasterProWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly TaskMasterProWebAppFactory _factory;

    public TasksControllerTests(TaskMasterProWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void AuthenticateClient(Guid userId)
    {
        var token = JwtTokenHelper.GenerateTestToken(userId);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task SeedDataAsync(params object[] entities)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TaskMasterProDbContext>();
        
        await db.Database.EnsureCreatedAsync();
        
        foreach (var entity in entities)
        {
            await db.AddAsync(entity);
        }
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Delete_ValidTask_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "Test", Email = "task1@test.com", PasswordHash = "hash" };
        var board = new Board { Id = Guid.NewGuid(), Title = "B", UserId = userId };
        var column = new BoardColumn { Id = Guid.NewGuid(), Title = "C", BoardId = board.Id, Board = board };
        var task = new TaskItem { Id = Guid.NewGuid(), Title = "T", BoardColumnId = column.Id, BoardColumn = column, OrderIndex = 0 };
        
        await SeedDataAsync(user, board, column, task);
        AuthenticateClient(userId);

        // Act
        var response = await _client.DeleteAsync($"/api/tasks/{task.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_TaskNotFound_Returns404()
    {
        // Arrange
        var userId = Guid.NewGuid();
        AuthenticateClient(userId);

        // Act
        var response = await _client.DeleteAsync($"/api/tasks/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WrongUser_Returns404()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var owner = new User { Id = ownerId, Name = "Owner", Email = "task2@test.com", PasswordHash = "hash" };
        var attacker = new User { Id = attackerId, Name = "Attacker", Email = "task3@test.com", PasswordHash = "hash" };
        var board = new Board { Id = Guid.NewGuid(), Title = "B", UserId = ownerId };
        var column = new BoardColumn { Id = Guid.NewGuid(), Title = "C", BoardId = board.Id, Board = board };
        var task = new TaskItem { Id = Guid.NewGuid(), Title = "T", BoardColumnId = column.Id, BoardColumn = column };
        
        await SeedDataAsync(owner, attacker, board, column, task);
        AuthenticateClient(attackerId);

        // Act
        var response = await _client.DeleteAsync($"/api/tasks/{task.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
