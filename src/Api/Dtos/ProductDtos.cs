using Domain.Products;

namespace Api.Dtos;

public record CreateProductDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required int ProductTypeId { get; init; }
}

public record UpdateProductDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required int ProductTypeId { get; init; }
}

public record ProductDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required int ProductTypeId { get; init; }
    public required string ProductTypeName { get; init; }
    public required DateTime CreatedAt { get; init; }

    public static ProductDto FromDomainModel(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            ProductTypeId = product.ProductTypeId,
            ProductTypeName = product.ProductType?.Name ?? string.Empty,
            CreatedAt = product.CreatedAt
        };
    }
}
