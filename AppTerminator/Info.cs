using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace AppTerminator
{
    public partial class Info : Form
    {
        public Info()
        {
            InitializeComponent();
            FileInfo info = new FileInfo(Application.ExecutablePath);
            label3.Text = "Дата: " + info.LastWriteTime.ToString();    
            string version = Application.ProductVersion;
            label1.Text = "Версия: " + version;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://t.me/Senny970");
            System.Diagnostics.Process.Start("https://steamcommunity.com/id/--Senny--");
        }
    }
}
