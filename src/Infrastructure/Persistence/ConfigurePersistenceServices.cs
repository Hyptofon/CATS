using Application.Common.Interfaces;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Infrastructure.Persistence;

public static class ConfigurePersistenceServices
{
    public static void AddPersistenceServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(
            configuration.GetConnectionString("DefaultConnection"));
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<ApplicationDbContext>(options => options
            .UseNpgsql(
                dataSource,
                builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning)));

        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());
        
        services.AddRepositories();
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ContainerTypeRepository>();
        services.AddScoped<IContainerTypeRepository>(provider => 
            provider.GetRequiredService<ContainerTypeRepository>());
        services.AddScoped<IContainerTypeQueries>(provider => 
            provider.GetRequiredService<ContainerTypeRepository>());

        services.AddScoped<ContainerRepository>();
        services.AddScoped<IContainerRepository>(provider => 
            provider.GetRequiredService<ContainerRepository>());
        services.AddScoped<IContainerQueries>(provider => 
            provider.GetRequiredService<ContainerRepository>());

        services.AddScoped<ProductTypeRepository>();
        services.AddScoped<IProductTypeRepository>(provider => 
            provider.GetRequiredService<ProductTypeRepository>());
        services.AddScoped<IProductTypeQueries>(provider => 
            provider.GetRequiredService<ProductTypeRepository>());
    }
}