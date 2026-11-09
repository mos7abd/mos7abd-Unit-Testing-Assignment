using DriveNow.Application.Abstractions.External;
using DriveNow.Domain.Exceptions;

namespace DriveNow.Infrastructure.External;

public class PaymentGateway : IPaymentGateway
{
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly IPaymentFailurePolicy _failurePolicy;
    private readonly IExecutionDelay _delay;

    public PaymentGateway(
        IReferenceGenerator referenceGenerator,
        IPaymentFailurePolicy failurePolicy,
        IExecutionDelay delay)
    {
        _referenceGenerator = referenceGenerator;
        _failurePolicy = failurePolicy;
        _delay = delay;
    }

    public string Charge(int customerId, decimal amount, string description)
    {
        PauseAndMaybeFail();

        Console.WriteLine($"[Payment] Charged {amount:N2} EGP to customer #{customerId}");

        return "PAY-" + _referenceGenerator.Generate(8);
    }

    public void Refund(int customerId, decimal amount, string description)
    {
        PauseAndMaybeFail();

        Console.WriteLine($"[Payment] Refunded {amount:N2} EGP to customer #{customerId}");
    }

    private void PauseAndMaybeFail()
    {
        _delay.Wait(TimeSpan.FromMilliseconds(1500));

        if (_failurePolicy.ShouldFail())
            throw new PaymentFailedException("Gateway timeout");
    }
}