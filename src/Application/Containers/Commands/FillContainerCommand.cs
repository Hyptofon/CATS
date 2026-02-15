using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Containers.Exceptions;
using Domain.Containers;
using LanguageExt;
using MediatR;

namespace Application.Containers.Commands;

public record FillContainerCommand : IRequest<Either<ContainerException, Container>>
{
    public required int ContainerId { get; init; }
    public required int ProductId { get; init; }
    public required decimal Quantity { get; init; }
    public required string Unit { get; init; }
    public required DateTime ProductionDate { get; init; }
    public DateTime? ExpirationDate { get; init; }
}

public class FillContainerCommandHandler(
    IContainerRepository containerRepository,
    IProductRepository productRepository,
    IContainerFillRepository containerFillRepository,
    ICurrentUserService currentUserService,
    IApplicationDbContext dbContext)
    : IRequestHandler<FillContainerCommand, Either<ContainerException, Container>>
{
    public async Task<Either<ContainerException, Container>> Handle(
        FillContainerCommand request,
        CancellationToken cancellationToken)
    {
        var container = await containerRepository.GetByIdAsync(request.ContainerId, cancellationToken);

        return await container.MatchAsync(
            async c =>
            {
                if (request.Quantity > c.Volume)
                {
                    return new ContainerOverfillException(request.ContainerId, request.Quantity, c.Volume);
                }

                if (!string.Equals(request.Unit, c.Unit, StringComparison.OrdinalIgnoreCase))
                {
                    return new ContainerUnitMismatchException(request.ContainerId, c.Unit, request.Unit);
                }

                var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

                return await product.MatchAsync(
                    p => 
                    {
                        if (c.ContainerType.AllowedProductTypes.Any() && 
                            !c.ContainerType.AllowedProductTypes.Any(pt => pt.Id == p.ProductTypeId))
                        {
                            return Task.FromResult<Either<ContainerException, Container>>(
                                new ContainerCompatibilityException(
                                    request.ContainerId, 
                                    c.ContainerType.Name, 
                                    p.ProductType?.Name ?? "Unknown"));
                        }
                        DateTime expirationDate;
                        if (request.ExpirationDate.HasValue)
                        {
                            expirationDate = request.ExpirationDate.Value;
                        }
                        else
                        {
                            var shelfLifeDays = p.ShelfLifeDays ?? p.ProductType?.ShelfLifeDays;
                            var shelfLifeHours = p.ShelfLifeHours ?? p.ProductType?.ShelfLifeHours;
                            
                            if (shelfLifeDays.HasValue || shelfLifeHours.HasValue)
                            {
                                expirationDate = request.ProductionDate
                                    .AddDays(shelfLifeDays ?? 0)
                                    .AddHours(shelfLifeHours ?? 0);
                            }
                            else
                            {
                                return Task.FromResult<Either<ContainerException, Container>>(
                                    new ContainerExpirationDateRequiredException(request.ContainerId));
                            }
                        }

                        return FillContainer(c, p, request, expirationDate, cancellationToken);
                    },
                    () => Task.FromResult<Either<ContainerException, Container>>(
                        new ProductNotFoundForContainerException(request.ContainerId, request.ProductId)));
            },
            () => Task.FromResult<Either<ContainerException, Container>>(
                new ContainerNotFoundException(request.ContainerId)));
    }

    private async Task<Either<ContainerException, Container>> FillContainer(
        Container container,
        Domain.Products.Product product,
        FillContainerCommand request,
        DateTime expirationDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var containerFill = ContainerFill.New(
                container.Id,
                product.Id,
                request.Quantity,
                request.Unit,
                request.ProductionDate,
                expirationDate,
                currentUserService.UserId ?? Guid.Empty);

            containerFillRepository.Add(containerFill);
            await dbContext.SaveChangesAsync(cancellationToken);
            container.Fill(
                product.Id,
                product.ProductTypeId, 
                request.Quantity,
                request.Unit,
                request.ProductionDate,
                expirationDate,
                containerFill.Id,
                currentUserService.UserId ?? Guid.Empty);

            containerRepository.Update(container);
            await dbContext.SaveChangesAsync(cancellationToken);

            return container;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not empty"))
        {
            return new ContainerNotEmptyException(request.ContainerId);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("exceeds"))
        {
            return new ContainerOverfillException(request.ContainerId, request.Quantity, container.Volume);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Unit mismatch"))
        {
            return new ContainerUnitMismatchException(request.ContainerId, container.Unit, request.Unit);
        }
        catch (Exception exception)
        {
            return new UnhandledContainerException(request.ContainerId, exception);
        }
    }
}