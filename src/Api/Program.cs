using Api.Modules;
using Application;
using Infrastructure;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.SetupServices(builder.Configuration); 

builder.Services.AddTransient<IClaimsTransformation, GoogleClaimsTransformation>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var googleConfig = builder.Configuration.GetSection("Authentication:Google");
    
    options.Authority = "https://accounts.google.com";
    options.Audience = googleConfig["ClientId"];

    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuers = new[] { "https://accounts.google.com", "accounts.google.com" },
        
        ValidateAudience = true,
        ValidAudience = googleConfig["ClientId"],
        
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

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
        c.DocumentTitle = "CATS API Docs";
    });

    // Database Seeding
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<Infrastructure.Persistence.ApplicationDbContext>();
        await context.Database.MigrateAsync();
        await Infrastructure.Persistence.DbInitializer.SeedAsync(context);
    }
}

app.UseCors();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }