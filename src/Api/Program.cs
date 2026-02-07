using Api.Modules;
using Application;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.SetupServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddControllers();

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
app.MapControllers();

app.Run();

public partial class Program { }