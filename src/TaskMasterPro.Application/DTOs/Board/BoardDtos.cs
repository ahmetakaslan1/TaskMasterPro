namespace TaskMasterPro.Application.DTOs;

public class CreateBoardRequest {
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public class UpdateBoardRequest {
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public class BoardSummaryDto {
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public class BoardDto {
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid UserId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public class BoardDetailDto {
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid UserId { get; init; }
    public List<ColumnWithTasksDto> Columns { get; init; } = new();
}
