using ERDM.Core;
using ERDM.Core.Entities;
using ERDM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;


namespace ERDMCore.Infrastructure.MongoDB.Settings
{
    public abstract class MongoRepository<T> : IRepository<T> where T : BaseEntity, new()
    {
        protected readonly IMongoCollection<T> _collection;
        protected readonly ILogger<MongoRepository<T>> _logger;
        protected readonly MongoDbSettings _settings;

        public MongoRepository(
            IMongoDatabase database,
            IOptions<MongoDbSettings> settings,
            ILogger<MongoRepository<T>> logger)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var collectionName = _settings.GetFullCollectionName(typeof(T).Name.ToLower());
            _collection = database.GetCollection<T>(collectionName);
        }

        // Basic CRUD
        public virtual async Task<T> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("GetById: {CollectionName}, Id: {Id}",
                    _collection.CollectionNamespace.CollectionName, id);

                var filter = Builders<T>.Filter.Eq(x => x.Id, id);
                return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity from {CollectionName} with id {Id}",
                    _collection.CollectionNamespace.CollectionName, id);
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("GetAll: {CollectionName}", _collection.CollectionNamespace.CollectionName);

                var filter = Builders<T>.Filter.Empty;
                return await _collection.Find(filter).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all entities from {CollectionName}",
                    _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("GetActive: {CollectionName}", _collection.CollectionNamespace.CollectionName);

                var filter = Builders<T>.Filter.Eq(x => x.IsActive, true);
                return await _collection.Find(filter).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active entities from {CollectionName}",
                    _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(entity.Id))
                    entity.Id = ObjectId.GenerateNewId().ToString();

                if (entity.CreatedAt == default)
                    entity.CreatedAt = DateTime.UtcNow;

                _logger.LogDebug("Add: {CollectionName}, Id: {Id}",
                    _collection.CollectionNamespace.CollectionName, entity.Id);

                await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
                await PublishDomainEvents(entity, cancellationToken);

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding entity to {CollectionName}",
                    _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            try
            {
                entity.UpdatedAt = DateTime.UtcNow;
                _logger.LogDebug("Update: {CollectionName}, Id: {Id}",
                    _collection.CollectionNamespace.CollectionName, entity.Id);

                var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);
                var options = new FindOneAndReplaceOptions<T>
                {
                    ReturnDocument = ReturnDocument.After,
                    IsUpsert = false
                };

                var result = await _collection.FindOneAndReplaceAsync(
                    filter, entity, options, cancellationToken);

                await PublishDomainEvents(entity, cancellationToken);

                return result ?? entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity in {CollectionName}",
                    _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            try
            {
                // Soft delete
                entity.IsActive = false;
                entity.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(entity, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity from {CollectionName}",
                    _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        public virtual async Task DeleteByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
                await DeleteAsync(entity, cancellationToken);
        }

        // Bulk Operations
        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                var entitiesList = entities.ToList();
                foreach (var entity in entitiesList)
                {
                    if (string.IsNullOrEmpty(entity.Id))
                        entity.Id = ObjectId.GenerateNewId().ToString();

                    if (entity.CreatedAt == default)
                        entity.CreatedAt = DateTime.UtcNow;
                }

                _logger.LogDebug("AddRange: {CollectionName}, Count: {Count}",
                    _collection.CollectionNamespace.CollectionName, entitiesList.Count);

                await _collection.InsertManyAsync(entitiesList, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding range to {CollectionName}",
                    _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                var entitiesList = entities.ToList();
                foreach (var entity in entitiesList)
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                }

                _logger.LogDebug("UpdateRange: {CollectionName}, Count: {Count}",
                    _collection.CollectionNamespace.CollectionName, entitiesList.Count);

                var bulkOps = new List<WriteModel<T>>();
                foreach (var entity in entitiesList)
                {
                    var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);
                    bulkOps.Add(new ReplaceOneModel<T>(filter, entity));
                }

                await _collection.BulkWriteAsync(bulkOps, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating range in {CollectionName}",
                    _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                var entitiesList = entities.ToList();
                foreach (var entity in entitiesList)
                {
                    entity.IsActive = false;
                    entity.UpdatedAt = DateTime.UtcNow;
                }

                await UpdateRangeAsync(entitiesList, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting range from {CollectionName}",
                    _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        // Query Operations
        public virtual async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Find: {CollectionName}", _collection.CollectionNamespace.CollectionName);

                return await _collection.Find(predicate).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding entities in {CollectionName}",
                    _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        public virtual async Task<T> FindOneAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("FindOne: {CollectionName}", _collection.CollectionNamespace.CollectionName);

                return await _collection.Find(predicate).FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding single entity in {CollectionName}",
                    _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        public virtual async Task<int> CountAsync(
            Expression<Func<T, bool>> predicate = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var filter = predicate ?? Builders<T>.Filter.Empty;
                var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
                return (int)count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting entities in {CollectionName}",
                    _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var count = await CountAsync(predicate, cancellationToken);
            return count > 0;
        }

        // Paging
        public virtual async Task<PaginatedResult<T>> GetPaginatedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate = null, Expression<Func<T, object>> sortBy = null, bool sortDescending = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var filter = predicate ?? Builders<T>.Filter.Empty;
                var totalCount = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

                var findFluent = _collection.Find(filter);

                // Apply sorting
                if (sortBy != null)
                {
                    findFluent = sortDescending
                        ? findFluent.SortByDescending(sortBy)
                        : findFluent.SortBy(sortBy);
                }
                else
                {
                    // Default sort by CreatedAt
                    findFluent = findFluent.SortByDescending(x => x.CreatedAt);
                }

                var data = await findFluent
                    .Skip((pageNumber - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync(cancellationToken);

                return new PaginatedResult<T>(data, (int)totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated results from {CollectionName}",
                    _collection.CollectionNamespace.CollectionName);
                throw;
            }
        }
        // Helper method to publish domain events
        protected virtual async Task PublishDomainEvents(T entity, CancellationToken cancellationToken)
        {
            // Domain events publishing logic
            await Task.CompletedTask;
        }
    }
}
