﻿using Locker;
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

namespace LockerForm
{
    public partial class Form1 : Form
    {
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


        private static bool _locked = false;
        private bool _setingUpKey = false;
        private static int mouseX = 0;
        private int prevMouseX = -1;
        private Form _f2;


        public Form1()
        {
            _hookID = SetHook(_proc);

            this.TransparencyKey = Color.White;
            this.BackColor = Color.White;

            InitializeComponent();
        }


        #region Private Methods

        private void Lock()
        {
            LockWorkStation();
            SwitchLock();
        }

        private void SwitchLock()
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

        /* Mouse hooks */

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
  

        #region Event Handlers

        private void Form1_Load(object sender, EventArgs e)
        {
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

            bw.RunWorkerAsync();
            _f2 = new Form2();
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
                        SwitchLock();
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

        #endregion

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
            foreach (var property in instance.Properties)
            {
                Console.WriteLine(property.Name + " = " + property.Value);
            }

            SystemSounds.Exclamation.Play();
            File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "log.txt", "[" + DateTime.Now.ToString("u") + "] " + "USB DEVICE - IN" + Environment.NewLine);
            MessageBox.Show("Intruder!!!");
        }

        void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            foreach (var property in instance.Properties)
            {
                Console.WriteLine(property.Name + " = " + property.Value);
            }

            File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "log.txt", "[" + DateTime.Now.ToString("u") + "] " + "USB DEVICE - OUT" + Environment.NewLine);
        }     
    }
}
