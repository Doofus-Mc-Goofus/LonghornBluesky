using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FishyFlip;
using FishyFlip.Lexicon;
using FishyFlip.Lexicon.App.Bsky.Actor;
using FishyFlip.Lexicon.Com.Atproto.Repo;
using FishyFlip.Models;
using INI;

namespace Client
{
    /// <summary>
    /// Interaction logic for EditProf.xaml
    /// </summary>
    public partial class EditProf : Page
    {
        private readonly ATProtocol aTProtocol;
        private readonly ATDid ATDid;
        private readonly Session session;
        private readonly Dashboard dashboard;
        public string Rkey { get; set; }

        public EditProf(ATDid Uri, ATProtocol aTProtocol, Session session, Dashboard dashboard)
        {
            InitializeComponent();
            ATDid = Uri;
            this.session = session;
            this.aTProtocol = aTProtocol;
            this.dashboard = dashboard;
            if (File.Exists("config.ini"))
            {
                IniFile myIni = new IniFile("config.ini");
                if (myIni.Read("ICanHasSecretBeytahFeatures", "LHbsky") == "2")
                {
                    SecretFeatures.Visibility = Visibility.Visible;
                }
            }
            _ = Load();
        }
        private async Task Load()
        {
            Result<ProfileViewDetailed> result = await aTProtocol.GetProfileAsync(session.Did);
            result.Switch(
            success =>
            {
                Username.Text = success.DisplayName;
                Bio.Text = success.Description;
                try
                {
                    PFP.Source = new BitmapImage(new Uri(success.Avatar));
                }
                catch
                {

                }
                try
                {
                    Banner.Source = new BitmapImage(new Uri(success.Banner));
                }
                catch
                {

                }
            },
            error =>
            {
                _ = MessageBox.Show($"Error: {error.StatusCode} {error.Detail}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            );
        }
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            FishyFlip.Lexicon.App.Bsky.Actor.Profile profile = new FishyFlip.Lexicon.App.Bsky.Actor.Profile
            {
                Description = Bio.Text,
                DisplayName = Username.Text
            };
            // fix
            Result<PutRecordOutput> result = await aTProtocol.PutProfileAsync(session.Did, "", profile);
            _ = MessageBox.Show(result.Value.ToString());
            dashboard.NavigateToProfile(ATDid.ToString());
        }
        private void Rectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _ = bingus.Focus();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            dashboard.NavigateToProfile(ATDid.ToString());
        }

        private void Bio_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Bio.Text == string.Empty)
            {
                Bio.Text = "Tell us a bit about yourself";
            }
            Bio.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void Bio_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Bio.Text == "Tell us a bit about yourself")
            {
                Bio.Text = string.Empty;
            }
            Bio.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void Username_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Username.Text == "Display Name")
            {
                Username.Text = string.Empty;
            }
            Username.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void Username_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Username.Text == string.Empty)
            {
                Username.Text = "Display Name";
            }
            Username.Foreground = new SolidColorBrush(Colors.Gray);
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // fix
            Unloaded -= Page_Unloaded;
            Rectangular.MouseUp -= Rectangle_MouseUp;
            test.Children.Clear();
            wrapper.Children.Clear();
            ((ScrollViewer)Content).Content = null;
            GC.SuppressFinalize(this);
        }
    }
}
