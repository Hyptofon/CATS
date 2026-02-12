using Api.Filters;
using FluentValidation;
using Microsoft.OpenApi.Models;

namespace Api.Modules;

public static class SetupModule
{
    public static void SetupServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(options => 
        { 
            options.Filters.Add<ValidationFilter>();
            options.Filters.Add<ValidationExceptionFilter>(); 
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });
        
        services.AddCors();
        services.AddRequestValidators();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGenConfiguration();
    }

    private static void AddCors(this IServiceCollection services)
    {
        services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
                policy.SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));
    }

    private static void AddRequestValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
    }

    private static void AddSwaggerGenConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CATS API",
                Version = "v1",
                Description = "REST API для системи обліку тари"
            });
            
            // === ЗМІНА: Простий ввід JWT токена вручну ===
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Встав свій JWT токен сюди (починається з ey...). Наприклад: Bearer eyJhbGciOiJIUz...",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            // ==============================================

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });

            c.UseAllOfToExtendReferenceSchemas();
        });
    }
}