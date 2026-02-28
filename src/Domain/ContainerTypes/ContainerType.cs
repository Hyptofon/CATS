namespace Domain.ContainerTypes;

using Products;
using Domain.Common;

public class ContainerType : BaseAuditableEntity
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string DefaultUnit { get; private set; }
    public string CodePrefix { get; private set; }
    public string? Meta { get; private set; }

    public virtual ICollection<ProductType> AllowedProductTypes { get; private set; } = new List<ProductType>();

    private ContainerType(
        string name,
        string codePrefix,
        string defaultUnit,
        string? meta,
        Guid? createdById,
        DateTime createdAt)
    {
        Name = name;
        CodePrefix = codePrefix;
        DefaultUnit = defaultUnit;
        Meta = meta;
        CreatedById = createdById;
        CreatedAt = createdAt;
        IsDeleted = false;
    }

    public static ContainerType New(
        string name,
        string codePrefix,
        string defaultUnit,
        string? meta,
        Guid? createdById)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Container type name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(codePrefix))
            throw new ArgumentException("Code prefix cannot be empty", nameof(codePrefix));

        if (string.IsNullOrWhiteSpace(defaultUnit))
            throw new ArgumentException("Default unit cannot be empty", nameof(defaultUnit));

        return new ContainerType(
            name,
            codePrefix,
            defaultUnit,
            meta,
            createdById,
            DateTime.UtcNow);
    }

    public void UpdateDetails(
        string name, 
        string codePrefix,
        string defaultUnit,
        string? meta, 
        Guid? modifiedById)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Container type name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(codePrefix))
            throw new ArgumentException("Code prefix cannot be empty", nameof(codePrefix));

        if (string.IsNullOrWhiteSpace(defaultUnit))
            throw new ArgumentException("Default unit cannot be empty", nameof(defaultUnit));

        Name = name;
        CodePrefix = codePrefix;
        DefaultUnit = defaultUnit;
        Meta = meta;
        LastModifiedById = modifiedById;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAllowedProductTypes(IEnumerable<ProductType> productTypes)
    {
        AllowedProductTypes.Clear();
        foreach (var type in productTypes)
        {
            AllowedProductTypes.Add(type);
        }
    }
}
