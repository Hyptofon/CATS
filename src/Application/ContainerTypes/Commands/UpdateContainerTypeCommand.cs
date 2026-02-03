using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.ContainerTypes.Exceptions;
using Domain.ContainerTypes;
using LanguageExt;
using MediatR;

namespace Application.ContainerTypes.Commands;

public record UpdateContainerTypeCommand : IRequest<Either<ContainerTypeException, ContainerType>>
{
    public required int ContainerTypeId { get; init; }
    public required string Name { get; init; }
    public string? Meta { get; init; }
}

public class UpdateContainerTypeCommandHandler(
    IContainerTypeRepository containerTypeRepository,
    ICurrentUserService currentUserService,
    IApplicationDbContext dbContext)
    : IRequestHandler<UpdateContainerTypeCommand, Either<ContainerTypeException, ContainerType>>
{
    public async Task<Either<ContainerTypeException, ContainerType>> Handle(
        UpdateContainerTypeCommand request,
        CancellationToken cancellationToken)
    {
        var containerTypeId = request.ContainerTypeId;
        var existingContainerType = await containerTypeRepository.GetByIdAsync(
            containerTypeId, 
            cancellationToken);

        return await existingContainerType.MatchAsync(
            containerType => UpdateEntity(containerType, request, cancellationToken),
            () => Task.FromResult<Either<ContainerTypeException, ContainerType>>(
                new ContainerTypeNotFoundException(containerTypeId)));
    }

    private async Task<Either<ContainerTypeException, ContainerType>> UpdateEntity(
        ContainerType containerType,
        UpdateContainerTypeCommand request,
        CancellationToken cancellationToken)
    {
        var existingContainerTypeWithSameName = await containerTypeRepository
            .GetByNameAsync(request.Name, cancellationToken);

        if (existingContainerTypeWithSameName.IsSome 
            && existingContainerTypeWithSameName.Map(ct => ct.Id != containerType.Id).IfNone(false))
        {
            return new ContainerTypeAlreadyExistException(containerType.Id);
        }
        
        try
        {
            containerType.UpdateDetails(
                request.Name, 
                request.Meta, 
                currentUserService.UserId);
            
            containerTypeRepository.Update(containerType);
            
            await dbContext.SaveChangesAsync(cancellationToken);
            
            return containerType;
        }
        catch (Exception exception)
        {
            return new UnhandledContainerTypeException(containerType.Id, exception);
        }
    }
}