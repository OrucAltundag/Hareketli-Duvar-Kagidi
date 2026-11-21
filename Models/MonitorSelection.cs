using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HareketliDuvarKagidi.Models;

public enum DisplayMode { Doldur, Sığdır, Uzat }

public sealed class MonitorSelection(MonitorInfo monitor) : INotifyPropertyChanged
{
    private string? _videoPath;
    private DisplayMode _mode = DisplayMode.Doldur;
    public MonitorInfo Monitor { get; } = monitor;

    public string? VideoPath
    {
        get => _videoPath;
        set { if (_videoPath != value) { _videoPath = value; OnPropertyChanged(); } }
    }

    public DisplayMode Mode
    {
        get => _mode;
        set { if (_mode != value) { _mode = value; OnPropertyChanged(); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
