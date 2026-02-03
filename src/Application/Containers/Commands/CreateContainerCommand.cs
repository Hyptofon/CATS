using Microsoft.EntityFrameworkCore; 
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Containers.Exceptions;
using Domain.Containers;
using LanguageExt;
using MediatR;

namespace Application.Containers.Commands;

public record CreateContainerCommand : IRequest<Either<ContainerException, Container>>
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required decimal Volume { get; init; }
    public required int ContainerTypeId { get; init; }
    public string? Meta { get; init; }
}

public class CreateContainerCommandHandler(
    IContainerRepository containerRepository,
    IContainerTypeRepository containerTypeRepository,
    ICurrentUserService currentUserService,
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateContainerCommand, Either<ContainerException, Container>>
{
    public async Task<Either<ContainerException, Container>> Handle(
        CreateContainerCommand request,
        CancellationToken cancellationToken)
    {
        var existingContainer = await containerRepository.GetByCodeAsync(
            request.Code, 
            cancellationToken);

        if (existingContainer.IsSome)
        {
            return new ContainerAlreadyExistException(0);
        }

        var containerTypeId = request.ContainerTypeId;
        var containerType = await containerTypeRepository.GetByIdAsync(
            containerTypeId, 
            cancellationToken);

        return await containerType.MatchAsync(
            ct => CreateEntity(request, containerTypeId, cancellationToken),
            () => Task.FromResult<Either<ContainerException, Container>>(
                new ContainerTypeNotFoundForContainerException(containerTypeId)));
    }

    private async Task<Either<ContainerException, Container>> CreateEntity(
        CreateContainerCommand request,
        int containerTypeId,
        CancellationToken cancellationToken)
    {
        try
        {
            var container = Container.New(
                request.Code,
                request.Name,
                request.Volume,
                containerTypeId,
                request.Meta,
                currentUserService.UserId);

            containerRepository.Add(container);
            
            await dbContext.SaveChangesAsync(cancellationToken);
            
            if (dbContext is DbContext efContext)
            {
                await efContext.Entry(container)
                    .Reference(c => c.ContainerType)
                    .LoadAsync(cancellationToken);
            }

            return container;
        }
        catch (Exception exception)
        {
            return new UnhandledContainerException(0, exception);
        }
    }
}