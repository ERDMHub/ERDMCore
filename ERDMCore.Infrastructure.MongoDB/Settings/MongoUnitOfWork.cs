using ERDM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;


namespace ERDMCore.Infrastructure.MongoDB.Settings
{
    public class MongoUnitOfWork : IUnitOfWork
    {
        private readonly IMongoClient _client;
        private readonly ILogger<MongoUnitOfWork> _logger;
        private IClientSessionHandle _session;
        private bool _disposed;

        public MongoUnitOfWork(IMongoClient client, ILogger<MongoUnitOfWork> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool HasActiveTransaction => _session != null && _session.IsInTransaction;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_session == null || !_session.IsInTransaction)
                return 0;

            try
            {
                await _session.CommitTransactionAsync(cancellationToken);
                _logger.LogDebug("Transaction committed successfully");
                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing transaction");
                throw;
            }
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_session == null)
            {
                _session = await _client.StartSessionAsync(cancellationToken: cancellationToken);
            }

            _session.StartTransaction();
            _logger.LogDebug("Transaction started");
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            await SaveChangesAsync(cancellationToken);
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_session != null && _session.IsInTransaction)
            {
                await _session.AbortTransactionAsync(cancellationToken);
                _logger.LogDebug("Transaction rolled back");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _session?.Dispose();
            }
            _disposed = true;
        }
    }
}
