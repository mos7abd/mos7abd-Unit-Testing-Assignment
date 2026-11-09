using DriveNow.Application.Abstractions.External;

namespace DriveNow.Infrastructure.External;

public class PaymentFailurePolicy : IPaymentFailurePolicy
{
    public bool ShouldFail()
    {
        return Random.Shared.Next(10) == 0;
    }
}