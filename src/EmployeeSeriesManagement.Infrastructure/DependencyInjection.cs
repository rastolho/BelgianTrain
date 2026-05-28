using EmployeeSeriesManagement.Application.Interfaces;
using EmployeeSeriesManagement.Infrastructure.Data;
using EmployeeSeriesManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeSeriesManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is required. Set it via appsettings.json " +
                "or the ConnectionStrings__DefaultConnection environment variable.");

        services.AddDbContext<EmployeeSeriesDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();

        return services;
    }
}
