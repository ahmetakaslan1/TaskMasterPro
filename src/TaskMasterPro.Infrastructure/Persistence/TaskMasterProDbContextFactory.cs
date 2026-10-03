using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using TaskMasterPro.Infrastructure.Persistence.Interceptors;

namespace TaskMasterPro.Infrastructure.Persistence;

public class TaskMasterProDbContextFactory : IDesignTimeDbContextFactory<TaskMasterProDbContext>
{
    public TaskMasterProDbContext CreateDbContext(string[] args)
    {
        // For design-time tools (migrations), we can provide a dummy connection string 
        // or read from an appsettings.json if available.
        var optionsBuilder = new DbContextOptionsBuilder<TaskMasterProDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=TaskMasterPro;Username=postgres;Password=postgres");

        // The interceptor needs to be provided, but it has no dependencies, so we can instantiate it directly.
        var interceptor = new UpdateAuditableEntitiesInterceptor();

        return new TaskMasterProDbContext(optionsBuilder.Options, interceptor);
    }
}
