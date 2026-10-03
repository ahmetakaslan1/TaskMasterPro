namespace TaskMasterPro.Application.DTOs;

public class CreateTaskRequest {
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid ColumnId { get; init; }
    public string? Priority { get; init; } = "Medium";
    public string? Color { get; init; } = "#3b82f6";
    public DateTime? DueDate { get; init; }
}

public class UpdateTaskRequest {
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Priority { get; init; } = "Medium";
    public string? Color { get; init; } = "#3b82f6";
    public DateTime? DueDate { get; init; }
}

public class MoveTaskRequest {
    public Guid TargetColumnId { get; init; }
    public int NewIndex { get; init; }
}

public class TaskDto {
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Priority { get; init; } = string.Empty;
    public int OrderIndex { get; init; }
    public Guid ColumnId { get; init; }
    public int CommentCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string Color { get; init; } = "#3b82f6";
    public DateTime? DueDate { get; init; }
}

public class TaskDetailDto {
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Priority { get; init; } = string.Empty;
    public int OrderIndex { get; init; }
    public Guid ColumnId { get; init; }
    public int CommentCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string Color { get; init; } = "#3b82f6";
    public DateTime? DueDate { get; init; }
    public List<CommentDto> Comments { get; init; } = new();
}

public class TaskMoveResponseDto {
    public List<TaskDto> SourceColumn { get; init; } = new();
    public List<TaskDto> TargetColumn { get; init; } = new();
}
