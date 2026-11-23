namespace USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

/// <summary>
/// Coordinates repository persistence as a single unit of work.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
