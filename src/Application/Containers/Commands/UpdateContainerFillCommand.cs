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
    public required decimal Quantity { get; init; }
    public required DateTime ProductionDate { get; init; }
    public required DateTime ExpirationDate { get; init; }
}

public class UpdateContainerFillCommandHandler(
    IContainerRepository containerRepository,
    IContainerFillRepository containerFillRepository,
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

            fill.UpdateDetails(
                request.Quantity,
                request.ProductionDate,
                request.ExpirationDate);
            containerFillRepository.Update(fill);

            container.UpdateCurrentFill(
                request.Quantity,
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
        catch (Exception exception)
        {
            return new UnhandledContainerException(request.ContainerId, exception);
        }
    }
}
