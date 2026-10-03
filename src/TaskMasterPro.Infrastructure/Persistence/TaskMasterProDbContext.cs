using Microsoft.EntityFrameworkCore;
using TaskMasterPro.Domain.Entities;
using TaskMasterPro.Infrastructure.Persistence.Interceptors;
using System.Reflection;

namespace TaskMasterPro.Infrastructure.Persistence;

public class TaskMasterProDbContext : DbContext
{
    private readonly UpdateAuditableEntitiesInterceptor _auditableEntitiesInterceptor;

    public TaskMasterProDbContext(
        DbContextOptions<TaskMasterProDbContext> options,
        UpdateAuditableEntitiesInterceptor auditableEntitiesInterceptor) 
        : base(options)
    {
        _auditableEntitiesInterceptor = auditableEntitiesInterceptor;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<BoardColumn> BoardColumns => Set<BoardColumn>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableEntitiesInterceptor);
        base.OnConfiguring(optionsBuilder);
    }
}
