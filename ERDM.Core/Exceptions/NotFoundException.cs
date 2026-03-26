namespace ERDM.Core.Exceptions
{
    public class NotFoundException : DomainException
    {
        public NotFoundException(string entityName, string id)
            : base($"Entity '{entityName}' with id {id} was not found.")
        {
            EntityName = entityName;
            EntityId = id;
        }

        public string EntityName { get; }
        public string EntityId { get; }
    }
}
