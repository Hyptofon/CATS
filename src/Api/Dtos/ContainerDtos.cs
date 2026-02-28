using System.Text.Json.Serialization;
using Api.Converters;                   
using Domain.Containers;

namespace Api.Dtos;

/// <summary>
/// Повна інформація про контейнер (відповідь API)
/// </summary>
public record ContainerDto
{
    /// <summary>Унікальний ідентифікатор контейнера</summary>
    public required int Id { get; init; }
    /// <summary>Унікальний код контейнера (наприклад "МБ-0001")</summary>
    public required string Code { get; init; }
    /// <summary>Назва контейнера</summary>
    public required string Name { get; init; }
    /// <summary>Об'єм контейнера</summary>
    public required decimal Volume { get; init; }
    /// <summary>Одиниця виміру об'єму (наприклад "л", "кг")</summary>
    public required string Unit { get; init; }
    /// <summary>ID типу контейнера</summary>
    public required int ContainerTypeId { get; init; }
    /// <summary>Назва типу контейнера</summary>
    public required string ContainerTypeName { get; init; }
    /// <summary>Поточний статус контейнера: "Empty" або "Filled"</summary>
    public required string Status { get; init; }
    /// <summary>ID продукту, яким зараз наповнений контейнер (null якщо порожній)</summary>
    public int? CurrentProductId { get; init; }
    /// <summary>Назва продукту, яким зараз наповнений контейнер</summary>
    public string? CurrentProductName { get; init; }
    /// <summary>Поточна кількість продукту в контейнері</summary>
    public decimal? CurrentQuantity { get; init; }
    /// <summary>Дата виробництва поточного продукту</summary>
    public DateTime? CurrentProductionDate { get; init; }
    /// <summary>Дата закінчення терміну придатності поточного продукту</summary>
    public DateTime? CurrentExpirationDate { get; init; }
    /// <summary>Дата та час наповнення контейнера</summary>
    public DateTime? CurrentFilledAt { get; init; }
    /// <summary>Додаткові метадані у форматі JSON (вільне поле)</summary>
    [JsonConverter(typeof(JsonStringConverter))]
    public string? Meta { get; init; }
    /// <summary>Дата та час створення контейнера в системі</summary>
    public required DateTime CreatedAt { get; init; }
    /// <summary>ID користувача, який створив запис</summary>
    public Guid? CreatedById { get; init; }
    /// <summary>Ім'я користувача, який створив запис</summary>
    public string? CreatedByName { get; init; }
    /// <summary>Дата та час останнього оновлення</summary>
    public DateTime? UpdatedAt { get; init; }
    /// <summary>ID користувача, який останнім оновив запис</summary>
    public Guid? LastModifiedById { get; init; }
    /// <summary>Ім'я користувача, який останнім оновив запис</summary>
    public string? LastModifiedByName { get; init; }

    public static ContainerDto FromDomainModel(Container container)
        => new()
        {
            Id = container.Id,
            Code = container.Code,
            Name = container.Name,
            Volume = container.Volume,
            Unit = container.Unit,
            ContainerTypeId = container.ContainerTypeId,
            ContainerTypeName = container.ContainerType?.Name ?? "Unknown",
            Status = container.Status.ToString(),
            CurrentProductId = container.CurrentProductId,
            CurrentProductName = container.CurrentProduct?.Name,
            CurrentQuantity = container.CurrentQuantity,
            CurrentProductionDate = container.CurrentProductionDate,
            CurrentExpirationDate = container.CurrentExpirationDate,
            CurrentFilledAt = container.CurrentFilledAt,
            Meta = container.Meta,
            CreatedAt = container.CreatedAt,
            CreatedById = container.CreatedById,
            CreatedByName = container.CreatedByUser != null ? $"{container.CreatedByUser.FirstName} {container.CreatedByUser.LastName}".Trim() : null,
            UpdatedAt = container.UpdatedAt,
            LastModifiedById = container.LastModifiedById,
            LastModifiedByName = container.LastModifiedByUser != null ? $"{container.LastModifiedByUser.FirstName} {container.LastModifiedByUser.LastName}".Trim() : null
        };
}

/// <summary>
/// Дані для створення нового контейнера
/// </summary>
public record CreateContainerDto
{
    /// <summary>Код контейнера. Якщо не вказано — згенерується автоматично з префіксу типу контейнера</summary>
    public string? Code { get; init; }
    /// <summary>Назва контейнера (обов'язкове)</summary>
    public required string Name { get; init; }
    /// <summary>Об'єм контейнера (обов'язкове)</summary>
    public required decimal Volume { get; init; }
    /// <summary>Одиниця виміру об'єму, наприклад "л", "кг" (обов'язкове)</summary>
    public required string Unit { get; init; }
    /// <summary>ID типу контейнера (обов'язкове)</summary>
    public required int ContainerTypeId { get; init; }
    /// <summary>Додаткові метадані у форматі JSON</summary>
    [JsonConverter(typeof(JsonStringConverter))]
    public string? Meta { get; init; }
}

/// <summary>
/// Дані для оновлення контейнера (код змінити не можна)
/// </summary>
public record UpdateContainerDto
{
    /// <summary>Нова назва контейнера</summary>
    public required string Name { get; init; }
    /// <summary>Новий об'єм контейнера</summary>
    public required decimal Volume { get; init; }
    /// <summary>Нова одиниця виміру</summary>
    public required string Unit { get; init; }
    /// <summary>Новий ID типу контейнера</summary>
    public required int ContainerTypeId { get; init; }
    /// <summary>Нові метадані у форматі JSON</summary>
    [JsonConverter(typeof(JsonStringConverter))]
    public string? Meta { get; init; }
}

/// <summary>
/// Параметри пошуку/фільтрації контейнерів. Усі поля необов'язкові, можна комбінувати.
/// </summary>
public record SearchContainersDto
{
    /// <summary>Текст для пошуку за назвою або кодом контейнера</summary>
    public string? SearchTerm { get; init; }
    /// <summary>Фільтр по ID типу контейнера</summary>
    public int? ContainerTypeId { get; init; }
    /// <summary>Фільтр по статусу контейнера (Empty або Filled)</summary>
    public ContainerStatus? Status { get; init; }
    /// <summary>Фільтр по даті виробництва продукту</summary>
    public DateTime? ProductionDate { get; init; }
    /// <summary>Фільтр по ID поточного продукту в контейнері</summary>
    public int? CurrentProductId { get; init; }
    /// <summary>Фільтр по ID типу поточного продукту</summary>
    public int? CurrentProductTypeId { get; init; }
    /// <summary>Фільтр по ID останнього продукту (який був у контейнері раніше)</summary>
    public int? LastProductId { get; init; }
    /// <summary>Якщо true — показати тільки прострочені; false — тільки не прострочені; null — всі</summary>
    public bool? ShowExpired { get; init; }
    /// <summary>Показати контейнери, наповнені в цей день</summary>
    public DateTime? FilledToday { get; init; }
}
