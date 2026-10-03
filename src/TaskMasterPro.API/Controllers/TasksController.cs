using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskMasterPro.API.Models;
using TaskMasterPro.Application.DTOs;
using TaskMasterPro.Application.Interfaces;

namespace TaskMasterPro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var task = await _taskService.GetByIdAsync(id, GetUserId(), cancellationToken);
        return Ok(ApiResponse<TaskDetailDto>.Ok(task));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await _taskService.UpdateAsync(id, request, GetUserId(), cancellationToken);
        return Ok(ApiResponse<TaskDto>.Ok(task, "Task updated successfully."));
    }

    [HttpPatch("{id:guid}/move")]
    public async Task<IActionResult> Move(Guid id, [FromBody] MoveTaskRequest request, CancellationToken cancellationToken)
    {
        var response = await _taskService.MoveAsync(id, request, GetUserId(), cancellationToken);
        return Ok(ApiResponse<TaskMoveResponseDto>.Ok(response, "Task moved successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _taskService.DeleteAsync(id, GetUserId(), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Task deleted successfully."));
    }
    
    // GET /api/tasks/{taskId}/comments
    [HttpGet("{taskId:guid}/comments")]
    public async Task<IActionResult> GetComments(Guid taskId, [FromServices] ICommentService commentService, CancellationToken cancellationToken)
    {
        var comments = await commentService.GetByTaskIdAsync(taskId, GetUserId(), cancellationToken);
        return Ok(ApiResponse<IEnumerable<CommentDto>>.Ok(comments));
    }

    [HttpPost("{taskId:guid}/comments")]
    public async Task<IActionResult> CreateComment(Guid taskId, [FromBody] CreateCommentRequest request, [FromServices] ICommentService commentService, CancellationToken cancellationToken)
    {
        var comment = await commentService.CreateAsync(taskId, request, GetUserId(), cancellationToken);
        return StatusCode(201, ApiResponse<CommentDto>.Ok(comment, "Comment created successfully."));
    }
}
