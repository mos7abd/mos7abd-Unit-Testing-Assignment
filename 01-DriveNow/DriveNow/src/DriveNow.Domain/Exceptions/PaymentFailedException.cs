using System;
using System.Collections.Generic;
using System.Text;

namespace DriveNow.Domain.Exceptions
{
    public class PaymentFailedException : Exception
    {
        public PaymentFailedException(string message)
            : base(message)
        {
        }
    }
}
