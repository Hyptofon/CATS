namespace Domain.Products;

public record ProductTypeId(Guid Value)
{
    public static ProductTypeId Empty() => new(Guid.Empty);
    public static ProductTypeId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}