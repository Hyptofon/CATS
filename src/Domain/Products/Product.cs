namespace Domain.Products;

public class Product
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int ProductTypeId { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedById { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? LastModifiedById { get; private set; }
    public bool IsDeleted { get; private set; }
    
    // Navigation properties
    public ProductType? ProductType { get; private set; }

    private Product(
        string name,
        string? description,
        int productTypeId,
        Guid? createdById,
        DateTime createdAt)
    {
        Name = name;
        Description = description;
        ProductTypeId = productTypeId;
        CreatedById = createdById;
        CreatedAt = createdAt;
        IsDeleted = false;
    }

    public static Product New(
        string name,
        string? description,
        int productTypeId,
        Guid? createdById)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Product name must not exceed 200 characters", nameof(name));

        return new Product(name, description, productTypeId, createdById, DateTime.UtcNow);
    }

    public void UpdateDetails(
        string name,
        string? description,
        int productTypeId,
        Guid? modifiedById)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        Name = name;
        Description = description;
        ProductTypeId = productTypeId;
        LastModifiedById = modifiedById;
        UpdatedAt = DateTime.UtcNow;
    }
}
