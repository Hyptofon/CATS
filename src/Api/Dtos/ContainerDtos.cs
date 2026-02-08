using System.Text.Json.Serialization;
using Api.Converters;                   
using Domain.Containers;

namespace Api.Dtos;

public record ContainerDto(
    int Id,
    string Code,
    string Name,
    decimal Volume,
    string Unit,
    int ContainerTypeId,
    string ContainerTypeName,
    string Status,
    int? CurrentProductId,
    string? CurrentProductName,
    decimal? CurrentQuantity,
    string? CurrentUnit,
    DateTime? CurrentProductionDate,
    DateTime? CurrentExpirationDate,
    DateTime? CurrentFilledAt,
    
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
            container.Unit,
            container.ContainerTypeId,
            container.ContainerType?.Name ?? "Unknown",
            container.Status.ToString(),
            container.CurrentProductId,
            container.CurrentProduct?.Name,
            container.CurrentQuantity,
            container.CurrentUnit,
            container.CurrentProductionDate,
            container.CurrentExpirationDate,
            container.CurrentFilledAt,
            container.Meta,
            container.CreatedAt);
}

public record CreateContainerDto(
    string? Code,
    string Name,
    decimal Volume,
    string Unit,
    int ContainerTypeId,
    
    [property: JsonConverter(typeof(JsonStringConverter))] 
    string? Meta
);

public record UpdateContainerDto(
    string Name,
    decimal Volume,
    string Unit,
    int ContainerTypeId,
    
    [property: JsonConverter(typeof(JsonStringConverter))] 
    string? Meta
);
