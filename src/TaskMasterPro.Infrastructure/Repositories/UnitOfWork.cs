using TaskMasterPro.Domain.Entities;
using TaskMasterPro.Domain.Interfaces;
using TaskMasterPro.Infrastructure.Persistence;

namespace TaskMasterPro.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly TaskMasterProDbContext _context;

    public IRepository<User> Users { get; }
    public IRepository<Board> Boards { get; }
    public IRepository<BoardColumn> BoardColumns { get; }
    public IRepository<TaskItem> TaskItems { get; }
    public IRepository<Comment> Comments { get; }

    public UnitOfWork(TaskMasterProDbContext context)
    {
        _context = context;
        Users = new GenericRepository<User>(_context);
        Boards = new GenericRepository<Board>(_context);
        BoardColumns = new GenericRepository<BoardColumn>(_context);
        TaskItems = new GenericRepository<TaskItem>(_context);
        Comments = new GenericRepository<Comment>(_context);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_context.Database.CurrentTransaction != null)
        {
            await _context.Database.CurrentTransaction.CommitAsync(cancellationToken);
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_context.Database.CurrentTransaction != null)
        {
            await _context.Database.CurrentTransaction.RollbackAsync(cancellationToken);
        }
    }
    
    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
