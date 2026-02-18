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
                policy.SetIsOriginAllowed(_ => true) // У продакшені тут варто вказати конкретні домени
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
                Description = "Backend API for Container Accounting & Tracking System. <br/>" +
                              "Use <b>/dev-auth.html</b> to generate a JWT token for testing."
            });
            
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                              "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                              "Example: 'Bearer eyJhbGciOiJIUzI1NiIx...'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });

            c.UseAllOfToExtendReferenceSchemas();

            var xmlFile = Path.Combine(AppContext.BaseDirectory, "Api.xml");
            if (File.Exists(xmlFile))
                c.IncludeXmlComments(xmlFile);
        });
    }
}