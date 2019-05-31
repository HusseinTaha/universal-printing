using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UniversalFileToPrinter
{
    public partial class MainPage : Form
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void MainPage_Load(object sender, EventArgs e)
        {
             var trayIcon      = new NotifyIcon();
    trayIcon.Text = "My application";
    //trayIcon.Icon = TheIcon

    // Add menu to tray icon and show it.
    //trayIcon.ContextMenu = trayMenu;
    //trayIcon.Visible     = true;

    Visible       = false; // Hide form window.
    ShowInTaskbar = false; // Remove from taskbar.
            this.Visible = false;
        }
    }
}
