using System.Data;
using System.Reflection;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Containers;
using Domain.ContainerTypes;
using Domain.Entities;
using Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserService currentUserService)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<ProductType> ProductTypes { get; set; }
    public DbSet<ContainerType> ContainerTypes { get; set; }
    public DbSet<Container> Containers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ContainerFill> ContainerFills { get; set; }
    public DbSet<User> Users => Set<User>();
    public DbSet<UserInvitation> UserInvitations => Set<UserInvitation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseAuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasOne(typeof(User), "CreatedByUser")
                    .WithMany()
                    .HasForeignKey("CreatedById")
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity(entityType.ClrType)
                    .HasOne(typeof(User), "LastModifiedByUser")
                    .WithMany()
                    .HasForeignKey("LastModifiedById")
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);
            }
        }
    }

    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        var transaction = await Database.BeginTransactionAsync(cancellationToken);
        return new DbTransactionWrapper(transaction);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedById = currentUserService.UserId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.LastModifiedById = currentUserService.UserId;
                    break;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}