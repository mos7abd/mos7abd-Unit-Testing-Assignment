using System;
using System.Collections.Generic;
using System.Text;

namespace DriveNow.Application.Abstractions.Logging
{
    public interface IAuditLogger
    {
        void Write(string message);
    }
}
