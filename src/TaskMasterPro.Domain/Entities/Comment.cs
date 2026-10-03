namespace TaskMasterPro.Domain.Entities;

public class Comment : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid TaskItemId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public TaskItem TaskItem { get; set; } = null!;
}
