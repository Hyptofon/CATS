namespace Domain.Common;

public abstract class BaseAuditableEntity
{
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedById { get; set; }
    public Domain.Entities.User? CreatedByUser { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    public Guid? LastModifiedById { get; set; }
    public Domain.Entities.User? LastModifiedByUser { get; set; }
    
    public bool IsDeleted { get; set; }
}
