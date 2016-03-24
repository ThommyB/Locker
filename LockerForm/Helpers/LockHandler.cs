using Locker.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ST = System.Timers;
using System.Windows.Forms;

namespace Locker.Helpers
{
    public class LockHandler
    {
        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);

        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();

        #region Minimize all windows
        //http://stackoverflow.com/questions/785054/minimizing-all-open-windows-in-c-sharp
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);

        const int WM_COMMAND = 0x111;
        const int MIN_ALL = 419;
        const int MIN_ALL_UNDO = 416;
        #endregion

        #region Mouse Hooks
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private const int WH_MOUSE_LL = 14;
        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero; 
        #endregion

        private ST.Timer _timer;
        private static bool _locked = false;
        private bool _setingUpKey = false;
        private static int mouseX = 0;
        private int prevMouseX = -1;

        public event EventHandler FormShow;
        public event EventHandler FormHide;
        public event EventHandler Bind;
        public event EventHandler Unbind;

        public LockHandler()
        {
            _hookID = SetHook(_proc);

            _timer = new ST.Timer();
            _timer.Interval = 10;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;

            if (Settings.Default.Key == -1)
            {
                _setingUpKey = true;
                if (Bind != null)
                    Bind(this, null);
            }
            else
            {
                if (Unbind != null)
                    Unbind(this, null);
            }
        }

        private void Lock()
        {
            LockWorkStation();
            ToggleLock();
        }

        public void ToggleLock(bool locked = false)
        {
            prevMouseX = mouseX;

            if (!_locked || locked)
            {

                if (FormShow != null)
                    FormShow(this, null);
                ToggleMinimezeAll(true);
                _locked = true;
            }
            else
            {
                if (FormHide != null)
                    FormHide(this, null);
                ToggleMinimezeAll(false);
                _locked = false;
            }
        }

        private void ToggleMinimezeAll(bool minimized)
        {
            IntPtr lHwnd = FindWindow("Shell_TrayWnd", null);
            if (minimized)
                SendMessage(lHwnd, WM_COMMAND, (IntPtr)MIN_ALL, IntPtr.Zero);
            else
                SendMessage(lHwnd, WM_COMMAND, (IntPtr)MIN_ALL_UNDO, IntPtr.Zero);
        }

        private void BindKey(int key)
        {
            Settings.Default.Key = key;
            Settings.Default.Save();

            if (Bind != null)
                Bind(this, null);

            MessageBox.Show("Bound key is: " + ((Keys)key).ToString());
            _setingUpKey = false;
        }

        public void UnbindKey()
        {
            _setingUpKey = true;
            Settings.Default.Key = -1;

            if (Unbind != null)
                Unbind(this, null);
        }

        private void _timer_Elapsed(object sender, ST.ElapsedEventArgs e)
        {
            for (Int32 i = 0; i < 255; i++)
            {
                int keyState = GetAsyncKeyState(i);
                if (keyState == 1 || keyState == -32767)
                {
                    if (_setingUpKey && (Keys)i != Keys.LButton && (Keys)i != Keys.RButton)
                    {
                        BindKey(i);
                        return;
                    }

                    // Locker key pressed
                    if (i == Settings.Default.Key)
                    {
                        ToggleLock();
                    }
                    else
                    {
                        if (_locked)
                        {
                            Lock();
                        }
                    }
                    break;
                }
            }

            // Mouse moved
            if (_locked && prevMouseX != mouseX)
            {
                Lock();
            }
        }

        public void Unhook()
        {
            UnhookWindowsHookEx(_hookID);
        }

        #region Mouse hooks

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            mouseX = hookStruct.pt.x;

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        #endregion
    }
}
