using DriveNow.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveNow.Infrastructure.Logging
{

    public class FileAuditLogWriter : IAuditLogWriter
    {
        private readonly string _filePath;

        public FileAuditLogWriter(string baseDirectory)
        {
            _filePath = Path.Combine(baseDirectory, "audit.log");
        }

        public void Append(string content)
        {
            File.AppendAllText(_filePath, content);
        }
    }
}
