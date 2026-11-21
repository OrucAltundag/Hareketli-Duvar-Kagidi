using System.ComponentModel;
using System.Runtime.InteropServices;
using HareketliDuvarKagidi.Models;

namespace HareketliDuvarKagidi.Services;

public sealed class MonitorService
{
    public IReadOnlyList<MonitorInfo> GetMonitors()
    {
        var monitors = new List<MonitorInfo>();
        Exception? failure = null;
        MonitorEnum callback = (nint handle, nint dc, ref NativeRect rect, nint data) =>
        {
            // Never allow a managed exception to cross the native callback boundary.
            try
            {
                var info = new NativeMonitorInfo { Size = Marshal.SizeOf<NativeMonitorInfo>() };
                if (!GetMonitorInfo(handle, ref info))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                var devices = new List<NativeDisplayDevice>();
                for (uint index = 0; ; index++)
                {
                    var candidate = new NativeDisplayDevice { Size = Marshal.SizeOf<NativeDisplayDevice>() };
                    if (!EnumDisplayDevices(info.DeviceName, index, ref candidate, 1)) break;
                    if ((candidate.StateFlags & 1) != 0 && !string.IsNullOrWhiteSpace(candidate.DeviceId))
                    {
                        devices.Add(candidate);
                    }
                }

                if (devices.Count == 0)
                    throw new InvalidOperationException($"{info.DeviceName}: monitör aygıt kimliği okunamadı.");

                var dpiResult = GetDpiForMonitor(handle, 0, out uint dpiX, out uint dpiY);
                // A logical desktop monitor can expose multiple mirrored device targets.
                foreach (var monitor in devices)
                {
                    monitors.Add(new MonitorInfo(monitor.DeviceId, monitor.DeviceString, info.DeviceName,
                        info.Monitor.Right - info.Monitor.Left, info.Monitor.Bottom - info.Monitor.Top,
                        info.Monitor.Left, info.Monitor.Top,
                        dpiResult == 0 ? dpiX : null, dpiResult == 0 ? dpiY : null,
                        (info.Flags & 1) != 0));
                }
                return true;
            }
            catch (Exception ex) { failure = ex; return false; }
        };

        bool success = EnumDisplayMonitors(0, 0, callback, 0);
        if (failure is not null) throw new InvalidOperationException("Monitör bilgileri alınamadı.", failure);
        if (!success) throw new Win32Exception(Marshal.GetLastWin32Error());
        return monitors.OrderByDescending(m => m.IsPrimary).ThenBy(m => m.X).ThenBy(m => m.Y).ToArray();
    }

    private delegate bool MonitorEnum(nint monitor, nint dc, ref NativeRect rect, nint data);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { public int Left, Top, Right, Bottom; }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NativeMonitorInfo
    {
        public int Size;
        public NativeRect Monitor, Work;
        public uint Flags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string DeviceName;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NativeDisplayDevice
    {
        public int Size;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceString;
        public uint StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceKey;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplayMonitors(nint dc, nint clip, MonitorEnum callback, nint data);

    [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(nint monitor, ref NativeMonitorInfo info);

    [DllImport("user32.dll", EntryPoint = "EnumDisplayDevicesW", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplayDevices(string device, uint index, ref NativeDisplayDevice info, uint flags);

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(nint monitor, int dpiType, out uint dpiX, out uint dpiY);
}
