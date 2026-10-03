namespace TaskMasterPro.Domain.Entities;

/// <summary>
/// Named BoardColumn (not Column) to avoid conflict with EF Core's [Column] attribute.
/// Table name: BoardColumns
/// </summary>
public class BoardColumn : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public Guid BoardId { get; set; }

    // Navigation
    public Board Board { get; set; } = null!;
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
