using System.IO;
using System.Diagnostics;
using System.Windows.Threading;
using LibVLCSharp.Shared;
using HareketliDuvarKagidi.Models;

namespace HareketliDuvarKagidi.Services;

internal sealed class WallpaperPlayback(LibVLC engine) : IDisposable
{
    private DesktopSurface? _surface;
    public bool IsDisposed => _surface == null;
    private MediaPlayer? _player;
    private Media? _media;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(300) };
    private readonly Stopwatch _startup = new();
    private bool _reported;
    private MonitorInfo? _monitor;
    private DisplayMode _mode;
    public event Action<string>? Status;

    public void Start(MonitorSelection selection)
    {
        if (!File.Exists(selection.VideoPath)) throw new IOException("Seçilen video dosyası bulunamadı.");
        var monitor = selection.Monitor;
        _monitor = monitor;
        _mode = selection.Mode;
        try
        {
            _surface = new DesktopSurface(monitor);

            _player = new MediaPlayer(engine) { Hwnd = _surface.Handle, EnableMouseInput = false, EnableKeyInput = false };
            _media = new Media(engine, selection.VideoPath!, FromType.FromPath);
            // Native repeat seeks within this input. Keep local-file preroll short on each seek.
            _media.AddOption(":file-caching=200");
            _media.AddOption(":input-repeat=2147483647");
            ApplyMode(selection.Mode);
            if (!_player.Play(_media)) throw new InvalidOperationException("VLC oynatmayı başlatamadı.");
            _startup.Start();
            _timer.Tick += CheckPlayback;
            _timer.Start();
        }
        catch { Dispose(); throw; }
    }

    private void CheckPlayback(object? sender, EventArgs e)
    {
        if (_player == null) return;
        if (_surface?.IsAttached != true || _player.State is VLCState.Error or VLCState.Ended or VLCState.Stopped ||
            (!_reported && _startup.Elapsed.TotalSeconds > 15))
        {
            var reason = _player.State == VLCState.Error ? "Video okunamadı. Dosya bozuk veya codec desteklenmiyor." : _surface?.IsAttached != true ? "Masaüstü bağlantısı kayboldu." : $"VLC durumu: {_player.State}, video çıkışı: {_player.VoutCount}.";
            DiagnosticLog.Write($"playback failed source={_monitor?.DeviceName} state={_player.State} outputs={_player.VoutCount} attached={_surface?.IsAttached}");
            Dispose();
            Status?.Invoke($"Oynatma durdu. {reason}");
        }
        else if (!_reported && _player.State == VLCState.Playing && _player.VoutCount > 0)
        {
            ApplyMode(_mode);
            _reported = true;
            DiagnosticLog.Write($"video ready source={_monitor?.DeviceName} bounds={_monitor?.X},{_monitor?.Y},{_monitor?.Width},{_monitor?.Height} mode={_mode}");
            Status?.Invoke($"Video çıkışı etkin · {_mode}");
        }
    }

    public void UpdateMonitor(MonitorInfo monitor)
    {
        _surface?.UpdateBounds(monitor);
        bool resized = _monitor?.Width != monitor.Width || _monitor.Height != monitor.Height;
        _monitor = monitor;
        if (resized) ApplyMode(_mode);
    }

    public void ApplyMode(DisplayMode mode)
    {
        _mode = mode;
        if (_player == null || _monitor == null) return;
        _player.CropGeometry = null;
        _player.AspectRatio = null;
        _player.Scale = 0;
        string ratio = $"{_monitor.Width}:{_monitor.Height}";
        if (mode == DisplayMode.Doldur) _player.CropGeometry = ratio;
        if (mode == DisplayMode.Uzat) _player.AspectRatio = ratio;
        if (_reported) Status?.Invoke($"Video çıkışı etkin · {_mode}");
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= CheckPlayback;
        _player?.Stop();
        _player?.Dispose();
        _player = null;
        _media?.Dispose();
        _media = null;

        _surface?.Dispose();
        _surface = null;
    }
}





