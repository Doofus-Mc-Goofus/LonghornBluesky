using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FishyFlip;
using FishyFlip.Models;
using INI;
using Microsoft.Win32;
namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly System.Windows.Forms.NotifyIcon notifyIcon1 = new System.Windows.Forms.NotifyIcon();
        private bool issignedIn = false;
        private bool isclientNotif = false;
        private Dashboard dashboard;
        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var list = new List<string>();
            try
            {
                list.Add("Error Code: " + e.Exception.HResult.ToString());
            }
            catch
            {

            }
            try
            {
                list.Add("Inner Exception: " + e.Exception.InnerException.Message);
            }
            catch
            {

            }
            try
            {
                list.Add("Stack Trace:" + e.Exception.StackTrace);
            }
            catch
            {

            }
            try
            {
                list.Add("Target: " + e.Exception.TargetSite.ToString());
            }
            catch
            {

            }
            string message = "A fatal error has occurred.\n\n" + e.Exception.Message;
            _ = MessageBox.Show(message + "\n\nThe client will now create a log and close.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            using (FileStream fs = File.Create(DateTime.Now.Ticks + ".log"))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(e.Exception.Message + "\n\n" + list);
                fs.Write(info, 0, info.Length);
            }
            e.Handled = true;
            notifyIcon1.Dispose();
            Application.Current.Shutdown();
        }
        public MainWindow()
        {
            if (!Debugger.IsAttached)
            {
                Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            }
            InitializeComponent();
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            Login loginpage = new Login(this);
            this.Content = loginpage;
            HKCU_AddKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", "Client.exe", 11000);
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "Ver", "0.1.0");
            if (HKCU_GetString(@"SOFTWARE\LonghornBluesky", "Ver") == "")
            {
                HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "Remember", "false");
                HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "RememberUsername", "");
                HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "RememberPassword", "");
                HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "RememberHost", "");
                HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "first", "");
                HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "ALERT", "LH_ALERT.wav");
                HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "DELETE", "LH_DELETE.wav");
                HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "NOTIF", "LH_NOTIF.wav");
                HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "POST", "LH_POST.wav");
                HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "UPDATEALERT", "LH_UPDATEALERT.wav");
            }
            if (File.Exists("config.ini"))
            {
                IniFile myIni = new IniFile("config.ini");
                if (myIni.Read("MSN", "LHbsky") == "1")
                {
                    Icon = new BitmapImage(new Uri("pack://application:,,,/res/logo.png"));
                }
            }
            notifyIcon1.Icon = Properties.Resources.logoicongray;
            notifyIcon1.Text = "Bluesky";
            notifyIcon1.Visible = true;
            notifyIcon1.MouseClick += PhuckYou;
            notifyIcon1.BalloonTipClicked += (s, ee) => _ = FocusWindow();
            notifyIcon1.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            _ = notifyIcon1.ContextMenuStrip.Items.Add("Check Notifications", null, (s, ee) => _ = FocusWindow());
            _ = notifyIcon1.ContextMenuStrip.Items.Add("Close Bluesky", null, (s, ee) => Application.Current.Shutdown());
            _ = CheckForUpdates();
        }

        public async Task CheckForUpdates()
        {
            HttpClient httpClient = new HttpClient();
            // If you are making a fork of this client, PLEASE change the URLs below to your own. If you don't, the client will try to install the original client's updates, which may cause problems.
            HttpResponseMessage response = await httpClient.GetAsync("https://system24.neocities.org/projects/api/LHbluesky/currentver");
            string latestVer = await response.Content.ReadAsStringAsync();
            if (HKCU_GetString(@"SOFTWARE\LonghornBluesky", "Ver") != latestVer)
            {
                isclientNotif = true;
                notifyIcon1.BalloonTipTitle = "A new Bluesky update is available";
                notifyIcon1.BalloonTipText = "Click here to install the updates now";
                notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
                notifyIcon1.ShowBalloonTip(10000);
                MediaPlayer mediaPlayer = new MediaPlayer();
                mediaPlayer.Open(new Uri(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "UPDATEALERT"), UriKind.RelativeOrAbsolute));
                mediaPlayer.Play();
                mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
                mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            }
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            ((MediaPlayer)sender).Close();
        }

        private void MediaPlayer_MediaFailed(object sender, ExceptionEventArgs e)
        {
            ((MediaPlayer)sender).Close();
        }

        private void PhuckYou(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                _ = FocusWindow();
            }
        }

        private async Task FocusWindow()
        {
            Activate();
            Focus();
            if (issignedIn)
            {
                if (isclientNotif)
                {
                    Process process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = @"explorer",
                            Arguments = @"https://github.com/Doofus-Mc-Goofus/LonghornBluesky"
                        }
                    };
                    _ = await Task.Run(process.Start);
                    await Task.Run(process.WaitForExit);
                    await Task.Run(process.Close);
                    isclientNotif = false;
                }
                else
                {
                    dashboard.NavigateToNotifications();
                }
            }
        }
        public void HKCU_AddKey(string path, string key, object value)
        {
            RegistryKey rk = Registry.CurrentUser.CreateSubKey(path);
            rk.SetValue(key, value);
        }
        public string HKCU_GetString(string path, string key)
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(path);
                return rk == null ? "" : (string)rk.GetValue(key);
            }
            catch { return ""; }
        }
        public async Task OpenFeed(Session session, ATProtocol aTProtocol)
        {
            Dashboard dashboard = new Dashboard(session, aTProtocol);
            await dashboard.Load();
            this.Content = dashboard;
            this.dashboard = dashboard;
            notifyIcon1.Icon = Properties.Resources.logoicon;
            issignedIn = true;
            if (File.Exists("config.ini"))
            {
                IniFile myIni = new IniFile("config.ini");
                if (myIni.Read("ICanHasSecretBeytahFeatures", "LHbsky") == "1")
                {
                    _ = MessageBox.Show("Thank you for trying out the hidden beta features. Please remember that this experience may be more unstable than usual. If you find any bugs, please report them on our GitHub repository.", "Is that a The Matrix (1999) reference?", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    myIni.Write("ICanHasSecretBeytahFeatures", "2", "LHbsky");
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notifyIcon1.Dispose();
        }
    }
}
