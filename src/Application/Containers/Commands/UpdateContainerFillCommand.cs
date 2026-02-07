using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Containers.Exceptions;
using Domain.Containers;
using LanguageExt;
using MediatR;

namespace Application.Containers.Commands;

public record UpdateContainerFillCommand : IRequest<Either<ContainerException, Container>>
{
    public required int ContainerId { get; init; }
    public int? ProductId { get; init; }
    public required decimal Quantity { get; init; }
    public required string Unit { get; init; }
    public required DateTime ProductionDate { get; init; }
    public required DateTime ExpirationDate { get; init; }
}

public class UpdateContainerFillCommandHandler(
    IContainerRepository containerRepository,
    IContainerFillRepository containerFillRepository,
    IProductRepository productRepository,
    ICurrentUserService currentUserService,
    IApplicationDbContext dbContext)
    : IRequestHandler<UpdateContainerFillCommand, Either<ContainerException, Container>>
{
    public async Task<Either<ContainerException, Container>> Handle(
        UpdateContainerFillCommand request,
        CancellationToken cancellationToken)
    {
        var container = await containerRepository.GetByIdAsync(request.ContainerId, cancellationToken);

        return await container.MatchAsync(
            c => UpdateFillAsync(c, request, cancellationToken),
            () => Task.FromResult<Either<ContainerException, Container>>(
                new ContainerNotFoundException(request.ContainerId)));
    }

    private async Task<Either<ContainerException, Container>> UpdateFillAsync(
        Container container,
        UpdateContainerFillCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.Quantity > container.Volume)
            {
                return new ContainerOverfillException(request.ContainerId, request.Quantity, container.Volume);
            }

            if (request.Unit != container.Unit)
            {
                return new ContainerUnitMismatchException(request.ContainerId, container.Unit, request.Unit);
            }
            
            if (!container.CurrentFillId.HasValue)
            {
                return new ContainerNotFullException(request.ContainerId);
            }

            var currentFill = await containerFillRepository.GetByIdAsync(
                container.CurrentFillId.Value,
                cancellationToken);

            if (currentFill.IsNone)
            {
                return new UnhandledContainerException(request.ContainerId,
                    new ContainerFillNotFoundException(container.CurrentFillId.Value));
            }

            var fill = currentFill.Match(f => f, () => throw new InvalidOperationException());

            int? productTypeId = null;
            if (request.ProductId.HasValue)
            {
                var product = await productRepository.GetByIdAsync(request.ProductId.Value, cancellationToken);
                if (product.IsNone)
                {
                    return new UnhandledContainerException(request.ContainerId,
                        new Exception($"Product with ID {request.ProductId.Value} not found"));
                }

                productTypeId = product.Match(p => p.ProductTypeId, () => (int?)null);
            }

            fill.UpdateDetails(
                request.ProductId,
                request.Quantity,
                request.Unit,
                request.ProductionDate,
                request.ExpirationDate);
            containerFillRepository.Update(fill);

            container.UpdateCurrentFill(
                request.ProductId,
                productTypeId,
                request.Quantity,
                request.Unit,
                request.ProductionDate,
                request.ExpirationDate,
                currentUserService.UserId ?? Guid.Empty);
            containerRepository.Update(container);

            await dbContext.SaveChangesAsync(cancellationToken);

            return container;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not full"))
        {
            return new ContainerNotFullException(request.ContainerId);
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
