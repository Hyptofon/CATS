namespace Api.Dtos;

/// <summary>
/// Дані для наповнення контейнера продуктом
/// </summary>
public record FillContainerDto
{
    /// <summary>ID продукту, яким наповнюється контейнер (обов'язкове)</summary>
    public required int ProductId { get; init; }
    /// <summary>Кількість продукту (обов'язкове)</summary>
    public required decimal Quantity { get; init; }
    /// <summary>Одиниця виміру кількості, наприклад "л", "кг" (обов'язкове)</summary>
    public required string Unit { get; init; }
    /// <summary>Дата виробництва продукту (обов'язкове)</summary>
    public required DateTime ProductionDate { get; init; }
    /// <summary>Дата закінчення терміну придатності. Якщо не вказано — розраховується автоматично з ShelfLife типу продукту</summary>
    public DateTime? ExpirationDate { get; init; }
}

/// <summary>
/// Дані для оновлення поточного наповнення контейнера
/// </summary>
public record UpdateContainerFillDto
{
    /// <summary>Новий ID продукту (необов'язкове — якщо не вказано, продукт не змінюється)</summary>
    public int? ProductId { get; init; }
    /// <summary>Нова кількість продукту (обов'язкове)</summary>
    public required decimal Quantity { get; init; }
    /// <summary>Нова одиниця виміру (обов'язкове)</summary>
    public required string Unit { get; init; }
    /// <summary>Нова дата виробництва (обов'язкове)</summary>
    public required DateTime ProductionDate { get; init; }
    /// <summary>Нова дата закінчення терміну придатності (обов'язкове)</summary>
    public required DateTime ExpirationDate { get; init; }
}

/// <summary>
/// Параметри пошуку записів наповнення контейнерів. Усі поля необов'язкові.
/// </summary>
public record SearchContainerFillsDto
{
    /// <summary>Фільтр по ID продукту</summary>
    public int? ProductId { get; init; }
    /// <summary>Фільтр по ID типу продукту</summary>
    public int? ProductTypeId { get; init; }
    /// <summary>Фільтр по ID контейнера</summary>
    public int? ContainerId { get; init; }
    /// <summary>Початкова дата діапазону (включно)</summary>
    public DateTime? FromDate { get; init; }
    /// <summary>Кінцева дата діапазону (включно)</summary>
    public DateTime? ToDate { get; init; }
    /// <summary>Якщо true — тільки активні наповнення (контейнер ще не спорожнений)</summary>
    public bool? OnlyActive { get; init; }
}

/// <summary>
/// Запис про наповнення контейнера (відповідь API)
/// </summary>
public record ContainerFillDto
{
    /// <summary>Унікальний ID запису наповнення</summary>
    public required int Id { get; init; }
    /// <summary>ID контейнера</summary>
    public required int ContainerId { get; init; }
    /// <summary>Код контейнера (наприклад "МБ-0001")</summary>
    public string? ContainerCode { get; init; }
    /// <summary>ID продукту</summary>
    public required int ProductId { get; init; }
    /// <summary>Назва продукту</summary>
    public required string ProductName { get; init; }
    /// <summary>Кількість продукту</summary>
    public required decimal Quantity { get; init; }
    /// <summary>Одиниця виміру</summary>
    public required string Unit { get; init; }
    /// <summary>Дата виробництва продукту</summary>
    public required DateTime ProductionDate { get; init; }
    /// <summary>Дата та час наповнення контейнера</summary>
    public required DateTime FilledDate { get; init; }
    /// <summary>Дата закінчення терміну придатності</summary>
    public required DateTime ExpirationDate { get; init; }
    /// <summary>Дата та час спорожнення контейнера (null якщо ще наповнений)</summary>
    public DateTime? EmptiedDate { get; init; }
    /// <summary>ID користувача, який наповнив контейнер</summary>
    public required Guid FilledByUserId { get; init; }
    /// <summary>Ім'я користувача, який наповнив контейнер</summary>
    public string? FilledByUserName { get; init; }
    /// <summary>ID користувача, який спорожнив контейнер (null якщо ще наповнений)</summary>
    public Guid? EmptiedByUserId { get; init; }
    /// <summary>Ім'я користувача, який спорожнив контейнер</summary>
    public string? EmptiedByUserName { get; init; }

    public static ContainerFillDto FromDomainModel(Domain.Containers.ContainerFill fill)
    {
        return new ContainerFillDto
        {
            Id = fill.Id,
            ContainerId = fill.ContainerId,
            ContainerCode = fill.Container?.Code,
            ProductId = fill.ProductId,
            ProductName = fill.Product?.Name ?? string.Empty,
            Quantity = fill.Quantity,
            Unit = fill.Unit,
            ProductionDate = fill.ProductionDate,
            FilledDate = fill.FilledDate,
            ExpirationDate = fill.ExpirationDate,
            EmptiedDate = fill.EmptiedDate,
            FilledByUserId = fill.FilledByUserId,
            FilledByUserName = fill.FilledByUser != null ? $"{fill.FilledByUser.FirstName} {fill.FilledByUser.LastName}".Trim() : null,
            EmptiedByUserId = fill.EmptiedByUserId,
            EmptiedByUserName = fill.EmptiedByUser != null ? $"{fill.EmptiedByUser.FirstName} {fill.EmptiedByUser.LastName}".Trim() : null
        };
    }
}
