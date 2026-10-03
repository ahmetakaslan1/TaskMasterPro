using TaskMasterPro.Domain.Entities;

namespace TaskMasterPro.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Board> Boards { get; }
    IRepository<BoardColumn> BoardColumns { get; }
    IRepository<TaskItem> TaskItems { get; }
    IRepository<Comment> Comments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
