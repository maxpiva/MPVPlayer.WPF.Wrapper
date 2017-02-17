using System;
using System.Runtime.InteropServices;
using HwndExtensions.Host;
// ReSharper disable InconsistentNaming

namespace NutzCode.MPVPlayer.WPF.Wrapper
{
    public class NativePlayer : ExtendedHwndHost
    {

        IntPtr hwndHost;

        internal const int
            WS_CHILD = 0x40000000,
            WS_VISIBLE = 0x10000000,
            LBS_NOTIFY = 0x00000001,
            HOST_ID = 0x00000002,
            LISTBOX_ID = 0x00000001,
            WS_VSCROLL = 0x00200000,
            WS_BORDER = 0x00800000;
           
        [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateWindowEx(int dwExStyle,
                                                 string lpszClassName,
                                                 string lpszWindowName,
                                                 int style,
                                                 int x, int y,
                                                 int width, int height,
                                                 IntPtr hwndParent,
                                                 IntPtr hMenu,
                                                 IntPtr hInst,
                                                 [MarshalAs(UnmanagedType.AsAny)] object pvParam);

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
        internal static extern bool DestroyWindow(IntPtr hwnd);

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            hwndHost = IntPtr.Zero;
            var width = Convert.ToInt32(ActualWidth);
            var height = Convert.ToInt32(ActualHeight);
            hwndHost = CreateWindowEx(0, "static", "",
                                      WS_CHILD | WS_VISIBLE,
                                      0, 0,
                                      width, height,
                                      hwndParent.Handle,
                                      (IntPtr)HOST_ID,
                                      IntPtr.Zero,
                                      0);

 
            return new HandleRef(this, hwndHost);
        }



        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            DestroyWindow(hwnd.Handle);
        }
    }
}
