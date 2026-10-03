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
public class BoardsController : ControllerBase
{
    private readonly IBoardService _boardService;

    public BoardsController(IBoardService boardService)
    {
        _boardService = boardService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var boards = await _boardService.GetAllAsync(GetUserId(), cancellationToken);
        return Ok(ApiResponse<IEnumerable<BoardSummaryDto>>.Ok(boards));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBoardRequest request, CancellationToken cancellationToken)
    {
        var board = await _boardService.CreateAsync(request, GetUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = board.Id }, ApiResponse<BoardDto>.Ok(board, "Board created successfully."));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var board = await _boardService.GetByIdAsync(id, GetUserId(), cancellationToken);
        return Ok(ApiResponse<BoardDetailDto>.Ok(board));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBoardRequest request, CancellationToken cancellationToken)
    {
        var board = await _boardService.UpdateAsync(id, request, GetUserId(), cancellationToken);
        return Ok(ApiResponse<BoardDto>.Ok(board, "Board updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _boardService.DeleteAsync(id, GetUserId(), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Board deleted successfully."));
    }
    
    // POST /api/boards/{boardId}/columns
    [HttpPost("{boardId:guid}/columns")]
    public async Task<IActionResult> CreateColumn(Guid boardId, [FromBody] CreateColumnRequest request, [FromServices] IColumnService columnService, CancellationToken cancellationToken)
    {
        var column = await columnService.CreateAsync(boardId, request, GetUserId(), cancellationToken);
        return StatusCode(201, ApiResponse<ColumnDto>.Ok(column, "Column created successfully."));
    }
}
