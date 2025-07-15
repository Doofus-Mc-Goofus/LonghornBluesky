using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace Client
{
    /// <summary>
    /// Interaction logic for Personalization.xaml
    /// </summary>
    public partial class Personalization : Page
    {
        private readonly Settings settings;
        private readonly Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
        public Personalization(Settings settings)
        {
            InitializeComponent();
            this.settings = settings;
            dialog.Filter = "WAV Files|*.wav"; // Filter files by extension
        }
        private void Rectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _ = rect.Focus();
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // fix
            Unloaded -= Page_Unloaded;
            rect.MouseUp -= Rectangle_MouseUp;
            homie.Children.Clear();
            ((Grid)Content).Children.Clear();
            Content = null;
            GC.SuppressFinalize(this);
        }
        private void ApplyStuff()
        {
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "isLOG", logcheck.IsChecked.ToString());
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
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            ApplyStuff();
            settings.GoBack();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            settings.GoBack();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            ApplyStuff();
        }

        private void LogCheck(object sender, RoutedEventArgs e)
        {
            LogSounds.Visibility = Visibility.Visible;
        }

        private void LogUncheck(object sender, RoutedEventArgs e)
        {
            LogSounds.Visibility = Visibility.Collapsed;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            logcheck.IsChecked = bool.Parse(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "isLOG"));
        }
        private string OpenWAV()
        {
            bool? result = dialog.ShowDialog();
            return result == true ? dialog.FileName : null;
        }
        private void NotifBrowse_Click(object sender, RoutedEventArgs e)
        {
            _ = MessageBox.Show(ConvertToProperName(Path.GetFileNameWithoutExtension(OpenWAV())));
        }
        private string ConvertToProperName(string filename)
        {
            // wouldn't it be more convienent and cool to read the track title instead of hardcoding it?
            // yes
            // is this a bad way of doing it?
            // yes
            // do I care?
            // no
            switch (filename)
            {
                case "LH_ACCOUNTDELETE":
                    return "Longhorn Logoff";
                case "LH_ALERT":
                    return "Longhorn Alert";
                case "LH_DELETE":
                    return "Longhorn Deleting";
                case "LH_NOTIF":
                    return "Longhorn Notification";
                case "LH_POST":
                    return "Longhorn Posting";
                case "LH_UPDATEALERT":
                    return "Longhorn Important Alert";
                case "LH_WELCOME":
                    return "Longhorn Logon";
                case "AERO_ACCOUNTDELETE":
                    return "Aero Logoff";
                case "AERO_ALERT":
                    return "Aero Alert";
                case "AERO_DELETE":
                    return "Aero Deleting";
                case "AERO_NOTIF":
                    return "Aero Notification";
                case "AERO_POST":
                    return "Aero Posting";
                case "AERO_UPDATEALERT":
                    return "Aero Important Alert";
                case "AERO_WELCOME":
                    return "Aero Logon";
                case "EXP_ACCOUNTDELETE":
                    return "eXPerience Logoff";
                case "EXP_ALERT":
                    return "eXPerience Alert";
                case "EXP_DELETE":
                    return "eXPerience Deleting";
                case "EXP_NOTIF":
                    return "eXPerience Notification";
                case "EXP_POST":
                    return "eXPerience Posting";
                case "EXP_UPDATEALERT":
                    return "eXPerience Important Alert";
                case "EXP_WELCOME":
                    return "eXPerience Logon";
                case "Y2K_ACCOUNTDELETE":
                    return "Y2K Logoff";
                case "Y2K_ALERT":
                    return "Y2K Alert";
                case "Y2K_DELETE":
                    return "Y2K Deleting";
                case "Y2K_NOTIF":
                    return "Y2K Notification";
                case "Y2K_POST":
                    return "Y2K Posting";
                case "Y2K_UPDATEALERT":
                    return "Y2K Important Alert";
                case "Y2K_WELCOME":
                    return "Y2K Logon";
                case "NEO_ACCOUNTDELETE":
                    return "Neo-Chicago Logoff";
                case "NEO_ALERT":
                    return "Neo-Chicago Alert";
                case "NEO_DELETE":
                    return "Neo-Chicago Deleting";
                case "NEO_NOTIF":
                    return "Neo-Chicago Notification";
                case "NEO_POST":
                    return "Neo-Chicago Posting";
                case "NEO_UPDATEALERT":
                    return "Neo-Chicago Important Alert";
                case "NEO_WELCOME":
                    return "Neo-Chicago Logon";
                default:
                    return filename;
            }
        }
    }
}
