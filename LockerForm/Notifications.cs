using Locker;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Locker
{
    public partial class Notifications : Form
    {
        private readonly int BORDER_RADIUS = 10;
        
        public int Y { get; set; }

        public enum Images
        {
            usb_in,
            usb_out,
            neutral
        };

        #region InteropServices
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect, // x-coordinate of upper-left corner
            int nTopRect, // y-coordinate of upper-left corner
            int nRightRect, // x-coordinate of lower-right corner
            int nBottomRect, // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
         );
        #endregion

        public Notifications(int br = 0)
        {
            InitializeComponent();

            // Setup border radius
            BORDER_RADIUS = br != 0 ? br : BORDER_RADIUS;
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, BORDER_RADIUS, BORDER_RADIUS));
        }

        /// <summary>
        /// Shows notification form
        /// </summary>
        /// <param name="text"></param>
        /// <param name="image"></param>
        /// <param name="y"></param>
        /// <param name="description"></param>
        /// <param name="fadeAfter">time in mils afte the form disapears</param>
        public void Show(string text, Images image, int y, string description = "", int fadeAfter = 10000)
        {
            this.Show();

            lblNotification.Text = text;
            toolTip.SetToolTip(this, description);

            SetPosition(y);
            SetImage(image);

            FadeOut(fadeAfter);
        }

        /// <summary>
        /// Fade out and close form after delay
        /// </summary>
        /// <param name="dealy">miliseconds</param>
        public async void FadeOut(int dealy = 2000)
        {
            await Task.Delay(dealy);

            while (this.Opacity > 0.0)
            {
                await Task.Delay(100);
                this.Opacity -= 0.05;
            }
            this.Opacity = 1;
            this.Close();
        }

        /// <summary>
        /// Set form position
        /// </summary>
        /// <param name="y">distance in pixels from bottom of the screen</param>
        private void SetPosition(int y)
        {
            Y = y;
            this.Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - this.Width;
            this.Top = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - y;
        }

        private void SetImage(Images image)
        {
            switch (image)
            {
                case Images.usb_in:
                    picNotificationImage.Image = Properties.Resources.usb_in;
                    break;

                case Images.usb_out:
                    picNotificationImage.Image = Properties.Resources.usb_out;
                    break;

                default:
                    picNotificationImage.Image = Properties.Resources.usb;
                    break;
            }
        }

        private void Notifications_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form1.Notifications.Dequeue();

            // Move down other notifications
            foreach (var n in Form1.Notifications)
            {
                n.Y = n.Y - Height;
                n.Top = n.Top + Height;
            } 
        }
    }
}
