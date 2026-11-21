namespace HareketliDuvarKagidi.Models;

// Device interface path identifies the Windows monitor device, not its list position.
// It may change when a device is reinstalled or connected through another port.
public sealed record MonitorInfo(
    string Id, string Name, string DeviceName,
    int Width, int Height, int X, int Y, uint? DpiX, uint? DpiY, bool IsPrimary)
{
    public string DisplayName => $"{Name} ({DeviceName}){(IsPrimary ? " — Ana ekran" : "")}";
    public string Resolution => $"{Width} × {Height} piksel";
    public string Position => $"X: {X}, Y: {Y}";
    public string DpiDescription => DpiX is { } x && DpiY is { } y
        ? $"{x} × {y} DPI · %{x * 100.0 / 96:0.#} / %{y * 100.0 / 96:0.#}"
        : "DPI okunamadı";
}
