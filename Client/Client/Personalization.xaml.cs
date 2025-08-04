using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using INI;
using Microsoft.Win32;

namespace Client
{
    /// <summary>
    /// Interaction logic for Personalization.xaml
    /// </summary>
    public partial class Personalization : Page
    {
        private readonly Settings settings;
        private readonly Dashboard dashboard;
        private readonly OpenFileDialog dialog = new OpenFileDialog();
        public Personalization(Settings settings, Dashboard dashboard)
        {
            InitializeComponent();
            this.settings = settings;
            this.dashboard = dashboard;
            dialog.Filter = "WAV Files|*.wav"; // Filter files by extension
            IniFile myIni = new IniFile("config.ini");
            if (File.Exists("config.ini") && myIni.Read("ICanHasSecretBeytahFeatures", "LHbsky") == "2")
            {
                Secret.Visibility = Visibility.Visible;
            }
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
            NOTIFBUTTON.Click -= NotifBrowse_Click;
            ALERTBUTTON.Click -= AlertBrowse_Click;
            IMPORTANTALERTBUTTON.Click -= ImportantAlertBrowse_Click;
            POSTBUTTON.Click -= PostBrowse_Click;
            DELETEBUTTON.Click -= DeleteBrowse_Click;
            LOGONBUTTON.Click -= LogonBrowse_Click;
            LOGOFFBUTTON.Click -= LogonBrowse_Click;
            NOTIFBOX.Items.Clear();
            ALERTBOX.Items.Clear();
            IMPORTANTALERTBOX.Items.Clear();
            POSTBOX.Items.Clear();
            DELETEBOX.Items.Clear();
            LOGONBOX.Items.Clear();
            LOGOFFBOX.Items.Clear();
            OK.Click -= OK_Click;
            Cancel.Click -= Cancel_Click;
            Apply.Click -= Apply_Click;
            ((Grid)Content).Children.Clear();
            Content = null;
            GC.SuppressFinalize(this);
        }
        private void ApplyStuff()
        {
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "isLOG", logcheck.IsChecked.ToString());
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "NOTIF", NOTIFBOX.Tag);
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "ALERT", ALERTBOX.Tag);
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "UPDATEALERT", IMPORTANTALERTBOX.Tag);
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "POST", POSTBOX.Tag);
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "DELETE", DELETEBOX.Tag);
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "LOGON", LOGONBOX.Tag);
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "LOGOFF", LOGOFFBOX.Tag);
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "showMenu", showMenu.IsChecked);
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "showNavigation", showNavigation.IsChecked);
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "fillLayout", fillLayout.IsChecked);
            dashboard.UpdateDashboardLayout();
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
            string notifpath = HKCU_GetString(@"SOFTWARE\LonghornBluesky", "NOTIF");
            ((ComboBoxItem)NOTIFBOX.Items[0]).Content = ConvertToProperName(Path.GetFileNameWithoutExtension(notifpath));
            NOTIFBOX.Tag = notifpath;
            string alertpath = HKCU_GetString(@"SOFTWARE\LonghornBluesky", "ALERT");
            ((ComboBoxItem)ALERTBOX.Items[0]).Content = ConvertToProperName(Path.GetFileNameWithoutExtension(alertpath));
            ALERTBOX.Tag = alertpath;
            string UPDATEALERT = HKCU_GetString(@"SOFTWARE\LonghornBluesky", "UPDATEALERT");
            ((ComboBoxItem)IMPORTANTALERTBOX.Items[0]).Content = ConvertToProperName(Path.GetFileNameWithoutExtension(UPDATEALERT));
            IMPORTANTALERTBOX.Tag = UPDATEALERT;
            string POST = HKCU_GetString(@"SOFTWARE\LonghornBluesky", "POST");
            ((ComboBoxItem)POSTBOX.Items[0]).Content = ConvertToProperName(Path.GetFileNameWithoutExtension(POST));
            POSTBOX.Tag = POST;
            string DELETE = HKCU_GetString(@"SOFTWARE\LonghornBluesky", "DELETE");
            ((ComboBoxItem)DELETEBOX.Items[0]).Content = ConvertToProperName(Path.GetFileNameWithoutExtension(DELETE));
            DELETEBOX.Tag = DELETE;
            string LOGON = HKCU_GetString(@"SOFTWARE\LonghornBluesky", "LOGON");
            ((ComboBoxItem)LOGONBOX.Items[0]).Content = ConvertToProperName(Path.GetFileNameWithoutExtension(LOGON));
            LOGONBOX.Tag = LOGON;
            string LOGOFF = HKCU_GetString(@"SOFTWARE\LonghornBluesky", "LOGOFF");
            ((ComboBoxItem)LOGOFFBOX.Items[0]).Content = ConvertToProperName(Path.GetFileNameWithoutExtension(LOGOFF));
            LOGOFFBOX.Tag = LOGOFF;
            showMenu.IsChecked = bool.Parse(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "showMenu"));
            showNavigation.IsChecked = bool.Parse(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "showNavigation"));
            fillLayout.IsChecked = bool.Parse(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "fillLayout"));
        }
        private string OpenWAV()
        {
            bool? result = dialog.ShowDialog();
            return result == true ? dialog.FileName : null;
        }
        private void NotifBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = OpenWAV();
            if (path != null)
            {
                ((ComboBoxItem)NOTIFBOX.Items[0]).Content = ConvertToProperName(Path.GetFileNameWithoutExtension(path));
                NOTIFBOX.Tag = path;
            }
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
                case "LH_EXIT":
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
                case "AERO_EXIT":
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
                case "EXP_EXIT":
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
                case "Y2K_EXIT":
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
                case "NEO_EXIT":
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

        private void AlertBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = OpenWAV();
            if (path != null)
            {
                ((ComboBoxItem)ALERTBOX.Items[0]).Content = ConvertToProperName(Path.GetFileNameWithoutExtension(path));
                ALERTBOX.Tag = path;
            }
        }

        private void ImportantAlertBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = OpenWAV();
            if (path != null)
            {
                ((ComboBoxItem)IMPORTANTALERTBOX.Items[0]).Content = ConvertToProperName(Path.GetFileNameWithoutExtension(path));
                IMPORTANTALERTBOX.Tag = path;
            }
        }

        private void PostBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = OpenWAV();
            if (path != null)
            {
                ((ComboBoxItem)POSTBOX.Items[0]).Content = ConvertToProperName(Path.GetFileNameWithoutExtension(path));
                POSTBOX.Tag = path;
            }
        }

        private void DeleteBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = OpenWAV();
            if (path != null)
            {
                ((ComboBoxItem)DELETEBOX.Items[0]).Content = ConvertToProperName(Path.GetFileNameWithoutExtension(path));
                DELETEBOX.Tag = path;
            }
        }

        private void LogonBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = OpenWAV();
            if (path != null)
            {
                ((ComboBoxItem)LOGONBOX.Items[0]).Content = ConvertToProperName(Path.GetFileNameWithoutExtension(path));
                LOGONBOX.Tag = path;
            }
        }

        private void LogoffBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = OpenWAV();
            if (path != null)
            {
                ((ComboBoxItem)LOGOFFBOX.Items[0]).Content = ConvertToProperName(Path.GetFileNameWithoutExtension(path));
                LOGOFFBOX.Tag = path;
            }
        }
    }
}
