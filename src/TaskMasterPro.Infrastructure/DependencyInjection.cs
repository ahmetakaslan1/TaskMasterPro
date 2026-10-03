using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskMasterPro.Application.Interfaces;
using TaskMasterPro.Domain.Interfaces;
using TaskMasterPro.Infrastructure.Authentication;
using TaskMasterPro.Infrastructure.Persistence;
using TaskMasterPro.Infrastructure.Persistence.Interceptors;
using TaskMasterPro.Infrastructure.Repositories;

namespace TaskMasterPro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<UpdateAuditableEntitiesInterceptor>();
        
        services.AddDbContext<TaskMasterProDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<UpdateAuditableEntitiesInterceptor>();
            
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                   .AddInterceptors(interceptor);
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        
        services.AddSingleton<IJwtProvider, JwtProvider>();

        return services;
    }
}
