using TaskMasterPro.Domain.Enums;

namespace TaskMasterPro.Domain.Entities;

/// <summary>
/// Named TaskItem (not Task) to avoid conflict with System.Threading.Tasks.Task.
/// </summary>
public class TaskItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public int OrderIndex { get; set; }
    public Guid BoardColumnId { get; set; }
    public string? Color { get; set; }
    public DateTime? DueDate { get; set; }

    // Navigation
    public BoardColumn BoardColumn { get; set; } = null!;
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
