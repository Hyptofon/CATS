using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Containers.Exceptions;
using Domain.Containers;
using LanguageExt;
using MediatR;

namespace Application.Containers.Commands;

public record EmptyContainerCommand : IRequest<Either<ContainerException, Container>>
{
    public required int ContainerId { get; init; }
}

public class EmptyContainerCommandHandler(
    IContainerRepository containerRepository,
    IContainerFillRepository containerFillRepository,
    ICurrentUserService currentUserService,
    IApplicationDbContext dbContext)
    : IRequestHandler<EmptyContainerCommand, Either<ContainerException, Container>>
{
    public async Task<Either<ContainerException, Container>> Handle(
        EmptyContainerCommand request,
        CancellationToken cancellationToken)
    {
        var container = await containerRepository.GetByIdAsync(request.ContainerId, cancellationToken);

        return await container.MatchAsync(
            c => EmptyContainerAsync(c, request, cancellationToken),
            () => Task.FromResult<Either<ContainerException, Container>>(
                new ContainerNotFoundException(request.ContainerId)));
    }

    private async Task<Either<ContainerException, Container>> EmptyContainerAsync(
        Container container,
        EmptyContainerCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!container.CurrentFillId.HasValue)
            {
                return new ContainerNotFullException(request.ContainerId);
            }

            var currentFill = await containerFillRepository.GetByIdAsync(
                container.CurrentFillId.Value,
                cancellationToken);

            if (currentFill.IsNone)
            {
                return new ContainerNotFullException(request.ContainerId);
            }

            var fill = currentFill.Match(f => f, () => throw new InvalidOperationException());

            fill.Close(currentUserService.UserId ?? Guid.Empty);
            containerFillRepository.Update(fill);

            container.Empty(currentUserService.UserId ?? Guid.Empty);
            containerRepository.Update(container);

            await dbContext.SaveChangesAsync(cancellationToken);

            return container;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not full"))
        {
            return new ContainerNotFullException(request.ContainerId);
        }
        catch (Exception exception)
        {
            return new UnhandledContainerException(request.ContainerId, exception);
        }
    }
}
