using System.IO;

namespace HareketliDuvarKagidi.Services;

internal static class DiagnosticLog
{
    public static void Write(string message)
    {
        try
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HareketliDuvarKagidi");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "diagnostic.log");
            using var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            if (stream.Length >= 128 * 1024) stream.SetLength(0);
            stream.Position = stream.Length;
            using var writer = new StreamWriter(stream);
            writer.WriteLine($"{DateTimeOffset.Now:O} pid={Environment.ProcessId} {message}");
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
