namespace ERDM.Core.Exceptions
{
    public class ConcurrencyException : DomainException
    {
        public ConcurrencyException(string entityName, string id)
            : base($"Concurrency conflict on entity '{entityName}' with id {id}")
        {
            EntityName = entityName;
            EntityId = id;
        }

        public string EntityName { get; }
        public string EntityId { get; }
    }
}
