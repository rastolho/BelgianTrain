using EmployeeSeriesManagement.Application.Interfaces;
using EmployeeSeriesManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeSeriesManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeService, EmployeeService>();
        return services;
    }
}
