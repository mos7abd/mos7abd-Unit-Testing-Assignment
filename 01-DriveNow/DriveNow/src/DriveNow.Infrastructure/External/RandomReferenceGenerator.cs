using DriveNow.Application.Abstractions.External;

namespace DriveNow.Infrastructure.External;

public class RandomReferenceGenerator : IReferenceGenerator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string Generate(int length)
    {
        var chars = new char[length];

        for (var i = 0; i < length; i++)
        {
            chars[i] = Alphabet[Random.Shared.Next(Alphabet.Length)];
        }

        return new string(chars);
    }
}