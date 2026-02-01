namespace Domain.ContainerTypes;

public class ContainerType
{
    public ContainerTypeId Id { get; }
    public string Name { get; private set; }
    public string? Meta { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedById { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? LastModifiedById { get; private set; }
    public bool IsDeleted { get; private set; }

    private ContainerType(
        ContainerTypeId id,
        string name,
        string? meta,
        Guid? createdById,
        DateTime createdAt)
    {
        Id = id;
        Name = name;
        Meta = meta;
        CreatedById = createdById;
        CreatedAt = createdAt;
        IsDeleted = false;
    }

    public static ContainerType New(
        string name,
        string? meta,
        Guid? createdById)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Container type name cannot be empty", nameof(name));

        return new ContainerType(
            ContainerTypeId.New(),
            name,
            meta,
            createdById,
            DateTime.UtcNow);
    }

    public void UpdateDetails(string name, string? meta, Guid? modifiedById)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Container type name cannot be empty", nameof(name));

        Name = name;
        Meta = meta;
        LastModifiedById = modifiedById;
        UpdatedAt = DateTime.UtcNow;
    }
}