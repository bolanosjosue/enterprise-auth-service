using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AuthService.Infrastructure.Persistence.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    public AuditableEntityInterceptor(
        ICurrentUserService currentUserService,
        IDateTime dateTime)
    {
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (_currentUserService.UserId.HasValue)
                {
                    var setCreatedByMethod = entry.Entity.GetType()
                        .GetMethod("SetCreatedBy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    setCreatedByMethod?.Invoke(entry.Entity, new object[] { _currentUserService.UserId.Value });
                }
            }

            if (entry.State == EntityState.Modified)
            {
                if (_currentUserService.UserId.HasValue)
                {
                    var setUpdatedByMethod = entry.Entity.GetType()
                        .GetMethod("SetUpdatedBy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    setUpdatedByMethod?.Invoke(entry.Entity, new object[] { _currentUserService.UserId.Value });
                }
            }
        }
    }
}