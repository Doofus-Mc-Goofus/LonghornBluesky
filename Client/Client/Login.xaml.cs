using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FishyFlip;
using INI;
using Microsoft.Extensions.Logging.Debug;

namespace Client
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        private readonly MainWindow mw;
        public Login(MainWindow mainWindow)
        {
            InitializeComponent();
            SupportText.Visibility = Visibility.Visible;
            PassBoxButton.Visibility = Visibility.Visible;
            LoginPage.Visibility = Visibility.Visible;
            WelcomePage.Visibility = Visibility.Collapsed;
            mw = mainWindow;
            PassBoxButton.MouseUp += (s, ee) => _ = SignIntoBlueSky(UserBox.Text, PassBox.Password, HostProv.Text);
            if (File.Exists("config.ini"))
            {
                IniFile myIni = new IniFile("config.ini");
                if (myIni.Read("MSN", "LHbsky") == "1")
                {
                    Wordmark.Source = new BitmapImage(new Uri("pack://application:,,,/res/logoshad.png"));
                    Wordmark.Height = 102;
                    Wordmark.Margin = new Thickness(0, 0, 0, 140);
                    Welcome.FontSize = 24;
                }
                if (myIni.Read("ICanHasSecretBeytahFeatures", "LHbsky") == "1" || myIni.Read("ICanHasSecretBeytahFeatures", "LHbsky") == "2")
                {
                    CreateAccount.Visibility = Visibility.Visible;
                    LoginGuest.Visibility = Visibility.Visible;
                    ForgotPassword.Visibility = Visibility.Visible;
                }
            }
            if (mainWindow.HKCU_GetString(@"SOFTWARE\LonghornBluesky", "Remember") == "true")
            {
                _ = SignIntoBlueSky(mainWindow.HKCU_GetString(@"SOFTWARE\LonghornBluesky", "RememberUsername"), mainWindow.HKCU_GetString(@"SOFTWARE\LonghornBluesky", "RememberPassword"), mainWindow.HKCU_GetString(@"SOFTWARE\LonghornBluesky", "RememberHost"));
            }
        }
        private async Task SignIntoBlueSky(string identifier, string password, string provider)
        {
            SupportText.Visibility = Visibility.Collapsed;
            PassBoxButton.Visibility = Visibility.Collapsed;
            LoginPage.Visibility = Visibility.Collapsed;
            WelcomePage.Visibility = Visibility.Visible;
            WelcomeText.Content = "Please wait...";
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            Uri test = new Uri("https://bsky.social");
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            try
            {
                test = new Uri(provider);
            }
            catch
            {
                _ = MessageBox.Show("Failed to authenticate.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            WelcomeText.Content = "Connecting to Bluesky...";
            ATProtocol atProtocol = new ATProtocolBuilder()
            .WithLogger(new DebugLoggerProvider().CreateLogger("FishyFlip"))
            .WithInstanceUrl(new Uri(HostProv.Text))
            .Build();
            WelcomeText.Content = "Attempting to authenticate...";
            (FishyFlip.Models.Session session, FishyFlip.Models.ATError error) = await atProtocol.AuthenticateWithPasswordResultAsync(identifier, password);
            if (session is null)
            {
                ShowPageAgain();
                Error.Visibility = Visibility.Visible;
                if (identifier[0].ToString() == "@")
                {
                    Error.Content = "Your username cannot start with @";
                }
                else if (identifier == string.Empty || password == string.Empty)
                {
                    Error.Content = "An identifier and password is required to sign in";
                }
                else
                {
                    Error.Content = error.Detail.Message;
                }
                return;
            }
            // _ = MessageBox.Show("Authenticated.");
            // _ = MessageBox.Show($"Session Did: {session.Did}");
            // _ = MessageBox.Show($"Session Email: {session.Email}");
            // _ = MessageBox.Show($"Session Handle: {session.Handle}");
            // _ = MessageBox.Show($"Session Token: {session.AccessJwt}");
            WelcomeText.Content = "Almost there...";
            if ((bool)RememberCheckBox.IsChecked)
            {
                mw.HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "Remember", "true");
                mw.HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "RememberUsername", identifier);
                mw.HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "RememberPassword", password);
                mw.HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "RememberHost", provider);
            }
            _ = mw.OpenFeed(session, atProtocol);
        }

        private void TimeOutFunc(object sender, EventArgs e)
        {
            ShowPageAgain();
            Error.Visibility = Visibility.Visible;
            Error.Content = "Connection timed out";
        }
        private void ShowPageAgain()
        {
            SupportText.Visibility = Visibility.Visible;
            PassBoxButton.Visibility = Visibility.Visible;
            LoginPage.Visibility = Visibility.Visible;
            WelcomePage.Visibility = Visibility.Collapsed;
        }
        private void UserBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (UserBox.Text == string.Empty)
            {
                UserBox.Text = "Username or email address";
            }
            UserBox.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void UserBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (UserBox.Text == "Username or email address")
            {
                UserBox.Text = string.Empty;
            }
            UserBox.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void PassBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            PasswordPreviewText.Visibility = Visibility.Collapsed;
            PassBox.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void PassBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (PassBox.Password == string.Empty)
            {
                PasswordPreviewText.Visibility = Visibility.Visible;
            }
            PassBox.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void HostProv_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HostProv.Text == string.Empty)
            {
                HostProv.Text = "https://bsky.social";
            }
            HostProv.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void HostProv_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HostProv.Text == "https://bsky.social")
            {
                HostProv.Text = string.Empty;
            }
            HostProv.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _ = BG.Focus();
        }

        private void PassBoxButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PassBoxButton.Source = new BitmapImage(new Uri("pack://application:,,,/res/signinbutton-3.png"));
        }

        private void PassBoxButton_MouseEnter(object sender, MouseEventArgs e)
        {
            PassBoxButton.Source = new BitmapImage(new Uri("pack://application:,,,/res/signinbutton-2.png"));
            PassBoxButton.Tag = true;
        }

        private void PassBoxButton_MouseLeave(object sender, MouseEventArgs e)
        {
            PassBoxButton.Source = new BitmapImage(new Uri("pack://application:,,,/res/signinbutton-1.png"));
            PassBoxButton.Tag = false;
        }

        private void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ServWrapPanel.Visibility = Visibility.Visible;
        }

        private void CheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            ServWrapPanel.Visibility = Visibility.Collapsed;
            HostProv.Text = "https://bsky.social";
        }

        private void Label_MouseEnter(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).TextDecorations.Add(TextDecorations.Underline);
        }

        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).TextDecorations.Clear();
        }

        private void Support_Click(object sender, MouseButtonEventArgs e)
        {
            Support sup = new Support();
            sup.Show();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= Page_Unloaded;
            BG.MouseDown -= Image_MouseDown;
            Wordmark.MouseDown -= Image_MouseDown;
            UserBox.LostKeyboardFocus -= UserBox_LostKeyboardFocus;
            UserBox.GotKeyboardFocus -= UserBox_GotKeyboardFocus;
            PassBox.LostKeyboardFocus -= PassBox_LostKeyboardFocus;
            PassBox.GotKeyboardFocus -= PassBox_GotKeyboardFocus;
            HostCheckBox.Checked -= CheckBox_Checked;
            HostCheckBox.Unchecked -= CheckBox_Unchecked;
            HostProv.LostKeyboardFocus -= HostProv_LostKeyboardFocus;
            HostProv.GotKeyboardFocus -= HostProv_GotKeyboardFocus;
            CreateAccount.MouseEnter -= Label_MouseEnter;
            CreateAccount.MouseLeave -= Label_MouseLeave;
            LoginGuest.MouseEnter -= Label_MouseEnter;
            LoginGuest.MouseLeave -= Label_MouseLeave;
            ForgotPassword.MouseEnter -= Label_MouseEnter;
            ForgotPassword.MouseLeave -= Label_MouseLeave;
            SupportText.MouseEnter -= Label_MouseEnter;
            SupportText.MouseLeave -= Label_MouseLeave;
            SupportText.MouseUp -= Support_Click;
            PassBoxButton.MouseEnter -= PassBoxButton_MouseEnter;
            PassBoxButton.MouseLeave -= PassBoxButton_MouseLeave;
            PassBoxButton.MouseDown -= PassBoxButton_MouseDown;
            PassBoxButton.MouseUp -= PassBoxButton_MouseLeave;
            PassBoxButton.MouseUp -= (s, ee) => _ = SignIntoBlueSky(UserBox.Text, PassBox.Password, HostProv.Text);
            GC.SuppressFinalize(this);
        }
    }
}
