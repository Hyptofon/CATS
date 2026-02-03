using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.ContainerTypes.Exceptions;
using Domain.ContainerTypes;
using LanguageExt;
using MediatR;

namespace Application.ContainerTypes.Commands;

public record DeleteContainerTypeCommand(int ContainerTypeId) 
    : IRequest<Either<ContainerTypeException, ContainerType>>;

public class DeleteContainerTypeCommandHandler(
    IContainerTypeRepository containerTypeRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<DeleteContainerTypeCommand, Either<ContainerTypeException, ContainerType>>
{
    public async Task<Either<ContainerTypeException, ContainerType>> Handle(
        DeleteContainerTypeCommand request,
        CancellationToken cancellationToken)
    {
        var containerTypeId = request.ContainerTypeId;
        
        var hasContainers = await containerTypeRepository.HasContainersAsync(
            containerTypeId, 
            cancellationToken);
        
        if (hasContainers)
        {
            return new ContainerTypeCannotBeDeletedException(containerTypeId);
        }
        
        var existingContainerType = await containerTypeRepository.GetByIdAsync(
            containerTypeId, 
            cancellationToken);

        return await existingContainerType.MatchAsync(
            containerType => DeleteEntity(containerType, cancellationToken),
            () => Task.FromResult<Either<ContainerTypeException, ContainerType>>(
                new ContainerTypeNotFoundException(containerTypeId)));
    }

    private async Task<Either<ContainerTypeException, ContainerType>> DeleteEntity(
        ContainerType containerType,
        CancellationToken cancellationToken)
    {
        try
        {
            containerTypeRepository.Delete(containerType);
            
            await dbContext.SaveChangesAsync(cancellationToken);
            
            return containerType;
        }
        catch (Exception exception)
        {
            return new UnhandledContainerTypeException(containerType.Id, exception);
        }
    }
}