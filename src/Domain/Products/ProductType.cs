namespace Domain.Products;


using Domain.ContainerTypes;

public class ProductType
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public int? ShelfLifeDays { get; private set; }
    public int? ShelfLifeHours { get; private set; }
    public string? Meta { get; private set; }

    public virtual ICollection<ContainerType> CompatibleContainerTypes { get; private set; } = new List<ContainerType>();
    
    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedById { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? LastModifiedById { get; private set; }
    public bool IsDeleted { get; private set; }

    private ProductType(
        string name,
        int? shelfLifeDays,
        int? shelfLifeHours,
        string? meta,
        Guid? createdById,
        DateTime createdAt)
    {
        Name = name;
        ShelfLifeDays = shelfLifeDays;
        ShelfLifeHours = shelfLifeHours;
        Meta = meta;
        CreatedById = createdById;
        CreatedAt = createdAt;
        IsDeleted = false;
    }

    public static ProductType New(
        string name,
        int? shelfLifeDays,
        int? shelfLifeHours,
        string? meta,
        Guid? createdById)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product type name cannot be empty", nameof(name));

        if (shelfLifeDays.HasValue && shelfLifeDays.Value < 0)
            throw new ArgumentException("Shelf life days cannot be negative", nameof(shelfLifeDays));
            
        if (shelfLifeHours.HasValue && shelfLifeHours.Value < 0)
            throw new ArgumentException("Shelf life hours cannot be negative", nameof(shelfLifeHours));

        return new ProductType(
            name,
            shelfLifeDays,
            shelfLifeHours,
            meta,
            createdById,
            DateTime.UtcNow);
    }

    public void UpdateDetails(
        string name,
        int? shelfLifeDays,
        int? shelfLifeHours,
        string? meta,
        Guid? modifiedById)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product type name cannot be empty", nameof(name));

        if (shelfLifeDays.HasValue && shelfLifeDays.Value < 0)
            throw new ArgumentException("Shelf life days cannot be negative", nameof(shelfLifeDays));

        if (shelfLifeHours.HasValue && shelfLifeHours.Value < 0)
            throw new ArgumentException("Shelf life hours cannot be negative", nameof(shelfLifeHours));

        Name = name;
        ShelfLifeDays = shelfLifeDays;
        ShelfLifeHours = shelfLifeHours;
        Meta = meta;
        LastModifiedById = modifiedById;
        UpdatedAt = DateTime.UtcNow;
    }
}