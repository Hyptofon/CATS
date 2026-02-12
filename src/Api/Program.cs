using Api.Modules;
using Application;
using Infrastructure;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// === 1. Вмикаємо доступ до HttpContext (для CurrentUserService) ===
builder.Services.AddHttpContextAccessor(); 

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.SetupServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddControllers();

// === 2. Реєструємо ClaimsTransformation (Створення юзера в БД) ===
builder.Services.AddTransient<IClaimsTransformation, GoogleClaimsTransformation>();

// === 3. Налаштування валідації токена (Бекенд) ===
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = "https://accounts.google.com";
    
    var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.Audience = googleClientId;

    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        // Дозволяємо обидва варіанти issuer (з https і без)
        ValidIssuers = new[] { "https://accounts.google.com", "accounts.google.com" },
        
        ValidateAudience = true,
        ValidAudience = googleClientId,
        
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true 
    };
});

var app = builder.Build();

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // === 4. Налаштування Swagger UI (Фронтенд авторизації) ===
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CATS API v1");
        c.RoutePrefix = string.Empty;
        c.InjectStylesheet("/css/swagger-dark.css");
        c.InjectJavascript("/js/swagger-custom.js");
        c.DocumentTitle = "CATS API Documentation";
    });

    // Автозаповнення бази даних
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<Infrastructure.Persistence.ApplicationDbContext>();
        await context.Database.MigrateAsync();
        await Infrastructure.Persistence.DbInitializer.SeedAsync(context);
    }
}

app.UseCors();

app.UseRouting();

// === 5. Підключаємо Middleware (Порядок важливий!) ===
app.UseAuthentication();
app.UseAuthorization();
// ====================================================

app.MapControllers();

app.Run();

public partial class Program { }