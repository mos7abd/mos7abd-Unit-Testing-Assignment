using DriveNow.Application.Abstractions.External;
using DriveNow.Infrastructure.Abstractions;

namespace DriveNow.Infrastructure.External;

public class EmailSender : IEmailSender
{
    private readonly IEmailOutbox _outbox;
    private readonly TimeProvider _timeProvider;

    public EmailSender(
        IEmailOutbox outbox,
        TimeProvider timeProvider)
    {
        _outbox = outbox;
        _timeProvider = timeProvider;
    }

    public void Send(string to, string subject, string body)
    {
        var now = _timeProvider.GetLocalNow().DateTime;

        var fileName = $"{now:yyyyMMdd_HHmmssfff}_{to}.txt";

        var content =
            $"To: {to}{Environment.NewLine}" +
            $"Subject: {subject}{Environment.NewLine}" +
            $"{Environment.NewLine}{body}";

        _outbox.Write(fileName, content);
    }
}