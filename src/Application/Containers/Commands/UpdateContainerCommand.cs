using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Containers.Exceptions;
using Domain.Containers;
using LanguageExt;
using MediatR;

namespace Application.Containers.Commands;

public record UpdateContainerCommand : IRequest<Either<ContainerException, Container>>
{
    public required int ContainerId { get; init; }
    public required string Name { get; init; }
    public required decimal Volume { get; init; }
    public required int ContainerTypeId { get; init; }
    public string? Meta { get; init; }
}

public class UpdateContainerCommandHandler(
    IContainerRepository containerRepository,
    IContainerTypeRepository containerTypeRepository,
    ICurrentUserService currentUserService,
    IApplicationDbContext dbContext)
    : IRequestHandler<UpdateContainerCommand, Either<ContainerException, Container>>
{
    public async Task<Either<ContainerException, Container>> Handle(
        UpdateContainerCommand request,
        CancellationToken cancellationToken)
    {
        var containerId = request.ContainerId;
        var existingContainer = await containerRepository.GetByIdAsync(containerId, cancellationToken);

        return await existingContainer.MatchAsync(
            container => UpdateEntity(container, request, cancellationToken),
            () => Task.FromResult<Either<ContainerException, Container>>(
                new ContainerNotFoundException(containerId)));
    }

    private async Task<Either<ContainerException, Container>> UpdateEntity(
        Container container,
        UpdateContainerCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var containerTypeId = request.ContainerTypeId;
            var containerType = await containerTypeRepository.GetByIdAsync(
                containerTypeId, 
                cancellationToken);

            if (containerType.IsNone)
            {
                return new ContainerTypeNotFoundForContainerException(containerTypeId);
            }

            container.UpdateDetails(
                request.Name,
                request.Volume,
                containerTypeId,
                request.Meta,
                currentUserService.UserId);
            
            containerRepository.Update(container);
            
            await dbContext.SaveChangesAsync(cancellationToken);

            var updatedContainerOption = await containerRepository.GetByIdAsync(
                container.Id, 
                cancellationToken);

            return updatedContainerOption.Match<Either<ContainerException, Container>>(
                c => c,
                () => new ContainerNotFoundException(container.Id)
            );
        }
        catch (Exception exception)
        {
            return new UnhandledContainerException(container.Id, exception);
        }
    }
}