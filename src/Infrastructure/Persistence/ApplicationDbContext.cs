using System.Data;
using System.Reflection;
using Application.Common.Interfaces;
using Domain.Containers;
using Domain.ContainerTypes;
using Domain.Entities;
using Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<ProductType> ProductTypes { get; set; }
    public DbSet<ContainerType> ContainerTypes { get; set; }
    public DbSet<Container> Containers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ContainerFill> ContainerFills { get; set; }
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        var transaction = await Database.BeginTransactionAsync(cancellationToken);
        return new DbTransactionWrapper(transaction);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}