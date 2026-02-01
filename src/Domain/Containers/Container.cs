using Domain.ContainerTypes;
using Domain.Products;

namespace Domain.Containers;

public class Container
{
    public ContainerId Id { get; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public decimal Volume { get; private set; }
    public ContainerTypeId ContainerTypeId { get; private set; }
    public ContainerStatus Status { get; private set; }
    
    // Current fill pointer
    public Guid? CurrentFillId { get; private set; }
    
    // Denormalized current state
    public Guid? CurrentProductId { get; private set; }
    public Guid? CurrentProductTypeId { get; private set; }
    public DateTime? CurrentProductionDate { get; private set; }
    public decimal? CurrentQuantity { get; private set; }
    public string? CurrentUnit { get; private set; }
    public DateTime? CurrentFilledAt { get; private set; }
    
    // Denormalized last content
    public Guid? LastProductId { get; private set; }
    public Guid? LastProductTypeId { get; private set; }
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
        ContainerId id, 
        string code, 
        string name, 
        decimal volume, 
        ContainerTypeId containerTypeId,
        string? meta,
        Guid? createdById,
        DateTime createdAt)
    {
        Id = id;
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
        ContainerTypeId containerTypeId,
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
            ContainerId.New(),
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
        ContainerTypeId containerTypeId,
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
}