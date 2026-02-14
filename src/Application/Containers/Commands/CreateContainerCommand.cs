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
    public string? Code { get; init; }
    public required string Name { get; init; }
    public required decimal Volume { get; init; }
    public required string Unit { get; init; }
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
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var existingContainer = await containerRepository.GetByCodeAsync(
                request.Code, 
                cancellationToken);

            if (existingContainer.IsSome)
            {
                return new ContainerAlreadyExistException(0);
            }
        }

        var containerTypeId = request.ContainerTypeId;
        var containerType = await containerTypeRepository.GetByIdAsync(
            containerTypeId, 
            cancellationToken);

        return await containerType.MatchAsync(
            ct => CreateEntity(request, ct, cancellationToken),
            () => Task.FromResult<Either<ContainerException, Container>>(
                new ContainerTypeNotFoundForContainerException(containerTypeId)));
    }

    private async Task<Either<ContainerException, Container>> CreateEntity(
        CreateContainerCommand request,
        Domain.ContainerTypes.ContainerType containerType,
        CancellationToken cancellationToken)
    {
        try
        {
            string finalCode;
            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                finalCode = request.Code;
            }
            else
            {
                var prefix = containerType.CodePrefix;
                
                if (string.IsNullOrWhiteSpace(prefix))
                {
                    var words = containerType.Name
                        .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    if (words.Length == 1)
                    {
                        var word = words[0];
                        prefix = word.Length > 4 
                            ? word[..4].ToUpperInvariant() 
                            : word.ToUpperInvariant();
                    }
                    else if (words.Length > 0)
                    {
                        prefix = string.Join("", words.Take(2).Select(w => w[0]))
                            .ToUpperInvariant();
                    }
                    else
                    {
                        prefix = "CONT";
                    }
                }
                
                var existingCount = await containerRepository.GetTotalCountByTypeIdAsync(
                    containerType.Id,
                    cancellationToken);
                
                var nextNumber = existingCount + 1;
                
                var formattedNumber = nextNumber.ToString("D4");
                
                finalCode = $"{prefix}-{formattedNumber}";
            }

            var container = Container.New(
                finalCode,
                request.Name,
                request.Volume,
                request.Unit,
                containerType.Id,
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
