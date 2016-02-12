using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BobBuilder
{
    public partial class NotificationsForm : Form
    {
        public NotificationsForm()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 10, 10));
        }

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

        public string NotificationText
        {
            get { return lblNotification.Text; }
            set
            {
                if(value != lblNotification.Text)
                {
                    lblNotification.Text = value;
                }   
            }
        }

        public async void FadeOut()
        {
            await Task.Delay(2000);
            
            while (this.Opacity > 0.0)
            {
                await Task.Delay(100);
                this.Opacity -= 0.05;
            }
            this.Opacity = 1;
            this.Hide();
        }

        private void NotificationsForm_Load(object sender, EventArgs e)
        {
            this.Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 50 - this.Width;
            this.Top = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 100;
        }

        private void NotificationsForm_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void lblNotification_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", Form1.PATH);
            this.Hide();
        }
    }
}
