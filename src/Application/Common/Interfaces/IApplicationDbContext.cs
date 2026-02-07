using System.Data;
using Domain.Containers;
using Domain.ContainerTypes;
using Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Container> Containers { get; }
    DbSet<ContainerType> ContainerTypes { get; }
    DbSet<ProductType> ProductTypes { get; }
    DbSet<Product> Products { get; }
    DbSet<ContainerFill> ContainerFills { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IDbTransactionWrapper : IDbTransaction
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}