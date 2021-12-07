namespace ProgressOnderwijsUtils
{
    public interface IAttachedToTracer
    {
        ISqlCommandTracer Tracer { get; }
    }

    public interface IHasDefaultCommandTimeout
    {
        CommandTimeoutDefaults TimeoutDefaults { get; }
    }

    public sealed class SqlConnectionContext : SiteBase, IAttachedToTracer, IHasDefaultCommandTimeout
    {
        public SqlConnectionContext(ISqlCommandTracer tracer, CommandTimeoutDefaults timeoutDefaults)
        {
            Tracer = tracer;
            TimeoutDefaults = timeoutDefaults;
        }

        public ISqlCommandTracer Tracer { get; }
        public CommandTimeoutDefaults TimeoutDefaults { get; set; }
    }
}
