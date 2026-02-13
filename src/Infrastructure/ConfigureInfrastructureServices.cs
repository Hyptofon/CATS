using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class ConfigureInfrastructureServices
{
    public static void AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddPersistenceServices(configuration);
        services.AddTransient<IEmailSender, SmtpEmailSender>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
    }
}