using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;
using Microsoft.Win32;

namespace Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly App app = (App)Application.Current;
        private bool isDispose = false;
        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
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
            Application.Current.Shutdown();
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
        public MainWindow()
        {
            if (!Debugger.IsAttached)
            {
                Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            }
            InitializeComponent();
            if (Environment.OSVersion.Version.Major + Environment.OSVersion.Version.Minor < 8)
            {
                _ = MessageBox.Show("Longhorn Bluesky is not compatible with this version of Windows. You will need Windows 8 or newer to install this program.", "Installation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            if (SystemParameters.PrimaryScreenHeight < 768 || SystemParameters.PrimaryScreenWidth < 1024 || System.Windows.Forms.Screen.PrimaryScreen.BitsPerPixel < 32)
            {
                _ = MessageBox.Show("Longhorn Bluesky requires a monitor with a resolution of at least 1024x768 and 32-bit color.", "Installation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            if (!Environment.Is64BitOperatingSystem)
            {
                _ = MessageBox.Show("Longhorn Bluesky requires a 64-bit CPU to run.", "Installation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            app = (App)Application.Current;
            app.ChangeTheme(new Uri("pack://application:,,,/PresentationFramework.Aero;V4.0.0.0;31bf3856ad364e35;component/themes/aero.normalcolor.xaml"));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to cancel setup?", "Longhorn Bluesky Setup", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            SelectPage.Visibility = Visibility.Collapsed;
            Install.Visibility = Visibility.Visible;
            _ = InstallLHBSKY();
        }
        public static async Task<bool> IsConnectedToInternet()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return false;
            }

            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = await ping.SendPingAsync("www.google.com", 3000);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }
        private async Task InstallLHBSKY()
        {
            bool isConnected = await IsConnectedToInternet();
            if (isConnected)
            {
                try
                {
                    HttpClient httpClient = new HttpClient();
                    // If you are making a fork of this client, PLEASE change the URL below to your own. If you don't, the client will try to install the original client's updates, which may cause problems.
                    // Gets file path to latest client
                    HttpResponseMessage response = await httpClient.GetAsync("https://raw.githubusercontent.com/Doofus-Mc-Goofus/LonghornBluesky/refs/heads/main/Client/API/clientDownload");
                    string latestVer = await response.Content.ReadAsStringAsync();

                    // Downloads that file
                    DownloadText.Content = "Downloading Longhorn Bluesky...";
                    ProgressBar.Value = 25;
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "..\\client.package");
                    Stream stream = await httpClient.GetStreamAsync(latestVer);
                    FileStream fileStream = new FileStream(Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".tmp"), FileMode.Create, FileAccess.Write);
                    await stream.CopyToAsync(fileStream);
                    fileStream.Close();
                    _ = UpdateFile(filePath);

                    // Cleans up
                    DownloadText.Content = "Preparing to install...";
                    ProgressBar.Value = 50;
                    await Task.Delay(100);
                    string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).Replace(" (x86)", ""), "LonghornBluesky");
                    if (Directory.Exists(folder))
                    {
                        Directory.Delete(folder, true);
                    }
                    // Extracts it
                    DownloadText.Content = "Installing Longhorn Bluesky...";
                    ProgressBar.Value = 75;
                    try
                    {
                        await Task.Run(() => ZipFile.ExtractToDirectory(filePath, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).Replace(" (x86)", ""), "LonghornBluesky")));
                    }
                    catch (Exception ex)
                    {
                        _ = MessageBox.Show(ex.Message.ToString());
                    }

                    // And then finishes up
                    ProgressBar.Value = 100;
                    DownloadText.Content = "Finishing up...";
                    await Task.Delay(100);
                    File.Delete(filePath);
                    stream.Close();
                    httpClient.Dispose();
                    response.Dispose();
                }
                catch (Exception ex)
                {
                    List<string> list = new List<string>();
                    try
                    {
                        list.Add("Error Code: " + ex.HResult.ToString());
                    }
                    catch
                    {

                    }
                    try
                    {
                        list.Add("Inner Exception: " + ex.InnerException.Message);
                    }
                    catch
                    {

                    }
                    try
                    {
                        list.Add("Stack Trace:" + ex.StackTrace);
                    }
                    catch
                    {

                    }
                    try
                    {
                        list.Add("Target: " + ex.TargetSite.ToString());
                    }
                    catch
                    {

                    }
                    string message = "A fatal error has occurred.\n\n" + ex.Message;
                    _ = MessageBox.Show(message + "\n\nSetup will now create a log and close.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    using (FileStream fs = File.Create(DateTime.Now.Ticks + ".log"))
                    {
                        byte[] info = new UTF8Encoding(true).GetBytes(ex.Message + "\n\n" + list);
                        fs.Write(info, 0, info.Length);
                    }
                    Application.Current.Shutdown();
                }
                Finish.Visibility = Visibility.Visible;
                Install.Visibility = Visibility.Collapsed;
            }
            else
            {
                _ = MessageBox.Show("Longhorn Bluesky requires an internet connection to install. Please connect to the internet and try again", "Installation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task UpdateFile(string filename)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // probably sloppy and terrible
            File.Copy(Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".tmp"), filename, true);
            File.Delete(Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".tmp"));
        }
        private void CancelNormal_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to cancel setup?", "Longhorn Bluesky Setup", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            if (RunCheck.IsChecked == true)
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).Replace(" (x86)", ""), "LonghornBluesky\\Client.exe"),
                        WorkingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).Replace(" (x86)", ""), "LonghornBluesky")
                    }
                };
                _ = Task.Run(process.Start);
            }
            if (DesktopShortcutCheck.IsChecked == true)
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Longhorn Bluesky.lnk";
                Uri uri = new Uri("pack://application:,,,/lhbsky.shortcut");
                StreamResourceInfo imageInfo = Application.GetResourceStream(uri);
                FileStream fileStream = new FileStream(desktopPath, FileMode.Create, FileAccess.Write);
                imageInfo.Stream.CopyTo(fileStream);
                fileStream.Close();
            }
            Application.Current.Shutdown();
        }

        private void BG_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BG.Focus();
        }
    }
}
