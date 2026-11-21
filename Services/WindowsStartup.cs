using System.IO;
using Microsoft.Win32;

namespace HareketliDuvarKagidi.Services;

internal static class WindowsStartup
{
    private const string Preferences = @"Software\HareketliDuvarKagidi";
    private const string Run = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string Name = "HareketliDuvarKagidi";
    public static bool Enabled => (int?)Registry.GetValue(@"HKEY_CURRENT_USER\" + Preferences, "StartWithWindows", 1) != 0;

    public static void Apply(bool enabled)
    {
        string path = Environment.ProcessPath ?? throw new IOException("Uygulama yolu okunamadı.");
        if (Path.GetFileNameWithoutExtension(path).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            path = Path.ChangeExtension(typeof(WindowsStartup).Assembly.Location, ".exe");
        if (!File.Exists(path)) throw new IOException("Uygulamanın çalıştırılabilir dosyası bulunamadı.");
        using var run = Registry.CurrentUser.CreateSubKey(Run);
        if (enabled) run.SetValue(Name, $"\"{path}\" --background", RegistryValueKind.String);
        else run.DeleteValue(Name, throwOnMissingValue: false);
        using var preferences = Registry.CurrentUser.CreateSubKey(Preferences);
        preferences.SetValue("StartWithWindows", enabled ? 1 : 0, RegistryValueKind.DWord);
    }
}
