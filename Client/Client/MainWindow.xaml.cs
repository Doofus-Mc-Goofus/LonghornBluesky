﻿using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Net.Http;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
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
        private readonly DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private Dashboard dashboard;
        private readonly App app = (App)Application.Current;
        private readonly NotificationOverlay notificationOverlay = new NotificationOverlay();
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string list = string.Empty;
            try
            {
                list += "\n\nError Code: " + e.Exception.HResult.ToString();
            }
            catch
            {

            }
            try
            {
                list += "\n\nInner Exception: " + e.Exception.InnerException.Message;
            }
            catch
            {

            }
            try
            {
                list += "\n\nStack Trace:" + e.Exception.StackTrace;
            }
            catch
            {

            }
            try
            {
                list += "\n\nTarget: " + e.Exception.TargetSite.ToString();
            }
            catch
            {

            }
            string message = "A fatal error has occurred.\n\n" + e.Exception.Message;
            _ = MessageBox.Show(message + "\n\nThe client will now create a log and close.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            using (FileStream fs = File.Create(DateTime.Now.Ticks + ".log"))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(e.Exception.Message + list);
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
            if (SystemParameters.PrimaryScreenHeight < 768 || SystemParameters.PrimaryScreenWidth < 1024 || System.Windows.Forms.Screen.PrimaryScreen.BitsPerPixel < 32)
            {
                _ = MessageBox.Show("Longhorn Bluesky requires a monitor with a resolution of at least 1024x768 and 32-bit color.", "Installation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            app = (App)Application.Current;
            app.ChangeTheme(new Uri("pack://application:,,,/PresentationFramework.Aero;V4.0.0.0;31bf3856ad364e35;component/themes/aero.normalcolor.xaml"));
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            Login loginpage = new Login(this);
            Content = loginpage;
            HKCU_AddKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", "Client.exe", 11000);
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "Ver", "0.2.1");
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "isCanary", "false");
            if (HKCU_GetString(@"SOFTWARE\LonghornBluesky", "ALERT") == null)
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
            if (HKCU_GetString(@"SOFTWARE\LonghornBluesky", "LOGON") == null)
            {
                HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "LOGON", "LH_WELCOME.wav");
                HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "LOGOFF", "LH_EXIT.wav");
            }
            if (HKCU_GetString(@"SOFTWARE\LonghornBluesky", "LOGOFF") == "LH_ACCOUNTDELETE.wav")
            {
                HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "LOGOFF", "LH_EXIT.wav");
            }
            if (HKCU_GetString(@"SOFTWARE\LonghornBluesky", "isLOG") == null)
            {
                HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "isLOG", "false");
            }
            if (HKCU_GetString(@"SOFTWARE\LonghornBluesky", "showMenu") == null)
            {
                HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "showMenu", "false");
            }
            if (HKCU_GetString(@"SOFTWARE\LonghornBluesky", "wnd_left") != null)
            {
                Left = double.Parse(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "wnd_left"));
                Top = double.Parse(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "wnd_top"));
                if (HKCU_GetString(@"SOFTWARE\LonghornBluesky", "wnd_state") == "Maximized")
                {
                    WindowState = WindowState.Maximized;
                }
                else
                {
                    Width = double.Parse(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "wnd_width"));
                    Height = double.Parse(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "wnd_height"));
                }
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
            notifyIcon1.BalloonTipClicked += (s, ee) => _ = FocusWindow(false);
            notifyIcon1.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            _ = notifyIcon1.ContextMenuStrip.Items.Add("Check Notifications", null, (s, ee) => _ = FocusWindow(true));
            _ = notifyIcon1.ContextMenuStrip.Items.Add("Close Bluesky", null, (s, ee) => Application.Current.Shutdown());
            dispatcherTimer.Interval = TimeSpan.FromSeconds(10);
            dispatcherTimer.Tick += (s, ee) => _ = CheckForUpdates();
            dispatcherTimer.Start();
            notificationOverlay.Show();
        }

        public async Task CheckForUpdates()
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                // If you are making a fork of this client, PLEASE change the URLs below to your own. If you don't, the client will try to install the original client's updates, which may cause problems.
                HttpResponseMessage response = HKCU_GetString(@"SOFTWARE\LonghornBluesky", "isCanary") == "true"
                    ? await httpClient.GetAsync("https://system24.neocities.org/projects/api/LHbluesky/currentCanaryVer")
                    : await httpClient.GetAsync("https://system24.neocities.org/projects/api/LHbluesky/currentver");
                string latestVer = await response.Content.ReadAsStringAsync();
                if (HKCU_GetString(@"SOFTWARE\LonghornBluesky", "Ver") != latestVer)
                {
                    isclientNotif = true;
                    notificationOverlay.CreateNotification("A new Bluesky update is available", "Click here to install the updates", 0);
                    SoundPlayer soundPlayer = new SoundPlayer(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "UPDATEALERT"));
                    soundPlayer.Play();
                    dispatcherTimer.Stop();
                }
                else
                {
                    dispatcherTimer.Interval = TimeSpan.FromMinutes(1);
                    dispatcherTimer.Start();
                }
            }
            catch
            {
                notificationOverlay.CreateNotification("Automatic updates are unavailable", "Automatic updates are currently unavailable. You will have to check for them manually", 0);
                SoundPlayer soundPlayer = new SoundPlayer(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "UPDATEALERT"));
                soundPlayer.Play();
                dispatcherTimer.Stop();
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
                _ = FocusWindow(false);
            }
        }

        private async Task FocusWindow(bool ignore)
        {
            _ = Activate();
            _ = Focus();
            if (issignedIn)
            {
                if (isclientNotif && !ignore)
                {
                    Process process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = @"explorer",
                            Arguments = @"https://github.com/Doofus-Mc-Goofus/LonghornBluesky/releases/latest"
                        }
                    };
                    _ = await Task.Run(process.Start);
                    await Task.Run(process.WaitForExit);
                    await Task.Run(process.Close);
                    await Task.Run(process.Dispose);
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
        public void HKCU_DeleteKey(string path)
        {
            Registry.CurrentUser.DeleteSubKey(path);
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
            Dashboard dashboard = new Dashboard(session, aTProtocol, this);
            await dashboard.Load();
            Content = dashboard;
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

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "wnd_left", Left.ToString());
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "wnd_top", Top.ToString());
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "wnd_width", Width.ToString());
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "wnd_height", Height.ToString());
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "wnd_state", WindowState.ToString());
            notifyIcon1.Dispose();
            notificationOverlay.Close();
            if (HKCU_GetString(@"SOFTWARE\LonghornBluesky", "isLOG") == "True" && issignedIn)
            {
                e.Cancel = true;
                Visibility = Visibility.Hidden;
                // IDK
                await Task.Delay(100);
                SoundPlayer soundPlayer = new SoundPlayer(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "LOGOFF"));
                soundPlayer.PlaySync();
                e.Cancel = false;
                Application.Current.Shutdown();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (issignedIn && (e.Key == Key.LeftAlt || e.Key == Key.RightAlt || e.Key == Key.System))
            {
                dashboard.ToggleMenu();
            }
        }
    }
}
