using Locker;
using Locker.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Media;

namespace Locker
{
    public partial class Form1 : Form
    {
        #region Imports

        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);

        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();

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

        private static bool _locked = false;
        private bool _setingUpKey = false;
        private static int mouseX = 0;
        private int prevMouseX = -1;

        public static Queue<Notifications> Notifications { get; set; }

        public Form1()
        {
            _hookID = SetHook(_proc);

            // Transparent background
            this.TransparencyKey = Color.White;
            this.BackColor = Color.White;

            InitializeComponent();
        }


        #region Private Methods

        private void Lock()
        {
            LockWorkStation();
            ToggleLock();
        }

        private void ToggleLock()
        {
            if (_locked == false)
            {
                Show();
                notifyIcon.Icon = new System.Drawing.Icon(Application.StartupPath + @"\locked.ico");
                _locked = true;
            }
            else
            {
                Hide();
                notifyIcon.Icon = new System.Drawing.Icon(Application.StartupPath + @"\unlocked.ico");
                _locked = false;
            }
        }

        private void BindKey(int key)
        {
            Settings.Default.Key = key;
            Settings.Default.Save();

            lblBindKey.Visible = false;
            Hide();

            MessageBox.Show("Bound key is: " + ((Keys)key).ToString());
            _setingUpKey = false;
        }

        private void UnbindKey()
        {
            _setingUpKey = true;
            Settings.Default.Key = -1;
            lblBindKey.Visible = true;

            Show();
        }

        private void ShowNotification(string text, Locker.Notifications.Images image, string description = "")
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, Locker.Notifications.Images, string>(ShowNotification), text, image, description);
                return;
            }

            Notifications notify = new Notifications();
            int y = Notifications.Count > 0 ? Notifications.Last().Y + Notifications.Last().Height : 100;
            Notifications.Enqueue(notify);
            notify.Show(text, image, y, description);
        }

        private void DeviceEvent(bool usbIn, string description = "")
        {
            string eventText = "";
            if (usbIn)
            {
                eventText = "USB IN";
                ShowNotification(eventText, Locker.Notifications.Images.usb_in, description);
                SystemSounds.Exclamation.Play();
            }
            else
            {
                eventText = "USB OUT";
                ShowNotification(eventText, Locker.Notifications.Images.usb_out, description);
            }

            File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "log.txt",
                string.Format("[{0}] {1} | {2}{3}", DateTime.Now.ToString("u"), eventText, Environment.NewLine, description));
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

        #endregion


        #region Event Handlers

        private void Form1_Load(object sender, EventArgs e)
        {
            // if there is no bound key, let user set one
            if (Settings.Default.Key == -1)
            {
                _setingUpKey = true;
                lblBindKey.Visible = true;
            }
            else
            {
                lblBindKey.Visible = false;
                Hide();
            }

            Notifications = new Queue<Notifications>();

            bw.RunWorkerAsync();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (Settings.Default.Key != -1)
            {
                Hide();
            }
        }

        private void timer_Tick(object sender, EventArgs e)
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
                        prevMouseX = mouseX;
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

        private void unbindKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UnbindKey();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
        }

        /// <summary>
        /// Setup usb watcher
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");

            ManagementEventWatcher insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
            insertWatcher.Start();

            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            ManagementEventWatcher removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            removeWatcher.Start();

            Thread.Sleep(20000000);
        }

        private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            
            DeviceEvent(true, instance?.Properties["Name"].Value.ToString());
        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];

            DeviceEvent(false, instance?.Properties["Name"].Value.ToString());
        }     
        
        #endregion

    }
}
