using System.ComponentModel;
using System.Runtime.InteropServices;
using HareketliDuvarKagidi.Models;

namespace HareketliDuvarKagidi.Services;

internal sealed class DesktopSurface : IDisposable
{
    public nint Handle { get; private set; }
    private readonly nint _parent;
    private readonly nint _icons;
    public bool IsAttached => Handle != 0 && IsWindowVisible(Handle) &&
        GetParent(Handle) == _parent && IsWindowVisible(_icons);

    public DesktopSurface(MonitorInfo monitor)
    {
        var progman = FindWindow("Progman", null);
        if (progman == 0 || SendMessageTimeout(progman, 0x052C, 0xD, 1, 2, 1000, out _) == 0)
            throw new InvalidOperationException("Explorer masaüstü katmanı hazırlanamadı.");

        nint parent = 0, icons = 0, insertAfter = 0;
        EnumWindows((window, _) =>
        {
            var view = FindWindowEx(window, 0, "SHELLDLL_DefView", null);
            if (view == 0 || !IsWindowVisible(view)) return true;
            var background = FindWindowEx(window, 0, "WorkerW", null);
            if (window == progman && background != 0)
            {
                // Raised desktop: create a sibling between the icons and background.
                parent = window;
                insertAfter = view;
            }
            else
            {
                parent = FindWindowEx(0, window, "WorkerW", null);
            }
            icons = view;
            return parent == 0;
        }, 0);
        if (parent == 0 || !IsWindowVisible(parent))
            throw new InvalidOperationException("Simgelerin arkasında görünür bir masaüstü katmanı bulunamadı.");

        var point = new Point { X = monitor.X, Y = monitor.Y };
        if (!ScreenToClient(parent, ref point)) throw new Win32Exception();
        _parent = parent;
        _icons = icons;
        Handle = CreateWindowEx(0x08000000, "STATIC", "", 0x40000000 | 0x02000000,
            point.X, point.Y, monitor.Width, monitor.Height, parent, 0, 0, 0);
        if (Handle == 0) throw new Win32Exception();
        if (!SetWindowPos(Handle, insertAfter, point.X, point.Y, monitor.Width, monitor.Height, 0x0050)
            || !IsAttached || !MatchesBounds(monitor))
        {
            Dispose();
            throw new InvalidOperationException("Video yüzeyinin masaüstü bağlantısı veya konumu doğrulanamadı.");
        }
    }

    public void UpdateBounds(MonitorInfo monitor)
    {
        if (!IsAttached) throw new InvalidOperationException("Masaüstü bağlantısı kayboldu.");
        if (MatchesBounds(monitor)) return;
        var point = new Point { X = monitor.X, Y = monitor.Y };
        if (!ScreenToClient(_parent, ref point) ||
            !SetWindowPos(Handle, 0, point.X, point.Y, monitor.Width, monitor.Height, 0x0014) ||
            !MatchesBounds(monitor))
            throw new InvalidOperationException("Video yüzeyi yeni ekran sınırlarına yerleştirilemedi.");
    }

    private bool MatchesBounds(MonitorInfo monitor) =>
        GetWindowRect(Handle, out var bounds) && bounds.Left == monitor.X && bounds.Top == monitor.Y &&
        bounds.Right - bounds.Left == monitor.Width && bounds.Bottom - bounds.Top == monitor.Height;

    public void Dispose()
    {
        if (Handle != 0) DestroyWindow(Handle);
        Handle = 0;
    }

    [StructLayout(LayoutKind.Sequential)] private struct Point { public int X, Y; }
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    private delegate bool EnumProc(nint hwnd, nint data);
    [DllImport("user32")] private static extern bool EnumWindows(EnumProc callback, nint data);
    [DllImport("user32", CharSet = CharSet.Unicode)] private static extern nint FindWindow(string cls, string? title);
    [DllImport("user32", CharSet = CharSet.Unicode)] private static extern nint FindWindowEx(nint parent, nint after, string cls, string? title);
    [DllImport("user32")] private static extern nint SendMessageTimeout(nint hwnd, uint msg, nuint w, nint l, uint flags, uint timeout, out nuint result);
    [DllImport("user32", SetLastError = true)] private static extern bool ScreenToClient(nint hwnd, ref Point point);
    [DllImport("user32", CharSet = CharSet.Unicode, SetLastError = true)] private static extern nint CreateWindowEx(uint ex, string cls, string title, uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint param);
    [DllImport("user32")] private static extern bool SetWindowPos(nint hwnd, nint after, int x, int y, int width, int height, uint flags);
    [DllImport("user32")] private static extern nint GetParent(nint hwnd);
    [DllImport("user32")] private static extern bool GetWindowRect(nint hwnd, out Rect rect);
    [DllImport("user32")] private static extern bool IsWindowVisible(nint hwnd);
    [DllImport("user32")] private static extern bool DestroyWindow(nint hwnd);
}

