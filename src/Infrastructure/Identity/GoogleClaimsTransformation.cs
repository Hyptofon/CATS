using System.Security.Claims;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity;

public class GoogleClaimsTransformation : IClaimsTransformation
{
    private readonly IServiceScopeFactory _scopeFactory;

    public GoogleClaimsTransformation(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var clone = principal.Clone();
        var newIdentity = (ClaimsIdentity)clone.Identity!;

        var email = newIdentity.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email)) return clone;

        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                bool isFirstUser = !await context.Users.AnyAsync();
                user = new User
                {
                    Email = email,
                    IsActive = true,
                    Role = isFirstUser ? UserRole.Admin : UserRole.Operator
                };
                
                var name = newIdentity.FindFirst(ClaimTypes.Name)?.Value;
                if (!string.IsNullOrEmpty(name)) 
                {
                     var parts = name.Split(' ');
                     user.FirstName = parts[0];
                     if(parts.Length > 1) user.LastName = parts[^1];
                }

                context.Users.Add(user);
                await context.SaveChangesAsync();
            }
            
            var existingClaim = newIdentity.FindFirst("UserId");
            if (existingClaim != null) newIdentity.RemoveClaim(existingClaim);

            newIdentity.AddClaim(new Claim("UserId", user.Id.ToString()));

            if (!newIdentity.HasClaim(c => c.Type == ClaimTypes.Role))
            {
                newIdentity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
            }
        }

        return clone;
    }
}