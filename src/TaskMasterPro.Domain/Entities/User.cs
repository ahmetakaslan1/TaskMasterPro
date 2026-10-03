namespace TaskMasterPro.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Navigation
    public ICollection<Board> Boards { get; set; } = new List<Board>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
