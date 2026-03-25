using CaManagement.Api.Helpers;

namespace CaManagement.Api.Services;

/// <summary>
/// Thread-safe in-memory store with optional snapshot isolation (commit/rollback).
/// Uses <see cref="System.Threading.Lock"/> and primary constructor DI (.NET 8+ pattern).
/// Experimental C# interceptors are not used here: their attributes embed brittle source paths, so cross-cutting
/// behavior is shown with <see cref="Middlewares.ExceptionHandlingMiddleware"/> and <see cref="Configurations.ValidationFilter{T}"/> instead.
/// </summary>
public sealed class CaDataStore(TimeProvider time, ILogger<CaDataStore> logger)
{
    private readonly Lock _gate = new();
    private CaDataSnapshot _committed = SeedData.CreateInitial(time);
    private CaDataSnapshot? _working;

    private CaDataSnapshot Active
    {
        get
        {
            return _working ?? _committed;
        }
    }

    public void BeginTransaction()
    {
        lock (_gate)
        {
            if (_working is not null)
                throw new InvalidOperationException("Nested in-memory transactions are not supported in this PoC.");

            _working = _committed.Clone();
            logger.LogDebug("In-memory transaction started.");
        }
    }

    public void Commit()
    {
        lock (_gate)
        {
            if (_working is null)
                throw new InvalidOperationException("No active in-memory transaction to commit.");

            _committed = _working;
            _working = null;
            logger.LogInformation("In-memory transaction committed.");
        }
    }

    public void Rollback()
    {
        lock (_gate)
        {
            if (_working is null)
                logger.LogWarning("Rollback requested with no active transaction (no-op).");
            else
                logger.LogInformation("In-memory transaction rolled back.");

            _working = null;
        }
    }

    public bool HasActiveTransaction
    {
        get
        {
            lock (_gate)
            {
                return _working is not null;
            }
        }
    }

    public T Query<T>(Func<CaDataSnapshot, T> reader)
    {
        lock (_gate)
        {
            return reader(Active);
        }
    }

    public void Mutate(Action<CaDataSnapshot> mutator)
    {
        lock (_gate)
        {
            mutator(Active);
        }
    }
}
