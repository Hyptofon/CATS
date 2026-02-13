namespace Api.Dtos;

public record FillContainerDto
{
    public required int ProductId { get; init; }
    public required decimal Quantity { get; init; }
    public required string Unit { get; init; }
    public required DateTime ProductionDate { get; init; }
    public DateTime? ExpirationDate { get; init; }
}

public record UpdateContainerFillDto
{
    public int? ProductId { get; init; }
    public required decimal Quantity { get; init; }
    public required string Unit { get; init; }
    public required DateTime ProductionDate { get; init; }
    public required DateTime ExpirationDate { get; init; }
}

public record SearchContainerFillsDto
{
    public int? ProductId { get; init; }
    public int? ProductTypeId { get; init; }
    public int? ContainerId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public bool? OnlyActive { get; init; }
}

public record ContainerFillDto
{
    public required int Id { get; init; }
    public required int ContainerId { get; init; }
    public string? ContainerCode { get; init; }
    public required int ProductId { get; init; }
    public required string ProductName { get; init; }
    public required decimal Quantity { get; init; }
    public required string Unit { get; init; }
    public required DateTime ProductionDate { get; init; }
    public required DateTime FilledDate { get; init; }
    public required DateTime ExpirationDate { get; init; }
    public DateTime? EmptiedDate { get; init; }
    public required Guid FilledByUserId { get; init; }
    public Guid? EmptiedByUserId { get; init; }

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
            EmptiedByUserId = fill.EmptiedByUserId
        };
    }
}
