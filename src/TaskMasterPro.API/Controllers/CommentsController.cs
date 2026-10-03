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
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCommentRequest request, CancellationToken cancellationToken)
    {
        var comment = await _commentService.UpdateAsync(id, request, GetUserId(), cancellationToken);
        return Ok(ApiResponse<CommentDto>.Ok(comment, "Comment updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _commentService.DeleteAsync(id, GetUserId(), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Comment deleted successfully."));
    }
}
