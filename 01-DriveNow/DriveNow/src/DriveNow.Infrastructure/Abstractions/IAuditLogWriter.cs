using System;
using System.Collections.Generic;
using System.Text;

namespace DriveNow.Infrastructure.Abstractions
{
    public interface IAuditLogWriter
    {
        void Append(string content);
    }
}
