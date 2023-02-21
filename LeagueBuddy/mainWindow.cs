using System.ComponentModel;
using System.Resources;
using static Swan.Terminal;
using System.Windows.Forms;
using System.Drawing;
using LeagueBuddy;
using LeagueBuddy.Main;

namespace LeagueBuddy {
    public partial class mainWindow : Form {
        private NotifyIcon trayIcon;
        public mainWindow() {
            InitializeComponent();
            trayIcon = new NotifyIcon {
                Text = "LeagueBuddy",
                Icon = Properties.Resources.LeagueBuddyIcon,
                Visible = true
            };
            HideApp();
            Launcher.LoadSettings();
            Launcher.LoadLogins();
            Launcher.Launch();
            trayIcon.Click += new System.EventHandler(trayIcon_Click);
        }
        private void trayIcon_Click(object sender, EventArgs e) {
            Utils.KillClientProcesses();
            trayIcon.Dispose();
            Environment.Exit(0);
        }
        private async void HideApp() {
            await Task.Delay(5000);
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
        }
    }
}