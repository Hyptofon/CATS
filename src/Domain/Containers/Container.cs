using Domain.ContainerTypes;
using Domain.Products;

namespace Domain.Containers;

public class Container
{
    public int Id { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public decimal Volume { get; private set; }
    public int ContainerTypeId { get; private set; }
    public ContainerStatus Status { get; private set; }
    
    // Current fill pointer
    public int? CurrentFillId { get; private set; }
    
    // Denormalized current state
    public int? CurrentProductId { get; private set; }
    public int? CurrentProductTypeId { get; private set; }
    public DateTime? CurrentProductionDate { get; private set; }
    public DateTime? CurrentExpirationDate { get; private set; }
    public decimal? CurrentQuantity { get; private set; }
    public string? CurrentUnit { get; private set; }
    public DateTime? CurrentFilledAt { get; private set; }
    
    // Denormalized last content
    public int? LastProductId { get; private set; }
    public int? LastProductTypeId { get; private set; }
    public DateTime? LastEmptiedAt { get; private set; }
    
    public string? Meta { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedById { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? LastModifiedById { get; private set; }
    public bool IsDeleted { get; private set; }
    
    // Navigation properties
    public ContainerType? ContainerType { get; private set; }

    private Container(
        string code, 
        string name, 
        decimal volume, 
        int containerTypeId,
        string? meta,
        Guid? createdById,
        DateTime createdAt)
    {
        Code = code;
        Name = name;
        Volume = volume;
        ContainerTypeId = containerTypeId;
        Status = ContainerStatus.Empty;
        Meta = meta;
        CreatedById = createdById;
        CreatedAt = createdAt;
        IsDeleted = false;
    }

    public static Container New(
        string code,
        string name,
        decimal volume,
        int containerTypeId,
        string? meta,
        Guid? createdById)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Container code cannot be empty", nameof(code));
            
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Container name cannot be empty", nameof(name));
            
        if (volume <= 0)
            throw new ArgumentException("Volume must be greater than zero", nameof(volume));

        return new Container(
            code,
            name,
            volume,
            containerTypeId,
            meta,
            createdById,
            DateTime.UtcNow);
    }

    public void UpdateDetails(
        string name,
        decimal volume,
        int containerTypeId,
        string? meta,
        Guid? modifiedById)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Container name cannot be empty", nameof(name));
            
        if (volume <= 0)
            throw new ArgumentException("Volume must be greater than zero", nameof(volume));

        Name = name;
        Volume = volume;
        ContainerTypeId = containerTypeId;
        Meta = meta;
        LastModifiedById = modifiedById;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fill(
        int productId,
        int productTypeId,
        decimal quantity,
        string unit,
        DateTime productionDate,
        DateTime expirationDate,
        int fillId,
        Guid userId)
    {
        if (Status != ContainerStatus.Empty)
            throw new InvalidOperationException("Container is not empty");

        if (quantity > Volume)
            throw new InvalidOperationException($"Quantity ({quantity}) exceeds container volume ({Volume})");

        Status = ContainerStatus.Full;
        CurrentFillId = fillId;
        CurrentProductId = productId;
        CurrentProductTypeId = productTypeId;
        CurrentQuantity = quantity;
        CurrentUnit = unit;
        CurrentProductionDate = productionDate;
        CurrentExpirationDate = expirationDate;
        CurrentFilledAt = DateTime.UtcNow;
        LastModifiedById = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Empty(Guid userId)
    {
        if (Status != ContainerStatus.Full)
            throw new InvalidOperationException("Container is not full");
        LastProductId = CurrentProductId;
        LastProductTypeId = CurrentProductTypeId;
        LastEmptiedAt = DateTime.UtcNow;
        Status = ContainerStatus.Empty;
        CurrentFillId = null;
        CurrentProductId = null;
        CurrentProductTypeId = null;
        CurrentQuantity = null;
        CurrentUnit = null;
        CurrentProductionDate = null;
        CurrentExpirationDate = null;
        CurrentFilledAt = null;
        
        LastModifiedById = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCurrentFill(
        decimal quantity,
        DateTime productionDate,
        DateTime expirationDate,
        Guid userId)
    {
        if (Status != ContainerStatus.Full)
            throw new InvalidOperationException("Container is not full");

        if (quantity > Volume)
            throw new InvalidOperationException($"Quantity ({quantity}) exceeds container volume ({Volume})");

        CurrentQuantity = quantity;
        CurrentProductionDate = productionDate;
        CurrentExpirationDate = expirationDate;
        LastModifiedById = userId;
        UpdatedAt = DateTime.UtcNow;
    }
}
