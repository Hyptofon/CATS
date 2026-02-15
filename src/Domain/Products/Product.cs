namespace Domain.Products;

using Domain.Common;

public class Product : BaseAuditableEntity
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int ProductTypeId { get; private set; }
    public int? ShelfLifeDays { get; private set; }
    public int? ShelfLifeHours { get; private set; }
    
    // Navigation properties
    public ProductType? ProductType { get; private set; }

    private Product(
        string name,
        string? description,
        int productTypeId,
        int? shelfLifeDays,
        int? shelfLifeHours,
        Guid? createdById,
        DateTime createdAt)
    {
        Name = name;
        Description = description;
        ProductTypeId = productTypeId;
        ShelfLifeDays = shelfLifeDays;
        ShelfLifeHours = shelfLifeHours;
        CreatedById = createdById;
        CreatedAt = createdAt;
        IsDeleted = false;
    }

    public static Product New(
        string name,
        string? description,
        int productTypeId,
        int? shelfLifeDays,
        int? shelfLifeHours,
        Guid? createdById)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Product name must not exceed 200 characters", nameof(name));

        if (shelfLifeDays.HasValue && shelfLifeDays.Value < 0)
            throw new ArgumentException("Shelf life days cannot be negative", nameof(shelfLifeDays));

        if (shelfLifeHours.HasValue && shelfLifeHours.Value < 0)
            throw new ArgumentException("Shelf life hours cannot be negative", nameof(shelfLifeHours));

        return new Product(name, description, productTypeId, shelfLifeDays, shelfLifeHours, createdById, DateTime.UtcNow);
    }

    public void UpdateDetails(
        string name,
        string? description,
        int productTypeId,
        int? shelfLifeDays,
        int? shelfLifeHours,
        Guid? modifiedById)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        if (shelfLifeDays.HasValue && shelfLifeDays.Value < 0)
            throw new ArgumentException("Shelf life days cannot be negative", nameof(shelfLifeDays));

        if (shelfLifeHours.HasValue && shelfLifeHours.Value < 0)
            throw new ArgumentException("Shelf life hours cannot be negative", nameof(shelfLifeHours));

        Name = name;
        Description = description;
        ProductTypeId = productTypeId;
        ShelfLifeDays = shelfLifeDays;
        ShelfLifeHours = shelfLifeHours;
        LastModifiedById = modifiedById;
        UpdatedAt = DateTime.UtcNow;
    }
}
