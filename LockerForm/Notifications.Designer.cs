namespace Locker
{
    partial class Notifications
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblNotification = new System.Windows.Forms.Label();
            this.picNotificationImage = new System.Windows.Forms.PictureBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.picNotificationImage)).BeginInit();
            this.SuspendLayout();
            // 
            // lblNotification
            // 
            this.lblNotification.AutoSize = true;
            this.lblNotification.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblNotification.Location = new System.Drawing.Point(36, 11);
            this.lblNotification.Name = "lblNotification";
            this.lblNotification.Size = new System.Drawing.Size(41, 15);
            this.lblNotification.TabIndex = 1;
            this.lblNotification.Text = "label1";
            // 
            // picNotificationImage
            // 
            this.picNotificationImage.Location = new System.Drawing.Point(4, 5);
            this.picNotificationImage.Name = "picNotificationImage";
            this.picNotificationImage.Size = new System.Drawing.Size(27, 27);
            this.picNotificationImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picNotificationImage.TabIndex = 0;
            this.picNotificationImage.TabStop = false;
            // 
            // Notifications
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Chartreuse;
            this.ClientSize = new System.Drawing.Size(112, 36);
            this.Controls.Add(this.lblNotification);
            this.Controls.Add(this.picNotificationImage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.Name = "Notifications";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Notifications";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Notifications_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.picNotificationImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picNotificationImage;
        private System.Windows.Forms.Label lblNotification;
        private System.Windows.Forms.ToolTip toolTip;
    }
}