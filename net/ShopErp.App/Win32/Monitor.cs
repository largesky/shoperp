using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Management;
using System.Diagnostics;
using Microsoft.Win32;

namespace ShopErp.App.Win32
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    class MONITORINFOEX
    {
        public uint cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFOEX));
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDevice = "".PadRight(32, '\0');
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    class DISPLAY_DEVICE
    {
        public int cbSize = Marshal.SizeOf(typeof(DISPLAY_DEVICE));

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName = "".PadLeft(32, '\0');

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString = "".PadLeft(128, '\0');

        public int StateFlags = 0;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID = "".PadLeft(128, '\0');

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey = "".PadLeft(128, '\0');
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    class POINT
    {
        public Int32 x;
        public Int32 y;
    }

    class Monitor
    {
        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int flag);

        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true,
            EntryPoint = "GetMonitorInfoW", CharSet = CharSet.Unicode)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, [In, Out,] MONITORINFOEX ex);

        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true,
            EntryPoint = "EnumDisplayDevicesW", CharSet = CharSet.Unicode)]
        private static extern bool EnumDisplayDevices(string lpDevice, int iDevNum,
            [In, Out] DISPLAY_DEVICE lpDisplayDevice, int dwFlags);

        /// <summary>
        /// 获取UI元素所有的显示器
        /// </summary>
        /// <param name="fe"></param>
        /// <returns></returns>
        private static IntPtr GetFrameworkMonitor(FrameworkElement fe)
        {
            Window w = null;
            FrameworkElement c = fe;
            while (c.Parent != null && c.Parent.GetType().BaseType != typeof(Window))
            {
                c = c.Parent as FrameworkElement;
            }
            w = c.Parent as Window;

            if (w == null)
            {
                throw new Exception("该元素没有顶层Window对象");
            }
            var handle = new WindowInteropHelper(w).Handle;
            return MonitorFromWindow(handle, 0);
        }

        public static bool GetMonitorInfo(FrameworkElement fe, ref int physicalWidth, ref int physicalHeight,
            ref int pixelWidth, ref int pixelHeight)
        {
            //获取所在显示器
            IntPtr hMonitor = GetFrameworkMonitor(fe);
            if (hMonitor == IntPtr.Zero || hMonitor == new IntPtr(-1))
            {
                return false;
            }
            //获取显示器所在的设备名称
            MONITORINFOEX ex = new MONITORINFOEX();
            bool ret = GetMonitorInfo(hMonitor, ex);
            if (ret == false)
            {
                int err = Marshal.GetLastWin32Error();
                return false;
            }
            pixelWidth = ex.rcMonitor.right - ex.rcMonitor.left;
            pixelHeight = ex.rcMonitor.bottom - ex.rcMonitor.top;
            string monitorName = ex.szDevice;
            //获取设备所在的驱动
            DISPLAY_DEVICE monitor = new DISPLAY_DEVICE();
            ret = EnumDisplayDevices(ex.szDevice, 0, monitor, 0);
            if (ret == false)
            {
                int err = Marshal.GetLastWin32Error();
                return false;
            }
            string deviceID = monitor.DeviceID;
            string deviceType = deviceID.Split('\\')[1].Trim();
            string monitorID = deviceID.Split('\\')[2] + '\\' + deviceID.Split('\\')[3];
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\DISPLAY\" + deviceType, RegistryKeyPermissionCheck.ReadSubTree))
            {
                //列出下面所有设备
                foreach (var reg in key.GetSubKeyNames())
                {
                    using (RegistryKey monitorKey = key.OpenSubKey(reg, RegistryKeyPermissionCheck.ReadSubTree))
                    {
                        if (monitorKey.GetValue("Driver").ToString().Equals(monitorID))
                        {
                            using (RegistryKey pKey = monitorKey.OpenSubKey("Device Parameters"))
                            {
                                //找到该显示器,读取edid
                                byte[] edid = pKey.GetValue("EDID") as byte[];
                                if (edid == null || edid.Length < 22)
                                {
                                    return false;
                                }
                                physicalWidth = edid[21];
                                physicalHeight = edid[22];
                                return true;
                            }
                        }
                    }
                }
            }
            return ret;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos([In, Out] POINT point);
    }
}