using DriveNow.Application.Abstractions.External;

namespace DriveNow.Infrastructure.External;

public class SmsSender : ISmsSender
{
    private readonly IExecutionDelay _delay;

    public SmsSender(IExecutionDelay delay)
    {
        _delay = delay;
    }

    public void Send(string phone, string text)
    {
        _delay.Wait(TimeSpan.FromMilliseconds(500));

        Console.WriteLine($"[SMS to {phone}] {text}");
    }
}