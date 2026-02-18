using System.Text.Json.Serialization;
using Api.Converters;
using Domain.ContainerTypes;

namespace Api.Dtos;

/// <summary>
/// Повна інформація про тип контейнера (відповідь API)
/// </summary>
public record ContainerTypeDto
{
    /// <summary>Унікальний ідентифікатор типу контейнера</summary>
    public required int Id { get; init; }
    /// <summary>Назва типу контейнера (наприклад "Металева бочка")</summary>
    public required string Name { get; init; }
    /// <summary>Короткий префікс для генерації кодів контейнерів (наприклад "МБ")</summary>
    public required string CodePrefix { get; init; }
    /// <summary>Одиниця виміру за замовчуванням для контейнерів цього типу</summary>
    public required string DefaultUnit { get; init; }
    /// <summary>Додаткові метадані у форматі JSON</summary>
    [JsonConverter(typeof(JsonStringConverter))]
    public string? Meta { get; init; }
    /// <summary>Дата та час створення типу контейнера</summary>
    public required DateTime CreatedAt { get; init; }
    /// <summary>Список назв типів продуктів, які можна заливати в контейнери цього типу</summary>
    public required List<string> AllowedProductTypeNames { get; init; }

    public static ContainerTypeDto FromDomainModel(ContainerType containerType)
        => new()
        {
            Id = containerType.Id,
            Name = containerType.Name,
            CodePrefix = containerType.CodePrefix,
            DefaultUnit = containerType.DefaultUnit,
            Meta = containerType.Meta,
            CreatedAt = containerType.CreatedAt,
            AllowedProductTypeNames = containerType.AllowedProductTypes.Select(pt => pt.Name).ToList()
        };
}

/// <summary>
/// Дані для створення нового типу контейнера
/// </summary>
public record CreateContainerTypeDto
{
    /// <summary>Назва типу контейнера (унікальна, обов'язкова)</summary>
    public required string Name { get; init; }
    /// <summary>Короткий префікс для кодів, наприклад "МБ" (обов'язковий)</summary>
    public required string CodePrefix { get; init; }
    /// <summary>Одиниця виміру за замовчуванням (обов'язкова)</summary>
    public required string DefaultUnit { get; init; }
    /// <summary>Додаткові метадані у форматі JSON</summary>
    [JsonConverter(typeof(JsonStringConverter))]
    public string? Meta { get; init; }
    /// <summary>Список ID типів продуктів, які можна заливати в контейнери цього типу</summary>
    public required List<int> AllowedProductTypeIds { get; init; }
}

/// <summary>
/// Дані для оновлення типу контейнера
/// </summary>
public record UpdateContainerTypeDto
{
    /// <summary>Нова назва типу контейнера</summary>
    public required string Name { get; init; }
    /// <summary>Новий префікс для кодів</summary>
    public required string CodePrefix { get; init; }
    /// <summary>Нова одиниця виміру за замовчуванням</summary>
    public required string DefaultUnit { get; init; }
    /// <summary>Нові метадані у форматі JSON</summary>
    [JsonConverter(typeof(JsonStringConverter))]
    public string? Meta { get; init; }
    /// <summary>Новий список ID дозволених типів продуктів</summary>
    public required List<int> AllowedProductTypeIds { get; init; }
}
