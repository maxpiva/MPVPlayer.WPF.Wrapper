using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
// ReSharper disable InconsistentNaming
// ReSharper disable AssignNullToNotNullAttribute
#pragma warning disable 169

namespace NutzCode.MPVPlayer.WPF.Wrapper
{
    public class MPVInterop
    {

        private static MPVInterop _instance;
        public static MPVInterop Instance => _instance ?? (_instance = new MPVInterop());

        #region mpv interop

        private const int MpvFormatString = 1;
        private IntPtr _libMpvDll;
        private IntPtr _mpvHandle;

        [Flags]
        internal enum LoadLibraryFlags : uint
        {
            DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
            LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
            LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
            LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
            LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,
            LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008
        }

      
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct Mpv_Event_Log_Message
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string Prefix;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Level;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Text;
            public int LogLevel;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct Mpv_Event
        {
            public int EventId;
            public int Error;
            [MarshalAs(UnmanagedType.LPStruct)]
            public Mpv_Event_Log_Message Message;
        }

        [DllImport("kernel32.dll")]
        internal static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, LoadLibraryFlags dwFlags);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MpvCreate();
        private MpvCreate _mpvCreate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvInitialize(IntPtr mpvHandle);
        private MpvInitialize _mpvInitialize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvCommand(IntPtr mpvHandle, IntPtr strings);
        private MpvCommand _mpvCommand;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvTerminateDestroy(IntPtr mpvHandle);
        private MpvTerminateDestroy _mpvTerminateDestroy;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvSetOption(IntPtr mpvHandle, byte[] name, int format, ref long data);
        private MpvSetOption _mpvSetOption;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvSetOptionString(IntPtr mpvHandle, byte[] name, byte[] value);
        private MpvSetOptionString _mpvSetOptionString;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvGetPropertystring(IntPtr mpvHandle, byte[] name, int format, ref IntPtr data);
        private MpvGetPropertystring _mpvGetPropertyString;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvSetProperty(IntPtr mpvHandle, byte[] name, int format, ref byte[] data);
        private MpvSetProperty _mpvSetProperty;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void MpvFree(IntPtr data);
        private MpvFree _mpvFree;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvLoadConfigFile(IntPtr mpvHandle, byte[] filename);
        private MpvLoadConfigFile _mpvLoadConfigFile;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvRequestLogMessages(IntPtr mpvHandle, byte[] minlevel);
        private MpvRequestLogMessages _mpvRequestLogMessages;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvRequestEvent(IntPtr mpvHandle, int eventid, int enable);
        private MpvRequestEvent _mpvRequestEvent;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void MpvWaitEvent(IntPtr mpvHandle, double timeout);
        private MpvWaitEvent _mpvWaitEvent;
        

        private object GetDllType(Type type, string name)
        {
            IntPtr address = GetProcAddress(_libMpvDll, name);
            if (address != IntPtr.Zero)
                return Marshal.GetDelegateForFunctionPointer(address, type);
            return null;
        }

        internal void Initialize(Visual visual)
        {
            if (_mpvHandle != IntPtr.Zero)
                _mpvTerminateDestroy(_mpvHandle);

            string fullexepath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            FileInfo fi = new FileInfo(fullexepath);
            // ReSharper disable once PossibleNullReferenceException
            fullexepath = Path.Combine(fi.Directory.FullName, Environment.Is64BitProcess ? "x64" : "x86", "mpv-1.dll");
            _libMpvDll = LoadLibraryEx(fullexepath, IntPtr.Zero, 0);
            _mpvCreate = (MpvCreate)GetDllType(typeof(MpvCreate), "mpv_create");
            _mpvInitialize = (MpvInitialize)GetDllType(typeof(MpvInitialize), "mpv_initialize");
            _mpvTerminateDestroy = (MpvTerminateDestroy)GetDllType(typeof(MpvTerminateDestroy), "mpv_terminate_destroy");
            _mpvCommand = (MpvCommand)GetDllType(typeof(MpvCommand), "mpv_command");
            _mpvSetOption = (MpvSetOption)GetDllType(typeof(MpvSetOption), "mpv_set_option");
            _mpvSetOptionString = (MpvSetOptionString)GetDllType(typeof(MpvSetOptionString), "mpv_set_option_string");
            _mpvGetPropertyString = (MpvGetPropertystring)GetDllType(typeof(MpvGetPropertystring), "mpv_get_property");
            _mpvSetProperty = (MpvSetProperty)GetDllType(typeof(MpvSetProperty), "mpv_set_property");
            _mpvFree = (MpvFree)GetDllType(typeof(MpvFree), "mpv_free");
            /*
            _mpvLoadConfigFile = (MpvLoadConfigFile)GetDllType(typeof(MpvLoadConfigFile), "mpv_load_config_file");
            _mpvRequestLogMessages = (MpvRequestLogMessages) GetDllType(typeof(MpvRequestLogMessages), "mpv_request_log_messages");
            _mpvRequestEvent = (MpvRequestEvent)GetDllType(typeof(MpvRequestEvent), "mpv_request_event");
            _mpvWaitEvent = (MpvWaitEvent)GetDllType(typeof(MpvWaitEvent), "mpv_wait_event");
            */
            if (_libMpvDll == IntPtr.Zero)
                return;

            _mpvHandle = _mpvCreate.Invoke();
            if (_mpvHandle == IntPtr.Zero)
                return;

            _mpvInitialize.Invoke(_mpvHandle);
            SetWindowsHandle(visual);
        }
        private static byte[] GetUtf8Bytes(string s)
        {
            return Encoding.UTF8.GetBytes(s + "\0");
        }
        public void SetProperty(string property, string value)
        {
            if (_mpvHandle == IntPtr.Zero)
                return;
            var bytes = GetUtf8Bytes(value);
            _mpvSetProperty(_mpvHandle, GetUtf8Bytes(property), MpvFormatString, ref bytes);

        }
        public string GetProperty(string property)
        {
            if (_mpvHandle == IntPtr.Zero)
                return null;

            var lpBuffer = IntPtr.Zero;
            _mpvGetPropertyString(_mpvHandle, GetUtf8Bytes(property), MpvFormatString, ref lpBuffer);
            string ret = Marshal.PtrToStringAnsi(lpBuffer);
            _mpvFree(lpBuffer);
            return ret;
        }

        public int GetIntProperty(string property)
        {
            string r = GetProperty(property);
            int result;
            if (int.TryParse(r, out result))
                return result;
            return 0;
        }
        public double GetDoubleProperty(string property)
        {
            string r = GetProperty(property);
            double result;
            if (double.TryParse(r, out result))
                return result;
            return 0;
        }
        private static IntPtr AllocateUtf8IntPtrArrayWithSentinel(string[] arr, out IntPtr[] byteArrayPointers)
        {
            int numberOfStrings = arr.Length + 1; // add extra element for extra null pointer last (sentinel)
            byteArrayPointers = new IntPtr[numberOfStrings];
            IntPtr rootPointer = Marshal.AllocCoTaskMem(IntPtr.Size * numberOfStrings);
            for (int index = 0; index < arr.Length; index++)
            {
                var bytes = GetUtf8Bytes(arr[index]);
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);
                byteArrayPointers[index] = unmanagedPointer;
            }
            Marshal.Copy(byteArrayPointers, 0, rootPointer, numberOfStrings);
            return rootPointer;
        }
        private void SetWindowsHandle(Visual visual)
        {
            HwndSource source = PresentationSource.FromVisual(visual) as HwndSource;
            if (source != null)
            {
                IntPtr handle = source.Handle;
                _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("keep-open"), GetUtf8Bytes("always"));
                int mpvFormatInt64 = 4;
                var windowId = handle.ToInt64();
                _mpvSetOption(_mpvHandle, GetUtf8Bytes("wid"), mpvFormatInt64, ref windowId);
            }


        }
        public void DoMpvCommand(params string[] args)
        {
            if (_mpvHandle == IntPtr.Zero)
                return;
            IntPtr[] byteArrayPointers;
            var mainPtr = AllocateUtf8IntPtrArrayWithSentinel(args, out byteArrayPointers);
            _mpvCommand(_mpvHandle, mainPtr);
            foreach (var ptr in byteArrayPointers)
            {
                Marshal.FreeHGlobal(ptr);
            }
            Marshal.FreeHGlobal(mainPtr);
        }

        internal void MPVPlay()
        {
            if (_mpvHandle == IntPtr.Zero)
                return;
            var bytes = GetUtf8Bytes("no");
            _mpvSetProperty(_mpvHandle, GetUtf8Bytes("pause"), MpvFormatString, ref bytes);
        }
        internal void MPVPause()
        {
            if (_mpvHandle == IntPtr.Zero)
                return;
            var bytes = GetUtf8Bytes("yes");
            _mpvSetProperty(_mpvHandle, GetUtf8Bytes("pause"), MpvFormatString, ref bytes);
        }

        internal void Finish()
        {
            if (_mpvHandle != IntPtr.Zero)
                _mpvTerminateDestroy(_mpvHandle);
            _mpvHandle = IntPtr.Zero;
        }
        #endregion


    }
}
