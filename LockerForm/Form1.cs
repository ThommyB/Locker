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

        private static bool _locked = false;
        private bool _setingUpKey = false;
        private static int mouseX = 0;
        private int prevMouseX = -1;

        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;



        public Form1()
        {
            _hookID = SetHook(_proc);

            this.TransparencyKey = Color.White;
            this.BackColor = Color.White;

            InitializeComponent();
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
                        Settings.Default.Key = i;                      
                        Settings.Default.Save();

                        lblBindKey.Visible = false;
                        Hide();

                        MessageBox.Show("Bound key is: " + ((Keys)i).ToString());
                        _setingUpKey = false;

                        return;
                    }

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

            if (_locked && prevMouseX != mouseX)
            {
                Lock();
            }

            
        }

        private void Lock()
        {
            LockWorkStation();
            SwitchLock();
        }

        private void SwitchLock()
        {
            if (_locked == false)
            {
                Console.WriteLine("Locked");
                Show();
                notifyIcon.Icon = new System.Drawing.Icon(Application.StartupPath + @"\locked.ico");
                _locked = true;
            }
            else
            {
                Console.WriteLine("Unlocked");
                Hide();
                notifyIcon.Icon = new System.Drawing.Icon(Application.StartupPath + @"\unlocked.ico");
                _locked = false;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (Settings.Default.Key != -1)
            {
                Hide();
            }
            
        }

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
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

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

        private void unbindKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _setingUpKey = true;
            Settings.Default.Key = -1;
            lblBindKey.Visible = true;
            Show();
        }


    }
}
