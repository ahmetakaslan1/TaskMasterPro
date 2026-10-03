using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TaskMasterPro.Application.DTOs;
using TaskMasterPro.Domain.Entities;
using TaskMasterPro.Infrastructure.Persistence;
using TaskMasterPro.IntegrationTests.Helpers;
using Xunit;

namespace TaskMasterPro.IntegrationTests.Controllers;

public class BoardsControllerTests : IClassFixture<TaskMasterProWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly TaskMasterProWebAppFactory _factory;

    public BoardsControllerTests(TaskMasterProWebAppFactory factory)
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
        
        // Ensure db is created
        await db.Database.EnsureCreatedAsync();
        
        foreach (var entity in entities)
        {
            await db.AddAsync(entity);
        }
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetById_ValidBoard_ReturnsOkAndData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "Test", Email = "test@test.com", PasswordHash = "hash" };
        var board = new Board { Id = Guid.NewGuid(), Title = "Test Board", UserId = userId };
        await SeedDataAsync(user, board);
        AuthenticateClient(userId);

        // Act
        var response = await _client.GetAsync($"/api/boards/{board.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_BoardNotFound_Returns404()
    {
        // Arrange
        var userId = Guid.NewGuid();
        AuthenticateClient(userId);

        // Act
        var response = await _client.GetAsync($"/api/boards/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_WrongUser_Returns404()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var owner = new User { Id = ownerId, Name = "Owner", Email = "owner@test.com", PasswordHash = "hash" };
        var attacker = new User { Id = attackerId, Name = "Attacker", Email = "attacker@test.com", PasswordHash = "hash" };
        var board = new Board { Id = Guid.NewGuid(), Title = "Private Board", UserId = ownerId };
        await SeedDataAsync(owner, attacker, board);
        
        AuthenticateClient(attackerId);

        // Act
        var response = await _client.GetAsync($"/api/boards/{board.Id}");

        // Assert
        // Ownership check should throw NotFoundException to prevent data leakage
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ValidData_ReturnsCreated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "Test", Email = "test2@test.com", PasswordHash = "hash" };
        await SeedDataAsync(user);
        
        AuthenticateClient(userId);
        var request = new CreateBoardRequest("New Board", "Desc");

        // Act
        var response = await _client.PostAsJsonAsync("/api/boards", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<TaskMasterPro.API.Models.ApiResponse<BoardDto>>();
        result!.Success.Should().BeTrue();
        result.Data!.Title.Should().Be("New Board");
    }
}
