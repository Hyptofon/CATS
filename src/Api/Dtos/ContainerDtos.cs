using System.Text.Json.Serialization;
using Api.Converters;                   
using Domain.Containers;

namespace Api.Dtos;

public record ContainerDto(
    Guid Id,
    string Code,
    string Name,
    decimal Volume,
    Guid ContainerTypeId,
    string ContainerTypeName,
    string Status,
    
    [property: JsonConverter(typeof(JsonStringConverter))] 
    string? Meta,
    
    DateTime CreatedAt)
{
    public static ContainerDto FromDomainModel(Container container)
        => new(
            container.Id.Value,
            container.Code,
            container.Name,
            container.Volume,
            container.ContainerTypeId.Value,
            container.ContainerType?.Name ?? "Unknown",
            container.Status.ToString(),
            container.Meta,
            container.CreatedAt);
}

public record CreateContainerDto(
    string Code,
    string Name,
    decimal Volume,
    Guid ContainerTypeId,
    
    [property: JsonConverter(typeof(JsonStringConverter))] 
    string? Meta
);

public record UpdateContainerDto(
    string Name,
    decimal Volume,
    Guid ContainerTypeId,
    
    [property: JsonConverter(typeof(JsonStringConverter))] 
    string? Meta
);