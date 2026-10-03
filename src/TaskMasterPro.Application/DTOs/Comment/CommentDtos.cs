namespace TaskMasterPro.Application.DTOs;

public class CreateCommentRequest {
    public string Content { get; init; } = string.Empty;
}

public class UpdateCommentRequest {
    public string Content { get; init; } = string.Empty;
}

public class CommentDto {
    public Guid Id { get; init; }
    public string Content { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public Guid TaskItemId { get; init; }
    public DateTime CreatedAt { get; init; }
}
