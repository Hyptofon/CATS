using Api.Modules;
using Application;
using Infrastructure;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// === ВАЖЛИВО: Доступ до HttpContext ===
builder.Services.AddHttpContextAccessor(); 

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.SetupServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddControllers();

// === БЛОК АВТОРИЗАЦІЇ ===
builder.Services.AddTransient<IClaimsTransformation, GoogleClaimsTransformation>();

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
        ValidIssuer = "https://accounts.google.com",
        ValidateAudience = true,
        ValidAudience = googleClientId,
        ValidateLifetime = true
    };
});
// =========================

var app = builder.Build();

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CATS API v1");
        c.RoutePrefix = string.Empty;
        c.InjectStylesheet("/css/swagger-dark.css");
        c.InjectJavascript("/js/swagger-custom.js");
        c.DocumentTitle = "CATS API Documentation";
        c.OAuthClientId("288101212166-e68actio7v4m2olbripk5s3ak1571utl.apps.googleusercontent.com");
        // === ДОДАНО: Щоб кнопка Authorize працювала ===
        //c.OAuthClientId(builder.Configuration["Authentication:Google:ClientId"]);
        c.OAuthAppName("CATS API");
        c.OAuthUsePkce();
        // ==============================================
    });

    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<Infrastructure.Persistence.ApplicationDbContext>();
        await context.Database.MigrateAsync();
        await Infrastructure.Persistence.DbInitializer.SeedAsync(context);
    }
}

app.UseCors();

app.UseRouting();

// === ПОРЯДОК MIDDLEWARE ===
app.UseAuthentication();
app.UseAuthorization();
// ==========================

app.MapControllers();

app.Run();

public partial class Program { }