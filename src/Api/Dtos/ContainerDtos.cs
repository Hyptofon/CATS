using System.Text.Json.Serialization;
using Api.Converters;                   
using Domain.Containers;

namespace Api.Dtos;

public record ContainerDto(
    int Id,
    string Code,
    string Name,
    decimal Volume,
    int ContainerTypeId,
    string ContainerTypeName,
    string Status,
    
    [property: JsonConverter(typeof(JsonStringConverter))] 
    string? Meta,
    
    DateTime CreatedAt)
{
    public static ContainerDto FromDomainModel(Container container)
        => new(
            container.Id,
            container.Code,
            container.Name,
            container.Volume,
            container.ContainerTypeId,
            container.ContainerType?.Name ?? "Unknown",
            container.Status.ToString(),
            container.Meta,
            container.CreatedAt);
}

public record CreateContainerDto(
    string Code,
    string Name,
    decimal Volume,
    int ContainerTypeId,
    
    [property: JsonConverter(typeof(JsonStringConverter))] 
    string? Meta
);

public record UpdateContainerDto(
    string Name,
    decimal Volume,
    int ContainerTypeId,
    
    [property: JsonConverter(typeof(JsonStringConverter))] 
    string? Meta
);