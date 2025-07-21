using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Net.Http;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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
        private Frame dashboardFrame;
        private bool isAero;
        private bool isDispose = false;
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            isDispose = true;
            string list = string.Empty;
            try
            {
                list += "\n\nError Code: " + ((uint)e.Exception.HResult).ToString();
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
                list += "\n\nDetailed Stack Trace: " + Environment.StackTrace.ToString();
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
            try
            {
                list += "\n\nOS Version: " + Environment.OSVersion.ToString();
            }
            catch
            {

            }
            try
            {
                list += "\n\n.NET Version: " + Environment.Version.ToString();
            }
            catch
            {

            }
            try
            {
                list += "\n\nClient Version: " + HKCU_GetString(@"SOFTWARE\LonghornBluesky", "Ver");
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
            grid.Visibility = Visibility.Collapsed;
            if (SystemParameters.PrimaryScreenHeight < 768 || SystemParameters.PrimaryScreenWidth < 1024 || System.Windows.Forms.Screen.PrimaryScreen.BitsPerPixel < 32)
            {
                _ = MessageBox.Show("Longhorn Bluesky requires a monitor with a resolution of at least 1024x768 and 32-bit color.", "Installation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            app = (App)Application.Current;
            app.ChangeTheme(new Uri("pack://application:,,,/PresentationFramework.Aero;V4.0.0.0;31bf3856ad364e35;component/themes/aero.normalcolor.xaml"));
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            Login loginpage = new Login(this);
            WindowContent.Content = loginpage;
            HKCU_AddKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", "Client.exe", 11000);
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "Ver", "0.2.2a");
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "isCanary", "true");
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
            if (HKCU_GetString(@"SOFTWARE\LonghornBluesky", "checkUpdates") == null)
            {
                HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "checkUpdates", "true");
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
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        public async Task CheckForUpdates()
        {
            if (HKCU_GetString(@"SOFTWARE\LonghornBluesky", "checkUpdates") == "true")
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
                        dispatcherTimer.Stop();
                        try
                        {
                            SoundPlayer soundPlayer = new SoundPlayer(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "UPDATEALERT"));
                            soundPlayer.Play();
                        }
                        catch
                        {
                            _ = new Exception();
                        }
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
                    dispatcherTimer.Stop();
                    SoundPlayer soundPlayer = new SoundPlayer(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "UPDATEALERT"));
                    soundPlayer.Play();
                }
            }
            else
            {
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
            dashboardFrame = dashboard.PageFrame;
            await dashboard.Load();
            WindowContent.Content = dashboard;
            WindowContent.NavigationService.Navigated += NavServiceOnNavigated;
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
        private void NavServiceOnNavigated(object sender, NavigationEventArgs args)
        {
            _ = WindowContent.NavigationService.RemoveBackEntry();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            WindowContent.NavigationService.Navigated -= NavServiceOnNavigated;
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
            if (HKCU_GetString(@"SOFTWARE\LonghornBluesky", "isLOG") == "True" && issignedIn && !isDispose)
            {
                e.Cancel = true;
                Visibility = Visibility.Hidden;
                // IDK
                await Task.Delay(100);
                try
                {
                    SoundPlayer soundPlayer = new SoundPlayer(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "LOGOFF"));
                    soundPlayer.PlaySync();
                }
                catch
                {

                }
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
        private void Back_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Back.Source = !dashboardFrame.CanGoBack
                ? new BitmapImage(new Uri("pack://application:,,,/res/BackDisabled.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/BackPressed.png"));
        }

        private void Back_MouseEnter(object sender, MouseEventArgs e)
        {
            Back.Source = !dashboardFrame.CanGoBack
                ? new BitmapImage(new Uri("pack://application:,,,/res/BackDisabled.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/BackHover.png"));
        }

        private void Back_MouseLeave(object sender, MouseEventArgs e)
        {
            Back.Source = !dashboardFrame.CanGoBack
                ? new BitmapImage(new Uri("pack://application:,,,/res/BackDisabled.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/BackNormal.png"));
        }
        private void Back_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Back.Source = !dashboardFrame.CanGoBack
                ? new BitmapImage(new Uri("pack://application:,,,/res/BackDisabled.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/BackHover.png"));
        }
        private void Forward_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Forward.Source = !dashboardFrame.CanGoForward
                ? new BitmapImage(new Uri("pack://application:,,,/res/ForwardDisabled.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/ForwardPressed.png"));
        }

        private void Forward_MouseEnter(object sender, MouseEventArgs e)
        {
            Forward.Source = !dashboardFrame.CanGoForward
                ? new BitmapImage(new Uri("pack://application:,,,/res/ForwardDisabled.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/ForwardHover.png"));
        }

        private void Forward_MouseLeave(object sender, MouseEventArgs e)
        {
            Forward.Source = !dashboardFrame.CanGoForward
                ? new BitmapImage(new Uri("pack://application:,,,/res/ForwardDisabled.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/ForwardNormal.png"));
        }
        private void Forward_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Forward.Source = !dashboardFrame.CanGoForward
                ? new BitmapImage(new Uri("pack://application:,,,/res/ForwardDisabled.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/ForwardHover.png"));
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;      // width of left border that retains its size
            public int cxRightWidth;     // width of right border that retains its size
            public int cyTopHeight;      // height of top border that retains its size
            public int cyBottomHeight;   // height of bottom border that retains its size
        };

        [DllImport("DwmApi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(
            IntPtr hwnd,
            ref MARGINS pMarInset);
        [DllImport("dwmapi.dll")]
        public static extern IntPtr DwmIsCompositionEnabled(out bool pfEnabled);
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IniFile myIni = new IniFile("config.ini");
            if (myIni.Read("ICanHasSecretBeytahFeatures", "LHbsky") == "1" || myIni.Read("ICanHasSecretBeytahFeatures", "LHbsky") == "2")
            {
                MinHeight = 650;
                try
                {
                    // Obtain the window handle for WPF application
                    IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
                    HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
                    _ = DwmIsCompositionEnabled(out bool istheAero);
                    isAero = istheAero;
                    UpdateExtendedFrames();
                    // Get System Dpi
                    System.Drawing.Graphics desktop = System.Drawing.Graphics.FromHwnd(mainWindowPtr);
                    float DesktopDpiX = desktop.DpiX;
                    float DesktopDpiY = desktop.DpiY;

                    // Set Margins
                    MARGINS margins = new MARGINS
                    {
                        // Extend glass frame into client area
                        // Note that the default desktop Dpi is 96dpi. The  margins are
                        // adjusted for the system Dpi.
                        cxLeftWidth = 0,
                        cxRightWidth = 0,
                        cyTopHeight = 0,
                        cyBottomHeight = 0
                    };

                    int hr = DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
                    //
                    if (hr < 0)
                    {
                        //DwmExtendFrameIntoClientArea Failed
                    }
                }
                // If not Vista, paint background white.
                catch (DllNotFoundException)
                {
                    Application.Current.MainWindow.Background = Brushes.White;
                }
            }
            grid.LayoutUpdated += Grid_LayoutUpdated;
        }

        private void Grid_LayoutUpdated(object sender, EventArgs e)
        {
            IniFile myIni = new IniFile("config.ini");
            if (myIni.Read("ICanHasSecretBeytahFeatures", "LHbsky") == "1" || myIni.Read("ICanHasSecretBeytahFeatures", "LHbsky") == "2")
            {
                MinHeight = 650;
                try
                {
                    // Obtain the window handle for WPF application
                    IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
                    HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
                    _ = DwmIsCompositionEnabled(out bool istheAero);
                    isAero = istheAero;
                    UpdateExtendedFrames();
                    // Get System Dpi
                    System.Drawing.Graphics desktop = System.Drawing.Graphics.FromHwnd(mainWindowPtr);
                    float DesktopDpiX = desktop.DpiX;
                    float DesktopDpiY = desktop.DpiY;

                    // Set Margins
                    MARGINS margins = new MARGINS
                    {
                        // Extend glass frame into client area
                        // Note that the default desktop Dpi is 96dpi. The  margins are
                        // adjusted for the system Dpi.
                        cxLeftWidth = 0,
                        cxRightWidth = 0,
                        cyTopHeight = Convert.ToInt32(((int)topBar.ActualHeight) * (DesktopDpiX / 96)),
                        cyBottomHeight = 0
                    };

                    int hr = DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
                    //
                    if (hr < 0)
                    {
                        //DwmExtendFrameIntoClientArea Failed
                    }
                }
                // If not Vista, paint background white.
                catch (DllNotFoundException)
                {
                    Application.Current.MainWindow.Background = Brushes.White;
                }
            }
        }

        private void UpdateExtendedFrames()
        {
            IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
            HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
            mainWindowSrc.CompositionTarget.BackgroundColor = isAero ? Color.FromArgb(0, 185, 209, 234) : IsActive ? Color.FromArgb(255, 185, 209, 234) : Color.FromArgb(255, 215, 228, 242);
        }
        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.VisualStyle)
            {
                _ = DwmIsCompositionEnabled(out bool istheAero);
                isAero = istheAero;
                UpdateExtendedFrames();
            }
        }

        private void Window_Focus(object sender, EventArgs e)
        {
            UpdateExtendedFrames();
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            stackPanel.Visibility = Visibility.Collapsed;
        }

        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            stackPanel.Visibility = Visibility.Visible;
        }
    }
}
