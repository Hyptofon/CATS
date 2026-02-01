namespace Domain.Containers;

public record ContainerId(Guid Value)
{
    public static ContainerId Empty() => new(Guid.Empty);
    public static ContainerId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}