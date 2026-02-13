namespace Domain.Common;

public abstract class BaseAuditableEntity
{
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedById { get; set; } 
    public DateTime? UpdatedAt { get; set; }
    public Guid? LastModifiedById { get; set; }
    public bool IsDeleted { get; set; }
}
