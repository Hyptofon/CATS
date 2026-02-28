using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.Common;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebFactory>
{
    public static readonly Guid MockUserId = new("11111111-1111-1111-1111-111111111111");

    protected readonly ApplicationDbContext Context;
    protected readonly HttpClient Client;
    protected readonly IntegrationTestWebFactory Factory;

    protected BaseIntegrationTest(IntegrationTestWebFactory factory)
    {
        Factory = factory;
        
        var scope = factory.Services.CreateScope();
        Context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        EnsureMockUserExistsAsync().GetAwaiter().GetResult();
    }

    private async Task EnsureMockUserExistsAsync()
    {
        var existingUser = await Context.Users.FirstOrDefaultAsync(u => u.Id == MockUserId);
        if (existingUser == null)
        {
            var user = new Domain.Entities.User
            {
                Id = MockUserId,
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com"
            };
            Context.Users.Add(user);
            await SaveChangesAsync();
        }
    }

    protected async Task SaveChangesAsync()
    {
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }
}