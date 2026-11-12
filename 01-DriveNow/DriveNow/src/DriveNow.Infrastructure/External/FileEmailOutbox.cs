using DriveNow.Infrastructure.Abstractions;

namespace DriveNow.Infrastructure.External;

public class FileEmailOutbox : IEmailOutbox
{
    private readonly string _baseDirectory;

    public FileEmailOutbox(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
    }

    public void Write(string fileName, string content)
    {
        var folder = Path.Combine(_baseDirectory, "outbox");

        Directory.CreateDirectory(folder);

        var path = Path.Combine(folder, fileName);

        File.WriteAllText(path, content);
    }
}