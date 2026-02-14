using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.ContainerTypes.Exceptions;
using Domain.ContainerTypes;
using LanguageExt;
using MediatR;

namespace Application.ContainerTypes.Commands;

public record CreateContainerTypeCommand : IRequest<Either<ContainerTypeException, ContainerType>>
{
    public required string Name { get; init; }
    public required string CodePrefix { get; init; }
    public required string DefaultUnit { get; init; }
    public string? Meta { get; init; }
    public List<int> AllowedProductTypeIds { get; init; } = new();
}

public class CreateContainerTypeCommandHandler(
    IContainerTypeRepository containerTypeRepository,
    ICurrentUserService currentUserService,
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateContainerTypeCommand, Either<ContainerTypeException, ContainerType>>
{
    public async Task<Either<ContainerTypeException, ContainerType>> Handle(
        CreateContainerTypeCommand request,
        CancellationToken cancellationToken)
    {
        var existingContainerType = await containerTypeRepository.GetByNameAsync(
            request.Name, 
            cancellationToken);

        return await existingContainerType.MatchAsync(
            ct => new ContainerTypeAlreadyExistException(ct.Id),
            () => CreateEntity(request, cancellationToken));
    }

    private async Task<Either<ContainerTypeException, ContainerType>> CreateEntity(
        CreateContainerTypeCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var containerType = ContainerType.New(
                request.Name,
                request.CodePrefix,
                request.DefaultUnit,
                request.Meta,
                currentUserService.UserId);

            if (request.AllowedProductTypeIds.Any())
            {
                var allowedProductTypes = dbContext.ProductTypes
                    .Where(pt => request.AllowedProductTypeIds.Contains(pt.Id))
                    .ToList();
                    
                containerType.SetAllowedProductTypes(allowedProductTypes);
            }

            containerTypeRepository.Add(containerType);
            
            await dbContext.SaveChangesAsync(cancellationToken);

            return containerType;
        }
        catch (Exception exception)
        {
            return new UnhandledContainerTypeException(0, exception);
        }
    }
}
