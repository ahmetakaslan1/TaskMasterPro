namespace TaskMasterPro.Application.DTOs;

public class CreateColumnRequest {
    public string Title { get; init; } = string.Empty;
}

public class UpdateColumnRequest {
    public string Title { get; init; } = string.Empty;
}

public class ColumnDto {
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public int OrderIndex { get; init; }
    public Guid BoardId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public class ColumnWithTasksDto {
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public int OrderIndex { get; init; }
    public Guid BoardId { get; init; }
    public List<TaskDto> Tasks { get; init; } = new();
}

public class MoveColumnRequest {
    public int NewOrderIndex { get; init; }
}
