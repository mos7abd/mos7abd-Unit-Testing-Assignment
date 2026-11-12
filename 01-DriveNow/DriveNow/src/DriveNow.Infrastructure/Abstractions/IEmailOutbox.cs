using System;
using System.Collections.Generic;
using System.Text;

namespace DriveNow.Infrastructure.Abstractions
{
    public interface IEmailOutbox
    {
        void Write(string fileName, string content);
    }
}
