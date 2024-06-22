namespace AuthService.Domain.Entities.Common;

public abstract class AuditableEntity : BaseEntity
{
    public Guid? CreatedBy { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }

    protected void SetCreatedBy(Guid userId)
    {
        CreatedBy = userId;
    }

    protected void SetUpdatedBy(Guid userId)
    {
        UpdatedBy = userId;
        MarkAsUpdated();
    }
}