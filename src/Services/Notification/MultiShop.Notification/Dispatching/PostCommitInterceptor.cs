using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MultiShop.Notification.Dispatching;

/// <summary>
/// <see cref="PostCommitActions"/>'ı veritabanı commit'ine bağlar:
/// açık transaction varsa (event consumer'da MassTransit inbox'ı açar) commit'te, yoksa (test ucu) SaveChanges başarılı olunca çalıştırır.
/// Geri alınan ya da başarısız kayıtta bekleyen işler atılır.
/// </summary>
public sealed class PostCommitInterceptor : DbTransactionInterceptor, ISaveChangesInterceptor
{
    private readonly PostCommitActions _actions;

    public PostCommitInterceptor(PostCommitActions actions)
    {
        _actions = actions;
    }

    public override async Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
        => await _actions.RunAsync(cancellationToken);

    public override void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
        => _actions.RunAsync(CancellationToken.None).GetAwaiter().GetResult();

    public override Task TransactionRolledBackAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        _actions.Discard();
        return Task.CompletedTask;
    }

    public override void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData) => _actions.Discard();

    public async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (!InTransaction(eventData.Context))
            await _actions.RunAsync(cancellationToken);
        return result;
    }

    public int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (!InTransaction(eventData.Context))
            _actions.RunAsync(CancellationToken.None).GetAwaiter().GetResult();
        return result;
    }

    public Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        if (!InTransaction(eventData.Context))
            _actions.Discard();
        return Task.CompletedTask;
    }

    public void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (!InTransaction(eventData.Context))
            _actions.Discard();
    }

    // Transaction içindeki SaveChanges henüz commit değildir; karar TransactionCommitted/RolledBack'te verilir.
    private static bool InTransaction(DbContext? context) => context?.Database.CurrentTransaction is not null;
}
