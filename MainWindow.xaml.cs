using System.IO;
using System.Windows;
using System.Windows.Interop;
using HareketliDuvarKagidi.Models;
using HareketliDuvarKagidi.Services;
using Microsoft.Win32;
using LibVLCSharp.Shared;

namespace HareketliDuvarKagidi;

public partial class MainWindow : Window
{
    private readonly MonitorService _monitorService = new();
    private readonly Dictionary<string, MonitorSelection> _selections = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, WallpaperPlayback> _sessions = new(StringComparer.OrdinalIgnoreCase);
    private readonly SettingsStore _settings = new();
    private LibVLC? _engine;
    private HwndSource? _source;
    private readonly System.Windows.Threading.DispatcherTimer _displayTimer = new();
    private static readonly TimeSpan ScanInterval = TimeSpan.FromSeconds(5);
    private bool _refreshing;
    private bool _closed;
    private bool _exiting;
    private bool _loading = true;
    private readonly HashSet<string> _restoreAttempted = new(StringComparer.OrdinalIgnoreCase);

    public MainWindow()
    {
        InitializeComponent();
        try
        {
            StartupCheckBox.IsChecked = WindowsStartup.Enabled;
            WindowsStartup.Apply(StartupCheckBox.IsChecked == true);
        }
        catch (Exception ex) { StatusText.Text = $"Windows başlangıcı ayarlanamadı: {ex.Message}"; }
        Closing += (_, e) =>
        {
            if (_exiting) return;
            e.Cancel = true;
            Hide();
            DiagnosticLog.Write($"window hidden sessions={_sessions.Count}");
        };
        ModeBox.ItemsSource = Enum.GetValues<DisplayMode>();
        _displayTimer.Tick += (_, _) => RefreshMonitors();
        SourceInitialized += async (_, _) =>
        {
            _source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            _source?.AddHook(WindowMessage);
            IsEnabled = false;
            string? warning = await System.Threading.Tasks.Task.Run(_settings.Load);
            if (_closed) return;
            _loading = false;
            IsEnabled = true;
            RefreshMonitors();
            if (warning != null) StatusText.Text = warning;
        };
        Closed += (_, _) =>
        {
            _closed = true;
            _displayTimer.Stop();
            _source?.RemoveHook(WindowMessage);
            foreach (var session in _sessions.Values) session.Dispose();
            _sessions.Clear();
            _engine?.Dispose();
        };
    }

    public void ExitApplication()
    {
        _exiting = true;
        Close();
    }

    public void StopAll()
    {
        foreach (var entry in _settings.Selections.ToArray())
            _settings.Selections[entry.Key] = entry.Value with { IsPlaying = false };
        foreach (var selection in _selections.Values)
        {
            StopSession(selection.Monitor.Id);
            _settings.Selections[selection.Monitor.Id] = new(selection.VideoPath, selection.Mode, false);
        }
        StatusText.Text = _settings.Save() ?? "Tüm monitörlerde oynatma durduruldu.";
    }

    private void Startup_Click(object sender, RoutedEventArgs e)
    {
        try { WindowsStartup.Apply(StartupCheckBox.IsChecked == true); }
        catch (Exception ex)
        {
            StartupCheckBox.IsChecked = !StartupCheckBox.IsChecked;
            StatusText.Text = $"Windows başlangıcı ayarlanamadı: {ex.Message}";
        }
    }

    private nint WindowMessage(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
    {
        // Display, device, DPI and system settings notifications may arrive in bursts.
        if (!_closed && !_loading && message is 0x007E or 0x0219 or 0x02E0 or 0x001A)
        {
            _displayTimer.Stop();
            _displayTimer.Interval = TimeSpan.FromMilliseconds(350);
            _displayTimer.Start();
        }
        return 0;
    }

    private void Start_Click(object sender, RoutedEventArgs e)
    {
        if (MonitorList.SelectedItem is not MonitorSelection selection || selection.VideoPath == null)
        {
            StatusText.Text = "Önce bir monitör ve video seçin.";
            return;
        }
        StartSelection(selection);
    }

    private void StartSelection(MonitorSelection selection)
    {
        var monitor = selection.Monitor;
        if (_sessions.Keys.Any(id => !StringComparer.OrdinalIgnoreCase.Equals(id, monitor.Id)
            && _selections[id].Monitor.DeviceName == monitor.DeviceName))
        {
            DiagnosticLog.Write($"start refused cloned source={monitor.DeviceName}");
            StatusText.Text = "Bağımsız videolar için Windows ekran düzenini Genişlet olarak seçin.";
            return;
        }
        try
        {
            if (_engine == null)
            {
                Core.Initialize();
                _engine = new LibVLC("--no-audio", "--no-video-title-show");
            }
            StopSession(monitor.Id);
            var session = new WallpaperPlayback(_engine);
            session.Status += message =>
            {
                StatusText.Text = $"{monitor.DisplayName} · {message}";
                if (session.IsDisposed) _sessions.Remove(monitor.Id);
            };
            session.Start(selection);
            _sessions.Add(monitor.Id, session);
            SaveSelection(selection, true);
            StatusText.Text = $"{monitor.DisplayName} · Video çıkışı bekleniyor…";
        }
        catch (Exception ex)
        {
            DiagnosticLog.Write($"start failed source={monitor.DeviceName} error={ex.GetBaseException().GetType().Name} code={ex.GetBaseException().HResult:X8}");
            StatusText.Text = $"Başlatılamadı: {ex.GetBaseException().Message}";
        }
    }

    private void StopSession(string id)
    {
        if (_sessions.Remove(id, out var session)) session.Dispose();
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        if (MonitorList.SelectedItem is not MonitorSelection selection) return;
        StopSession(selection.Monitor.Id);
        SaveSelection(selection, false);
        StatusText.Text = $"{selection.Monitor.DisplayName} · Oynatma durduruldu.";
    }

    private void Refresh_Click(object sender, RoutedEventArgs e) => RefreshMonitors();

    private void RefreshMonitors()
    {
        if (_refreshing || _closed || _loading) return;
        _refreshing = true;
        _displayTimer.Stop();
        try
        {
            UpdateMonitors(_monitorService.GetMonitors());
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Monitörler yenilenemedi: {ex.GetBaseException().Message}";
        }
        finally
        {
            _refreshing = false;
            if (!_closed && !_displayTimer.IsEnabled)
            {
                // A DPI change on another screen need not reach the management window.
                _displayTimer.Interval = ScanInterval;
                _displayTimer.Start();
            }
        }
    }

    private void UpdateMonitors(IReadOnlyList<MonitorInfo> monitors)
    {
        var byId = monitors.ToDictionary(m => m.Id, StringComparer.OrdinalIgnoreCase);
        var occupied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string? warning = null;
        foreach (var id in _sessions.Keys.ToArray())
        {
            if (!byId.TryGetValue(id, out var monitor) || !occupied.Add(monitor.DeviceName))
            {
                StopSession(id);
                continue;
            }
            try { _sessions[id].UpdateMonitor(monitor); }
            catch (InvalidOperationException ex)
            {
                StopSession(id);
                warning = $"{monitor.DisplayName} · {ex.Message}";
                DiagnosticLog.Write($"surface update failed source={monitor.DeviceName}");
            }
        }
        if (warning != null) StatusText.Text = warning;
        if (MonitorList.ItemsSource is List<MonitorSelection> current &&
            current.Select(s => s.Monitor).SequenceEqual(monitors)) return;

        string? selectedId = (MonitorList.SelectedItem as MonitorSelection)?.Monitor.Id;
        foreach (var id in _selections.Keys.Where(id => !byId.ContainsKey(id)).ToArray())
        {
            _selections[id].PropertyChanged -= SelectionChanged;
            _selections.Remove(id);
            _restoreAttempted.Remove(id);
        }
        var items = new List<MonitorSelection>();
        foreach (var monitor in monitors)
        {
            _selections.TryGetValue(monitor.Id, out var selection);
            if (selection?.Monitor != monitor)
            {
                if (selection != null) selection.PropertyChanged -= SelectionChanged;
                _settings.Selections.TryGetValue(monitor.Id, out var saved);
                selection = new MonitorSelection(monitor)
                {
                    VideoPath = selection?.VideoPath ?? saved?.VideoPath,
                    Mode = selection?.Mode ?? saved?.Mode ?? DisplayMode.Doldur
                };
                selection.PropertyChanged += SelectionChanged;
                _selections[monitor.Id] = selection;
            }
            items.Add(selection);
        }
        MonitorList.ItemsSource = items;
        MonitorList.SelectedItem = items.FirstOrDefault(s =>
            StringComparer.OrdinalIgnoreCase.Equals(s.Monitor.Id, selectedId)) ?? items.FirstOrDefault();
        int missing = items.Count(item => item.VideoPath != null && !File.Exists(item.VideoPath));
        StatusText.Text = warning ?? ($"{items.Count} monitör algılandı." +
            (missing > 0 ? $" {missing} video dosyası bulunamadı. Yeni dosya seçin." : ""));
        foreach (var item in items)
        {
            if (_restoreAttempted.Add(item.Monitor.Id) &&
                _settings.Selections.TryGetValue(item.Monitor.Id, out var saved) && saved.IsPlaying)
                StartSelection(item);
        }
    }

    private void SelectionChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not MonitorSelection selection) return;
        if (e.PropertyName == nameof(MonitorSelection.Mode) &&
            _sessions.TryGetValue(selection.Monitor.Id, out var session))
            session.ApplyMode(selection.Mode);
        SaveSelection(selection);
    }

    private void SaveSelection(MonitorSelection selection, bool? playing = null)
    {
        _settings.Selections.TryGetValue(selection.Monitor.Id, out var previous);
        _settings.Selections[selection.Monitor.Id] =
            new(selection.VideoPath, selection.Mode, playing ?? previous?.IsPlaying ?? false);
        string? error = _settings.Save();
        if (error != null) StatusText.Text = error;
    }

    private async void ChooseVideo_Click(object sender, RoutedEventArgs e)
    {
        if (MonitorList.SelectedItem is not MonitorSelection selection) return;
        var dialog = new OpenFileDialog
        {
            Title = "Monitör için video seçin",
            Filter = "Video dosyaları (*.mp4;*.webm)|*.mp4;*.webm",
            CheckFileExists = true,
            Multiselect = false
        };
        if (dialog.ShowDialog(this) != true) return;
        string extension = Path.GetExtension(dialog.FileName);
        if (!extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase) &&
            !extension.Equals(".webm", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, "MP4 veya WebM dosyası seçin.", "Desteklenmeyen dosya", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        try
        {
            IsEnabled = false;
            string path = await System.Threading.Tasks.Task.Run(() => _settings.ImportVideo(dialog.FileName));
            if (_closed) return;
            selection.VideoPath = path;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            StatusText.Text = $"Video eklenemedi: {ex.Message}";
        }
        finally { if (!_closed) IsEnabled = true; }
    }
}








