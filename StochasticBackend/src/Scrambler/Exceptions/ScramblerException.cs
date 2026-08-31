namespace StochasticBackend.src.Scrambler.Exceptions
{
    public class ScramblerException: Exception
    {
        public string EntityName { get; } = "";
        public ScramblerException() : base() { }
        public ScramblerException(string message) : base(message) { }
        public ScramblerException(string message, Exception exception) : base(message, exception) { }
        public ScramblerException(string message, string entityName, Exception exception) : base(message, exception)
        {
            EntityName = entityName;
        }
    }
}
