using System.Windows;
using System.Windows.Interop;
using System.Security.Principal;
using System.Threading;
using Forms = System.Windows.Forms;

namespace HareketliDuvarKagidi;

public partial class App : Application
{
    private Mutex? _mutex;
    private bool _ownsMutex;
    private EventWaitHandle? _openEvent;
    private RegisteredWaitHandle? _openWait;
    private EventWaitHandle? _exitEvent;
    private RegisteredWaitHandle? _exitWait;
    private Forms.NotifyIcon? _tray;
    private Forms.ContextMenuStrip? _menu;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        string name = @"Local\HareketliDuvarKagidi-" + WindowsIdentity.GetCurrent().User!.Value;
        _mutex = new Mutex(false, name);
        try { _ownsMutex = _mutex.WaitOne(0); }
        catch (AbandonedMutexException) { _ownsMutex = true; }
        if (e.Args.Contains("--shutdown", StringComparer.OrdinalIgnoreCase))
        {
            if (!_ownsMutex)
            {
                try
                {
                    using var signal = EventWaitHandle.OpenExisting(name + "-Exit");
                    signal.Set();
                    try { _ownsMutex = _mutex.WaitOne(TimeSpan.FromSeconds(30)); }
                    catch (AbandonedMutexException) { _ownsMutex = true; }
                }
                catch (WaitHandleCannotBeOpenedException) { }
            }
            Shutdown(_ownsMutex ? 0 : 1);
            return;
        }
        if (!_ownsMutex)
        {
            try { using var signal = EventWaitHandle.OpenExisting(name + "-Open"); signal.Set(); }
            catch (WaitHandleCannotBeOpenedException) { }
            Shutdown();
            return;
        }
        var window = new MainWindow();
        MainWindow = window;
        _menu = new Forms.ContextMenuStrip();
        _menu.Items.Add("Aç", null, (_, _) => OpenWindow());
        _menu.Items.Add("Tümünü durdur", null, (_, _) => window.StopAll());
        _menu.Items.Add("Çıkış", null, (_, _) => ExitApplication());
        _tray = new Forms.NotifyIcon
        {
            Text = "Hareketli Duvar Kâğıdı",
            Icon = System.Drawing.SystemIcons.Application,
            ContextMenuStrip = _menu,
            Visible = true
        };
        _tray.DoubleClick += (_, _) => OpenWindow();
        Services.DiagnosticLog.Write($"startup version={typeof(App).Assembly.GetName().Version} tray={_tray.Visible}");
        _openEvent = new EventWaitHandle(false, EventResetMode.AutoReset, name + "-Open");
        _openWait = ThreadPool.RegisterWaitForSingleObject(_openEvent,
            (_, _) => Dispatcher.BeginInvoke(OpenWindow), null, Timeout.Infinite, false);
        _exitEvent = new EventWaitHandle(false, EventResetMode.AutoReset, name + "-Exit");
        _exitWait = ThreadPool.RegisterWaitForSingleObject(_exitEvent,
            (_, _) => Dispatcher.BeginInvoke(ExitApplication), null, Timeout.Infinite, false);
        if (e.Args.Contains("--background", StringComparer.OrdinalIgnoreCase))
            new WindowInteropHelper(window).EnsureHandle();
        else window.Show();
    }

    private void ExitApplication()
    {
        if (Dispatcher.HasShutdownStarted) return;
        ((MainWindow?)MainWindow)?.ExitApplication();
        Shutdown();
    }

    private void OpenWindow()
    {
        if (Dispatcher.HasShutdownStarted || MainWindow == null) return;
        MainWindow.Show();
        MainWindow.WindowState = WindowState.Normal;
        MainWindow.Activate();
        Services.DiagnosticLog.Write("window opened");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_tray != null) Services.DiagnosticLog.Write($"exit code={e.ApplicationExitCode}");
        _exitWait?.Unregister(null);
        _exitEvent?.Dispose();
        _openWait?.Unregister(null);
        _openEvent?.Dispose();
        if (_tray != null) { _tray.Visible = false; _tray.Dispose(); }
        _menu?.Dispose();
        if (_ownsMutex) _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}

