using DriveNow.Application.Abstractions.External;

namespace DriveNow.Infrastructure.External;

public class ExecutionDelay : IExecutionDelay
{
    public void Wait(TimeSpan duration)
    {
        Thread.Sleep(duration);
    }
}