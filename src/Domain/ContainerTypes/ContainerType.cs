namespace Domain.ContainerTypes;

public class ContainerType
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string DefaultUnit { get; private set; }
    public string? Meta { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedById { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? LastModifiedById { get; private set; }
    public bool IsDeleted { get; private set; }

    private ContainerType(
        string name,
        string defaultUnit,
        string? meta,
        Guid? createdById,
        DateTime createdAt)
    {
        Name = name;
        DefaultUnit = defaultUnit;
        Meta = meta;
        CreatedById = createdById;
        CreatedAt = createdAt;
        IsDeleted = false;
    }

    public static ContainerType New(
        string name,
        string defaultUnit,
        string? meta,
        Guid? createdById)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Container type name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(defaultUnit))
            throw new ArgumentException("Default unit cannot be empty", nameof(defaultUnit));

        return new ContainerType(
            name,
            defaultUnit,
            meta,
            createdById,
            DateTime.UtcNow);
    }

    public void UpdateDetails(
        string name, 
        string defaultUnit,
        string? meta, 
        Guid? modifiedById)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Container type name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(defaultUnit))
            throw new ArgumentException("Default unit cannot be empty", nameof(defaultUnit));

        Name = name;
        DefaultUnit = defaultUnit;
        Meta = meta;
        LastModifiedById = modifiedById;
        UpdatedAt = DateTime.UtcNow;
    }
}
