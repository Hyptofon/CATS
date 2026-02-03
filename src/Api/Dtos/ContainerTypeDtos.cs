using System.Text.Json.Serialization;
using Api.Converters;
using Domain.ContainerTypes;

namespace Api.Dtos;

public record ContainerTypeDto(
    int Id, 
    string Name, 
    
    [property: JsonConverter(typeof(JsonStringConverter))] 
    string? Meta, 
    
    DateTime CreatedAt)
{
    public static ContainerTypeDto FromDomainModel(ContainerType containerType)
        => new(
            containerType.Id, 
            containerType.Name, 
            containerType.Meta, 
            containerType.CreatedAt);
}

public record CreateContainerTypeDto(
    string Name, 
    
    [property: JsonConverter(typeof(JsonStringConverter))] 
    string? Meta
);

public record UpdateContainerTypeDto(
    string Name, 
    
    [property: JsonConverter(typeof(JsonStringConverter))] 
    string? Meta
);