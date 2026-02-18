using Domain.Products;

namespace Api.Dtos;

/// <summary>
/// Дані для створення нового продукту
/// </summary>
public record CreateProductDto
{
    /// <summary>Назва продукту (обов'язкове)</summary>
    public required string Name { get; init; }
    /// <summary>Опис продукту</summary>
    public string? Description { get; init; }
    /// <summary>ID типу продукту, до якого належить цей продукт (обов'язкове)</summary>
    public required int ProductTypeId { get; init; }
    /// <summary>Термін придатності в днях (якщо не вказано — береться з типу продукту)</summary>
    public int? ShelfLifeDays { get; init; }
    /// <summary>Термін придатності в годинах (якщо не вказано — береться з типу продукту)</summary>
    public int? ShelfLifeHours { get; init; }
}

/// <summary>
/// Дані для оновлення продукту
/// </summary>
public record UpdateProductDto
{
    /// <summary>Нова назва продукту</summary>
    public required string Name { get; init; }
    /// <summary>Новий опис продукту</summary>
    public string? Description { get; init; }
    /// <summary>Новий ID типу продукту</summary>
    public required int ProductTypeId { get; init; }
    /// <summary>Новий термін придатності в днях</summary>
    public int? ShelfLifeDays { get; init; }
    /// <summary>Новий термін придатності в годинах</summary>
    public int? ShelfLifeHours { get; init; }
}

/// <summary>
/// Повна інформація про продукт (відповідь API)
/// </summary>
public record ProductDto
{
    /// <summary>Унікальний ідентифікатор продукту</summary>
    public required int Id { get; init; }
    /// <summary>Назва продукту</summary>
    public required string Name { get; init; }
    /// <summary>Опис продукту</summary>
    public string? Description { get; init; }
    /// <summary>ID типу продукту</summary>
    public required int ProductTypeId { get; init; }
    /// <summary>Назва типу продукту</summary>
    public required string ProductTypeName { get; init; }
    /// <summary>Термін придатності в днях (може бути null, якщо вказано тільки години)</summary>
    public int? ShelfLifeDays { get; init; }
    /// <summary>Термін придатності в годинах (може бути null, якщо вказано тільки дні)</summary>
    public int? ShelfLifeHours { get; init; }
    /// <summary>Дата та час створення продукту в системі</summary>
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
            ShelfLifeDays = product.ShelfLifeDays,
            ShelfLifeHours = product.ShelfLifeHours,
            CreatedAt = product.CreatedAt
        };
    }
}
