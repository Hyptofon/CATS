using System.Text.Json.Serialization;
using Api.Converters;
using Domain.ContainerTypes;

namespace Api.Dtos;

public record ContainerTypeDto(
    int Id, 
    string Name,
    string DefaultUnit,
    
    [property: JsonConverter(typeof(JsonStringConverter))] 
    string? Meta, 
    
    DateTime CreatedAt,
    List<string> AllowedProductTypeNames)
{
    public static ContainerTypeDto FromDomainModel(ContainerType containerType)
        => new(
            containerType.Id, 
            containerType.Name,
            containerType.DefaultUnit,
            containerType.Meta, 
            containerType.CreatedAt,
            containerType.AllowedProductTypes.Select(pt => pt.Name).ToList());
}

public record CreateContainerTypeDto(
    string Name,
    string DefaultUnit,
    
    [property: JsonConverter(typeof(JsonStringConverter))] 
    string? Meta,
    List<int> AllowedProductTypeIds
);

public record UpdateContainerTypeDto(
    string Name,
    string DefaultUnit,
    
    [property: JsonConverter(typeof(JsonStringConverter))] 
    string? Meta,
    List<int> AllowedProductTypeIds
);
