using System.Text.Json.Serialization;
using Api.Converters;
using Domain.Products;

namespace Api.Dtos;

/// <summary>
/// Повна інформація про тип продукту (відповідь API)
/// </summary>
public record ProductTypeDto
{
    /// <summary>Унікальний ідентифікатор типу продукту</summary>
    public required int Id { get; init; }
    /// <summary>Назва типу продукту (наприклад "Молоко", "Соняшникова олія")</summary>
    public required string Name { get; init; }
    /// <summary>Термін придатності за замовчуванням в днях для продуктів цього типу</summary>
    public int? ShelfLifeDays { get; init; }
    /// <summary>Термін придатності за замовчуванням в годинах для продуктів цього типу</summary>
    public int? ShelfLifeHours { get; init; }
    /// <summary>Додаткові метадані у форматі JSON</summary>
    [JsonConverter(typeof(JsonStringConverter))]
    public string? Meta { get; init; }
    /// <summary>Дата та час створення типу продукту</summary>
    public required DateTime CreatedAt { get; init; }

    public static ProductTypeDto FromDomainModel(ProductType productType)
        => new()
        {
            Id = productType.Id,
            Name = productType.Name,
            ShelfLifeDays = productType.ShelfLifeDays,
            ShelfLifeHours = productType.ShelfLifeHours,
            Meta = productType.Meta,
            CreatedAt = productType.CreatedAt
        };
}

/// <summary>
/// Дані для створення нового типу продукту
/// </summary>
public record CreateProductTypeDto
{
    /// <summary>Назва типу продукту (унікальна, обов'язкова)</summary>
    public required string Name { get; init; }
    /// <summary>Термін придатності за замовчуванням в днях</summary>
    public int? ShelfLifeDays { get; init; }
    /// <summary>Термін придатності за замовчуванням в годинах</summary>
    public int? ShelfLifeHours { get; init; }
    /// <summary>Додаткові метадані у форматі JSON</summary>
    [JsonConverter(typeof(JsonStringConverter))]
    public string? Meta { get; init; }
}

/// <summary>
/// Дані для оновлення типу продукту
/// </summary>
public record UpdateProductTypeDto
{
    /// <summary>Нова назва типу продукту</summary>
    public required string Name { get; init; }
    /// <summary>Новий термін придатності за замовчуванням в днях</summary>
    public int? ShelfLifeDays { get; init; }
    /// <summary>Новий термін придатності за замовчуванням в годинах</summary>
    public int? ShelfLifeHours { get; init; }
    /// <summary>Нові метадані у форматі JSON</summary>
    [JsonConverter(typeof(JsonStringConverter))]
    public string? Meta { get; init; }
}