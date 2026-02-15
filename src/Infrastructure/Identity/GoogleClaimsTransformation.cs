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
                var invitation = await context.UserInvitations
                    .FirstOrDefaultAsync(i => i.Email == email && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow);

                if (isFirstUser)
                {
                    user = new User
                    {
                        Email = email,
                        IsActive = true,
                        Role = UserRole.Admin,
                        FirstName = "Admin"
                    };
                    
                    UpdateUserName(user, newIdentity);
                    context.Users.Add(user);
                    await context.SaveChangesAsync();
                }
                else if (invitation != null)
                {
                    user = new User
                    {
                        Email = email,
                        IsActive = true,
                        Role = invitation.Role
                    };
                    
                    UpdateUserName(user, newIdentity);

                    invitation.IsUsed = true;
                    context.Users.Add(user);
                    await context.SaveChangesAsync();
                }
                else
                {
                    return clone;
                }
            }
            else
            {
                UpdateUserName(user, newIdentity);
                await context.SaveChangesAsync();
            }

            var existingClaim = newIdentity.FindFirst("UserId");
            if (existingClaim != null) newIdentity.RemoveClaim(existingClaim);

            newIdentity.AddClaim(new Claim("UserId", user.Id.ToString()));
            newIdentity.AddClaim(new Claim("Status", user.IsActive ? "active" : "pending"));

            if (!newIdentity.HasClaim(c => c.Type == ClaimTypes.Role))
            {
                newIdentity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
            }
        }

        return clone;
    }

    private void UpdateUserName(User user, ClaimsIdentity identity)
    {
        // Try to get specific First/Last name claims first (Standard OIDC claims)
        var givenName = identity.FindFirst("given_name")?.Value ?? 
                        identity.FindFirst(ClaimTypes.GivenName)?.Value;
                        
        var familyName = identity.FindFirst("family_name")?.Value ?? 
                         identity.FindFirst(ClaimTypes.Surname)?.Value;

        if (!string.IsNullOrEmpty(givenName))
        {
            user.FirstName = givenName;
            user.LastName = familyName ?? ""; 
        }
        else
        {
            var name = identity.FindFirst("name")?.Value ?? 
                       identity.FindFirst(ClaimTypes.Name)?.Value;
                       
            if (!string.IsNullOrEmpty(name)) 
            {
                 var parts = name.Split(' ');
                 if (parts.Length > 0) user.FirstName = parts[0];
                 if (parts.Length > 1) user.LastName = parts[^1];
            }
        }
        if (string.IsNullOrEmpty(user.FirstName))
        {
            user.FirstName = "Unknown";
        }
    }
}