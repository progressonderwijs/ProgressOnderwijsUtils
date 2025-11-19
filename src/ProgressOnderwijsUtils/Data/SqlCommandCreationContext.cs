namespace ProgressOnderwijsUtils;

public interface IAttachedToTracer
{
    ISqlCommandTracer Tracer { get; }
}

public interface IHasDefaultCommandTimeout
{
    CommandTimeoutDefaults TimeoutDefaults { get; }
}

public sealed class SqlConnectionContext(ISqlCommandTracer tracer, CommandTimeoutDefaults timeoutDefaults) : SiteBase, IAttachedToTracer, IHasDefaultCommandTimeout
{
    public ISqlCommandTracer Tracer { get; } = tracer;
    public CommandTimeoutDefaults TimeoutDefaults { get; set; } = timeoutDefaults;
}
