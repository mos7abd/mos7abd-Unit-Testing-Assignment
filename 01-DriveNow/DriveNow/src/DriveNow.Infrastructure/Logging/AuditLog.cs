namespace DriveNow.Infrastructure.Logging;
using global::DriveNow.Application.Abstractions.Logging;
using global::DriveNow.Infrastructure.Abstractions;


public class AuditLog : IAuditLogger
{
    private readonly IAuditLogWriter _writer;
    private readonly TimeProvider _timeProvider;

    public AuditLog(
        IAuditLogWriter writer,
        TimeProvider timeProvider)
    {
        _writer = writer;
        _timeProvider = timeProvider;
    }

    public void Write(string message)
    {
        var now = _timeProvider.GetLocalNow();

        _writer.Append(
            $"[{now:O}] {message}{Environment.NewLine}");
    }
}
