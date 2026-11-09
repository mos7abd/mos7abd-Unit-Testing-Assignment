using System;
using System.Collections.Generic;
using System.Text;

namespace DriveNow.Application.Abstractions.External
{
    public interface IEmailSender
    {
        void Send(string to, string subject, string body);
    }
}
