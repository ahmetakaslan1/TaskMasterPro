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
public class ColumnsController : ControllerBase
{
    private readonly IColumnService _columnService;

    public ColumnsController(IColumnService columnService)
    {
        _columnService = columnService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateColumnRequest request, CancellationToken cancellationToken)
    {
        var column = await _columnService.UpdateAsync(id, request, GetUserId(), cancellationToken);
        return Ok(ApiResponse<ColumnDto>.Ok(column, "Column updated successfully."));
    }

    [HttpPatch("{id:guid}/move")]
    public async Task<IActionResult> Move(Guid id, [FromBody] MoveColumnRequest request, CancellationToken cancellationToken)
    {
        var updatedColumns = await _columnService.MoveAsync(id, request.NewOrderIndex, GetUserId(), cancellationToken);
        return Ok(ApiResponse<IEnumerable<ColumnDto>>.Ok(updatedColumns, "Column moved successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _columnService.DeleteAsync(id, GetUserId(), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Column deleted successfully."));
    }
    
    // POST /api/columns/{columnId}/tasks
    [HttpPost("{columnId:guid}/tasks")]
    public async Task<IActionResult> CreateTask(Guid columnId, [FromBody] CreateTaskRequest request, [FromServices] ITaskService taskService, CancellationToken cancellationToken)
    {
        // Set columnId dynamically from route even if body has it, to prevent conflict.
        var effectiveRequest = new CreateTaskRequest {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            Color = request.Color,
            DueDate = request.DueDate,
            ColumnId = columnId
        };
        var task = await taskService.CreateAsync(effectiveRequest, GetUserId(), cancellationToken);
        return StatusCode(201, ApiResponse<TaskDto>.Ok(task, "Task created successfully."));
    }
}
