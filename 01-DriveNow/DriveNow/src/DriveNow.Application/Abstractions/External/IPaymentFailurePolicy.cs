using System;
using System.Collections.Generic;
using System.Text;

namespace DriveNow.Application.Abstractions.External
{
    public interface IPaymentFailurePolicy
    {
        bool ShouldFail();
    }
}
