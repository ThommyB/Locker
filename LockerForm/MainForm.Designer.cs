﻿namespace Locker
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuNotifyIcon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.unbindKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearDeviceListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblBindKey = new System.Windows.Forms.Label();
            this.contextMenuNotifyIcon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuNotifyIcon;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Visible = true;
            // 
            // contextMenuNotifyIcon
            // 
            this.contextMenuNotifyIcon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.unbindKeyToolStripMenuItem,
            this.clearDeviceListToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.contextMenuNotifyIcon.Name = "contextMenuNotifyIcon";
            this.contextMenuNotifyIcon.Size = new System.Drawing.Size(161, 70);
            // 
            // unbindKeyToolStripMenuItem
            // 
            this.unbindKeyToolStripMenuItem.Name = "unbindKeyToolStripMenuItem";
            this.unbindKeyToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.unbindKeyToolStripMenuItem.Text = "Unbind Key";
            this.unbindKeyToolStripMenuItem.Click += new System.EventHandler(this.unbindKeyToolStripMenuItem_Click);
            // 
            // clearDeviceListToolStripMenuItem
            // 
            this.clearDeviceListToolStripMenuItem.Name = "clearDeviceListToolStripMenuItem";
            this.clearDeviceListToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.clearDeviceListToolStripMenuItem.Text = "Clear Device List";
            this.clearDeviceListToolStripMenuItem.Click += new System.EventHandler(this.clearDeviceListToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = global::Locker.Properties.Resources.locked_icon;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(122, 153);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // lblBindKey
            // 
            this.lblBindKey.AutoSize = true;
            this.lblBindKey.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblBindKey.Location = new System.Drawing.Point(17, 129);
            this.lblBindKey.Name = "lblBindKey";
            this.lblBindKey.Size = new System.Drawing.Size(92, 19);
            this.lblBindKey.TabIndex = 4;
            this.lblBindKey.Text = "Hit the Key";
            this.lblBindKey.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(122, 153);
            this.Controls.Add(this.lblBindKey);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            this.Opacity = 0.9D;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.contextMenuNotifyIcon.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuNotifyIcon;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolStripMenuItem unbindKeyToolStripMenuItem;
        private System.Windows.Forms.Label lblBindKey;
        private System.Windows.Forms.ToolStripMenuItem clearDeviceListToolStripMenuItem;
    }
}

