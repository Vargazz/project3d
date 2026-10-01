namespace Project3D.Domain.Interfaces;

/// <summary>
/// Unit of Work — garante que múltiplas operações de repositório
/// sejam executadas dentro da mesma transação.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
