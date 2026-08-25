namespace StochasticBackend.src.Shared.DatabasePSQL
{
    public class DatabaseException: Exception
    {
        public string EntityName { get; } = "";
        public DatabaseException(): base(){}
        public DatabaseException(string message): base(message){ }
        public DatabaseException(string message, Exception exception): base(message, exception){ }
        public DatabaseException(string message, string entityName, Exception exception): base(message, exception)
        {
            EntityName = entityName;
        }
    }
}
