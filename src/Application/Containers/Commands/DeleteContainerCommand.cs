using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Containers.Exceptions;
using Domain.Containers;
using LanguageExt;
using MediatR;

namespace Application.Containers.Commands;

public record DeleteContainerCommand(Guid ContainerId) 
    : IRequest<Either<ContainerException, Container>>;

public class DeleteContainerCommandHandler(
    IContainerRepository containerRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<DeleteContainerCommand, Either<ContainerException, Container>>
{
    public async Task<Either<ContainerException, Container>> Handle(
        DeleteContainerCommand request,
        CancellationToken cancellationToken)
    {
        var containerId = new ContainerId(request.ContainerId);
        
        var existingContainer = await containerRepository.GetByIdAsync(
            containerId, 
            cancellationToken);

        return await existingContainer.MatchAsync(
            container => DeleteEntity(container, cancellationToken),
            () => Task.FromResult<Either<ContainerException, Container>>(
                new ContainerNotFoundException(containerId)));
    }
    
    private async Task<Either<ContainerException, Container>> DeleteEntity(
        Container container,
        CancellationToken cancellationToken)
    {
        try
        {
            containerRepository.Delete(container);
            
            await dbContext.SaveChangesAsync(cancellationToken);
            
            return container;
        }
        catch (Exception exception)
        {
            return new UnhandledContainerException(container.Id, exception);
        }
    }
}