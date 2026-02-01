using System.Text.Json.Serialization;
using Api.Converters;
using Domain.Products;

namespace Api.Dtos;

public record ProductTypeDto(
    Guid Id, 
    string Name, 
    int? ShelfLifeDays, 
    
    [property: JsonConverter(typeof(JsonStringConverter))] 
    string? Meta, 
    
    DateTime CreatedAt)
{
    public static ProductTypeDto FromDomainModel(ProductType productType)
        => new(
            productType.Id.Value, 
            productType.Name, 
            productType.ShelfLifeDays,
            productType.Meta, 
            productType.CreatedAt);
}

public record CreateProductTypeDto(
    string Name, 
    int? ShelfLifeDays, 
    
    [property: JsonConverter(typeof(JsonStringConverter))] 
    string? Meta
);

public record UpdateProductTypeDto(
    string Name, 
    int? ShelfLifeDays, 
    
    [property: JsonConverter(typeof(JsonStringConverter))] 
    string? Meta
);