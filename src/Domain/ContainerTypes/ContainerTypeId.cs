namespace Domain.ContainerTypes;

public record ContainerTypeId(Guid Value)
{
    public static ContainerTypeId Empty() => new(Guid.Empty);
    public static ContainerTypeId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}