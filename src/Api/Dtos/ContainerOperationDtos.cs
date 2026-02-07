namespace Api.Dtos;

public record FillContainerDto
{
    public required int ProductId { get; init; }
    public required decimal Quantity { get; init; }
    public required string Unit { get; init; }
    public required DateTime ProductionDate { get; init; }
    public required DateTime ExpirationDate { get; init; }
}

public record UpdateContainerFillDto
{
    public int? ProductId { get; init; }
    public required decimal Quantity { get; init; }
    public required string Unit { get; init; }
    public required DateTime ProductionDate { get; init; }
    public required DateTime ExpirationDate { get; init; }
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
}
