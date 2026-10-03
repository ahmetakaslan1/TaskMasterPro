namespace TaskMasterPro.Domain.Entities;

public class Board : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid UserId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<BoardColumn> Columns { get; set; } = new List<BoardColumn>();
}
