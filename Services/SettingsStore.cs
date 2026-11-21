using System.Security.Cryptography;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using HareketliDuvarKagidi.Models;

namespace HareketliDuvarKagidi.Services;

internal sealed class SettingsStore
{
    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "HareketliDuvarKagidi", "settings.json");
    private bool _canSave = true;
    public Dictionary<string, SavedSelection> Selections { get; private set; } = new(StringComparer.OrdinalIgnoreCase);
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter<DisplayMode>(allowIntegerValues: false) }
    };

    public string? Load()
    {
        try
        {
            if (!File.Exists(_path)) return null;
            var data = JsonSerializer.Deserialize<Dictionary<string, SavedSelection>>(File.ReadAllText(_path), Options)
                ?? throw new JsonException("Ayar kökü boş.");
            if (data.Any(entry => entry.Value == null || !Enum.IsDefined(entry.Value.Mode)))
                throw new JsonException("Geçersiz monitör ayarı.");
            Selections = new(data, StringComparer.OrdinalIgnoreCase);
            var warnings = new List<string>();
            bool changed = false;
            foreach (var entry in Selections.ToArray())
            {
                if (entry.Value.VideoPath == null) continue;
                try
                {
                    string path = ImportVideo(entry.Value.VideoPath);
                    if (!StringComparer.OrdinalIgnoreCase.Equals(path, entry.Value.VideoPath))
                    {
                        Selections[entry.Key] = entry.Value with { VideoPath = path };
                        changed = true;
                    }
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
                {
                    warnings.Add(!File.Exists(entry.Value.VideoPath)
                        ? "Video dosyası bulunamadı. Yeni dosya seçin."
                        : $"Video koleksiyona eklenemedi: {ex.Message}");
                }
            }
            if (changed && Save() is { } error) warnings.Add(error);
            return warnings.Count > 0 ? string.Join(Environment.NewLine, warnings) : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            _canSave = false;
            return $"Ayarlar okunamadı: {ex.Message}";
        }
    }

    public string? Save()
    {
        if (!_canSave) return "Bozuk veya okunamayan settings.json korundu. Dosyayı düzeltip uygulamayı yeniden açın.";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            string temporary = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(Selections, Options));
            if (File.Exists(_path)) File.Replace(temporary, _path, _path + ".bak");
            else File.Move(temporary, _path);
            return null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return $"Ayarlar kaydedilemedi: {ex.Message}";
        }
    }

    public string ImportVideo(string source)
    {
        string extension = Path.GetExtension(source).ToLowerInvariant();
        if (extension is not (".mp4" or ".webm")) throw new IOException("MP4 veya WebM seçin.");
        string directory = Path.Combine(Path.GetDirectoryName(_path)!, "Videos");
        Directory.CreateDirectory(directory);
        using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
        string hash = Convert.ToHexString(SHA256.HashData(input));
        string destination = Path.Combine(directory, hash + extension);
        if (File.Exists(destination))
        {
            using var existing = File.OpenRead(destination);
            if (existing.Length == input.Length && Convert.ToHexString(SHA256.HashData(existing)) == hash)
                return destination;
        }
        input.Position = 0;
        string temporary = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write))
            {
                input.CopyTo(output);
                output.Flush(flushToDisk: true);
            }
            File.Move(temporary, destination, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
        return destination;
    }

    internal sealed record SavedSelection(string? VideoPath, DisplayMode Mode, bool IsPlaying = false);
}


