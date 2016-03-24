using Locker.Helpers;
using Locker.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Media;
using System.Windows.Forms;

namespace Locker
{
    public partial class MainForm : Form
    {
        LockHandler _lockHandler;
        public static Queue<Notifications> Notifications { get; set; }

        public MainForm()
        {
            _lockHandler = new LockHandler();
            _lockHandler.Bind += lockHandler_Bind;
            _lockHandler.Unbind += lockHandler_Unbind;
            _lockHandler.FormShow += lockHandler_FormShow;
            _lockHandler.FormHide += lockHandler_FormHide;

            // Transparent background
            this.TransparencyKey = Color.White;
            this.BackColor = Color.White;

            InitializeComponent();
        }

        #region Private Methods

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

        private void DeviceEvent(bool usbIn, string description)
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
        }

        #endregion


        #region Event Handlers

        private void lockHandler_FormHide(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, EventArgs>(lockHandler_FormHide), sender, e);
                return;
            }
            Hide();
            notifyIcon.Icon = new Icon(Application.StartupPath + @"\Resources\unlocked.ico");
        }

        private void lockHandler_FormShow(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, EventArgs>(lockHandler_FormShow), sender, e);
                return;
            }
            Show();
            notifyIcon.Icon = new Icon(Application.StartupPath + @"\Resources\locked.ico");
        }

        private void lockHandler_Unbind(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, EventArgs>(lockHandler_Unbind), sender, e);
                return;
            }
            Show();
            lblBindKey.Visible = true;
        }

        private void lockHandler_Bind(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, EventArgs>(lockHandler_Bind), sender, e);
                return;
            }
            Hide();
            lblBindKey.Visible = false;
        }

        private void Remote_CommandReceived(object sender, CommandReceivedEventArgs e)
        {
            if (Settings.Default.IgnoredDevices != null && Settings.Default.IgnoredDevices.Contains(e.Fingerprint))
            {
                return;
            }
            else if (Settings.Default.AuthorizedDevices == null || !Settings.Default.AuthorizedDevices.Contains(e.Fingerprint))
            {
                var confirmResult = MessageBox.Show("Authorize this device?", "Confirm", MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.No)
                {
                    if (Settings.Default.IgnoredDevices == null)
                        Settings.Default.IgnoredDevices = new System.Collections.Specialized.StringCollection();

                    Settings.Default.IgnoredDevices.Add(e.Fingerprint);
                    Settings.Default.Save();
                    return;
                }
                else
                {
                    Settings.Default.AuthorizedDevices = new System.Collections.Specialized.StringCollection();
                    Settings.Default.AuthorizedDevices.Add(e.Fingerprint);
                    Settings.Default.Save();
                }
            }

            if (e.Enabled)
            {
                _lockHandler.ToggleLock(true);
            }
            else
                _lockHandler.ToggleLock(false);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Notifications = new Queue<Notifications>();

            
            RemoteController remote = new RemoteController();
            remote.CommandReceived += Remote_CommandReceived;

            UsbWatcher usbWatcher = new UsbWatcher();
            usbWatcher.DeviceInserterd += usbWatcher_DeviceInserterd;
            usbWatcher.DeviceRemoved += usbWatcher_DeviceRemoved;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (Settings.Default.Key != -1)
            {
                Hide();
            }
        }

        void usbWatcher_DeviceRemoved(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            DeviceEvent(false, instance.Properties["Name"].Value.ToString());
        }

        void usbWatcher_DeviceInserterd(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            DeviceEvent(true, instance.Properties["Name"].Value.ToString());
        }

        private void unbindKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _lockHandler.UnbindKey();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void clearDeviceListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.AuthorizedDevices = null;
            Settings.Default.IgnoredDevices = null;
            Settings.Default.Save();
        }
       
        #endregion

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _lockHandler.Unhook();
        }
    }
}
