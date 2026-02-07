using Domain.Products;

namespace Domain.Containers;

public class ContainerFill
{
    public int Id { get; private set; }
    public int ContainerId { get; private set; }
    public int ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public string Unit { get; private set; }
    
    public DateTime ProductionDate { get; private set; }
    public DateTime FilledDate { get; private set; }
    public DateTime ExpirationDate { get; private set; }
    public DateTime? EmptiedDate { get; private set; }
    
    public Guid FilledByUserId { get; private set; }
    public Guid? EmptiedByUserId { get; private set; }
    
    // Navigation properties
    public Container? Container { get; private set; }
    public Product? Product { get; private set; }

    private ContainerFill(
        int containerId,
        int productId,
        decimal quantity,
        string unit,
        DateTime productionDate,
        DateTime expirationDate,
        Guid filledByUserId,
        DateTime filledDate)
    {
        ContainerId = containerId;
        ProductId = productId;
        Quantity = quantity;
        Unit = unit;
        ProductionDate = productionDate;
        ExpirationDate = expirationDate;
        FilledByUserId = filledByUserId;
        FilledDate = filledDate;
    }

    public static ContainerFill New(
        int containerId,
        int productId,
        decimal quantity,
        string unit,
        DateTime productionDate,
        DateTime expirationDate,
        Guid filledByUserId)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
            
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit is required", nameof(unit));

        return new ContainerFill(
            containerId,
            productId,
            quantity,
            unit,
            productionDate,
            expirationDate,
            filledByUserId,
            DateTime.UtcNow);
    }

    public void Close(Guid emptiedByUserId)
    {
        if (EmptiedDate.HasValue)
            throw new InvalidOperationException("Container fill is already closed");

        EmptiedDate = DateTime.UtcNow;
        EmptiedByUserId = emptiedByUserId;
    }

    public void UpdateDetails(
        int? productId,
        decimal quantity,
        string unit,
        DateTime productionDate,
        DateTime expirationDate)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit is required", nameof(unit));

        if (EmptiedDate.HasValue)
            throw new InvalidOperationException("Cannot update a closed container fill");

        if (productId.HasValue)
            ProductId = productId.Value;

        Quantity = quantity;
        Unit = unit;
        ProductionDate = productionDate;
        ExpirationDate = expirationDate;
    }
}
