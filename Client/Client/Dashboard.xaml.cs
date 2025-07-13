using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Actor;
using FishyFlip.Lexicon.App.Bsky.Embed;
using FishyFlip.Models;
using INI;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace Client
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Page
    {
        private readonly ATProtocol aTProtocol;
        private readonly Session session;
        private JArray feeds;
        private byte selectobjectsidebar;

        [DllImport("urlmon.dll")]
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        private static extern int CoInternetSetFeatureEnabled(
        int FeatureEntry,
        [MarshalAs(UnmanagedType.U4)] int dwFlags,
        bool fEnable);
        public Dashboard(Session session, ATProtocol aTProtocol)
        {
            InitializeComponent();
            this.session = session;
            this.aTProtocol = aTProtocol;
            selectobjectsidebar = 1;
        }
        public async Task Load()
        {
            PlayWelcomeSound();
            if (File.Exists("config.ini"))
            {
                IniFile myIni = new IniFile("config.ini");
                if (myIni.Read("ICanHasSecretBeytahFeatures", "LHbsky") == "0")
                {
                    // Hide unfinished things
                    // Explore.Visibility = Visibility.Collapsed;
                    Notifs.Visibility = Visibility.Collapsed;
                    Chat.Visibility = Visibility.Collapsed;
                    Feeds.Visibility = Visibility.Collapsed;
                    Lists.Visibility = Visibility.Collapsed;
                }
            }
            Result<GetPreferencesOutput> prefresult = await aTProtocol.GetPreferencesAsync();
            JObject obj = JObject.Parse(prefresult.Value.ToString());
            JArray arr = JArray.Parse(obj["preferences"].ToString());
            int i = 0;
            while (arr[i].SelectToken("$type").ToString() != "app.bsky.actor.defs#savedFeedsPrefV2")
            {
                i++;
            }
            JToken items = arr[i].SelectToken("items");
            JArray feeds = JArray.Parse(items.ToString());
            this.feeds = feeds;
            Home homePage = new Home(feeds, aTProtocol, this);
            _ = PageFrame.NavigationService.Navigate(homePage);
            Result<ProfileViewDetailed> result = await aTProtocol.GetProfileAsync(session.Did);
            result.Switch(
            success =>
            {
                Username.Text = success.DisplayName;
                Username.ToolTip = success.DisplayName;
                Fullname.Text = "@" + success.Handle.Handle;
                Fullname.ToolTip = "@" + success.Handle.Handle;
                PFP.Source = success.Avatar == null
                    ? new BitmapImage(new Uri("pack://application:,,,/res/usertile.png"))
                    : new BitmapImage(new Uri(success.Avatar));
            },
            error =>
            {
                _ = MessageBox.Show($"Error: {error.StatusCode} {error.Detail}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            );
        }
        public void PlayWelcomeSound()
        {
            if (HKCU_GetString(@"SOFTWARE\LonghornBluesky", "first") == "" || HKCU_GetString(@"SOFTWARE\LonghornBluesky", "first") == null)
            {
                SoundPlayer soundPlayer = new SoundPlayer("LH_WELCOME.wav");
                soundPlayer.Play();
                HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "first", "fish");
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
        public string HKCU_GetString(string path, string key)
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(path);
                return rk == null ? "" : (string)rk.GetValue(key);
            }
            catch { return ""; }
        }
        public void NavigateToProfile(string Did)
        {
            Profile ProfilePage = new Profile(ATDid.Create(Did), aTProtocol, session, this);
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(ProfilePage);
            if (session.Did.ToString() == ATDid.Create(Did).ToString())
            {
                Profile_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
                Profile_BG.Visibility = Visibility.Visible;
                Profile_BG.Opacity = 1;
                selectobjectsidebar = 7;
                HideOthersSidebar();
            }
            else
            {
                selectobjectsidebar = 0;
                HideOthersSidebar();
            }
        }
        public void NavigateToFollow(string Did, bool isBy, int number)
        {
            FollowPage ProfilePage = new FollowPage(ATDid.Create(Did), aTProtocol, isBy, this, number);
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(ProfilePage);
            selectobjectsidebar = 0;
            HideOthersSidebar();
        }
        public void NavigateToNotifications()
        {
            _ = MessageBox.Show("I didn't finish this feature yet. Please wait.", "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }
        public void NavigateToPost(string Uri)
        {
            PostPage PostPage = new PostPage(Uri, aTProtocol, this);
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(PostPage);
            selectobjectsidebar = 0;
            HideOthersSidebar();
        }
        public void NavigateToProfileEdit(EditProf Did)
        {
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(Did);
            selectobjectsidebar = 0;
            HideOthersSidebar();
        }
        public void QuotePost(EmbedRecord embedRecord)
        {
            Home_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
            Home_BG.Visibility = Visibility.Visible;
            Home_BG.Opacity = 1;
            selectobjectsidebar = 1;
            HideOthersSidebar();
            Home homePage = new Home(feeds, aTProtocol, this);
            homePage.Transfer(embedRecord);
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(homePage);
        }
        private void Label_MouseEnter(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).TextDecorations.Add(TextDecorations.Underline);
        }

        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).TextDecorations.Clear();
        }

        private void Help_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Support sup = new Support();
            sup.Show();
        }

        private void BG_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _ = BG.Focus();
        }

        private void Home_MouseEnter(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 1)
            {
                Home_BG.Visibility = Visibility.Visible;
                Home_BG.Opacity = 0.5;
            }
        }

        private void Home_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 1)
            {
                Home_BG.Visibility = Visibility.Collapsed;
            }
        }

        private void Home_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Home_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
            Home_BG.Visibility = Visibility.Visible;
            Home_BG.Opacity = 1;
            selectobjectsidebar = 1;
            HideOthersSidebar();
            Home homePage = new Home(feeds, aTProtocol, this);
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(homePage);
        }

        private void Explore_MouseEnter(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 2)
            {
                Explore_BG.Visibility = Visibility.Visible;
                Explore_BG.Opacity = 0.5;
            }
        }

        private void Explore_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 2)
            {
                Explore_BG.Visibility = Visibility.Collapsed;
            }
        }

        private void Explore_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Explore_Text.FontFamily = new FontFamily("Segoe UI Semibold");
            Explore_BG.Visibility = Visibility.Visible;
            Explore_BG.Opacity = 1;
            selectobjectsidebar = 2;
            HideOthersSidebar();
            Explore explore = new Explore(aTProtocol, this);
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(explore);
        }
        private void Notifs_MouseEnter(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 3)
            {
                Notifs_BG.Visibility = Visibility.Visible;
                Notifs_BG.Opacity = 0.5;
            }
        }

        private void Notifs_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 3)
            {
                Notifs_BG.Visibility = Visibility.Collapsed;
            }
        }

        private void Notifs_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Notifs_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
            Notifs_BG.Visibility = Visibility.Visible;
            Notifs_BG.Opacity = 1;
            selectobjectsidebar = 3;
            HideOthersSidebar();
            HandleableError usororer = new HandleableError(new ATError(418, new ErrorDetail("", "I'm a teapot")));
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(usororer);
        }
        private void HideOthersSidebar()
        {
            if (selectobjectsidebar != 1)
            {
                Home_BG.Visibility = Visibility.Collapsed;
                Home_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            }
            if (selectobjectsidebar != 2)
            {
                Explore_BG.Visibility = Visibility.Collapsed;
                Explore_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            }
            if (selectobjectsidebar != 3)
            {
                Notifs_BG.Visibility = Visibility.Collapsed;
                Notifs_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            }
            if (selectobjectsidebar != 4)
            {
                Chat_BG.Visibility = Visibility.Collapsed;
                Chat_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            }
            if (selectobjectsidebar != 5)
            {
                Feeds_BG.Visibility = Visibility.Collapsed;
                Feeds_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            }
            if (selectobjectsidebar != 6)
            {
                Lists_BG.Visibility = Visibility.Collapsed;
                Lists_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            }
            if (selectobjectsidebar != 7)
            {
                Profile_BG.Visibility = Visibility.Collapsed;
                Profile_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            }
            if (selectobjectsidebar != 8)
            {
                Settings_BG.Visibility = Visibility.Collapsed;
                Settings_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            }
        }

        private void Chat_MouseEnter(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 4)
            {
                Chat_BG.Visibility = Visibility.Visible;
                Chat_BG.Opacity = 0.5;
            }
        }

        private void Chat_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 4)
            {
                Chat_BG.Visibility = Visibility.Collapsed;
            }
        }

        private void Chat_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Chat_BG.Visibility = Visibility.Visible;
            Chat_BG.Opacity = 1;
            selectobjectsidebar = 4;
            Chat_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
            HideOthersSidebar();
            HandleableError usororer = new HandleableError(new ATError(418, new ErrorDetail("", "I'm a teapot")));
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(usororer);
        }

        private void Feeds_MouseEnter(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 5)
            {
                Feeds_BG.Visibility = Visibility.Visible;
                Feeds_BG.Opacity = 0.5;
            }
        }

        private void Feeds_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 5)
            {
                Feeds_BG.Visibility = Visibility.Collapsed;
            }
        }

        private void Feeds_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Feeds_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
            Feeds_BG.Visibility = Visibility.Visible;
            Feeds_BG.Opacity = 1;
            selectobjectsidebar = 5;
            HideOthersSidebar();
            HandleableError usororer = new HandleableError(new ATError(418, new ErrorDetail("", "I'm a teapot")));
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(usororer);
        }

        private void Lists_MouseEnter(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 6)
            {
                Lists_BG.Visibility = Visibility.Visible;
                Lists_BG.Opacity = 0.5;
            }
        }

        private void Lists_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 6)
            {
                Lists_BG.Visibility = Visibility.Collapsed;
            }
        }

        private void Lists_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Lists_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
            Lists_BG.Visibility = Visibility.Visible;
            Lists_BG.Opacity = 1;
            selectobjectsidebar = 6;
            HideOthersSidebar();
            HandleableError usororer = new HandleableError(new ATError(418, new ErrorDetail("", "I'm a teapot")));
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(usororer);
        }

        private void Profile_MouseEnter(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 7)
            {
                Profile_BG.Visibility = Visibility.Visible;
                Profile_BG.Opacity = 0.5;
            }
        }

        private void Profile_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 7)
            {
                Profile_BG.Visibility = Visibility.Collapsed;
            }
        }

        private void Profile_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Profile_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
            Profile_BG.Visibility = Visibility.Visible;
            Profile_BG.Opacity = 1;
            selectobjectsidebar = 7;
            HideOthersSidebar();
            Profile myProfilePage = new Profile(session.Did, aTProtocol, session, this);
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(myProfilePage);
        }
        private void NavServiceOnNavigated(object sender, NavigationEventArgs args)
        {
            _ = PageFrame.NavigationService.RemoveBackEntry();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            PageFrame.NavigationService.Navigated -= NavServiceOnNavigated;
        }

        private void Settings_MouseEnter(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 8)
            {
                Settings_BG.Visibility = Visibility.Visible;
                Settings_BG.Opacity = 0.5;
            }
        }

        private void Settings_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 8)
            {
                Settings_BG.Visibility = Visibility.Collapsed;
            }
        }

        private void Settings_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Settings_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
            Settings_BG.Visibility = Visibility.Visible;
            Settings_BG.Opacity = 1;
            selectobjectsidebar = 8;
            HideOthersSidebar();
            Settings CPL = new Settings(aTProtocol, session, this);
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(CPL);
        }

        private void PFPFrame_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PFPFrame.ContextMenu.IsOpen = true;
        }

        private void Logoff(object sender, RoutedEventArgs e)
        {
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "Remember", "false");
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "RememberUsername", "");
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "RememberPassword", "");
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "RememberHost", "");
            Application.Current.Shutdown();
        }
        public void HKCU_AddKey(string path, string key, object value)
        {
            RegistryKey rk = Registry.CurrentUser.CreateSubKey(path);
            rk.SetValue(key, value);
        }

        private async void Terms_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"explorer",
                    Arguments = @"https://bsky.social/about/support/tos"
                }
            };
            _ = await Task.Run(process.Start);
            await Task.Run(process.WaitForExit);
            await Task.Run(process.Close);
            await Task.Run(process.Dispose);
        }

        private async void PrivacyPolicy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"explorer",
                    Arguments = @"https://bsky.social/about/support/privacy-policy"
                }
            };
            _ = await Task.Run(process.Start);
            await Task.Run(process.WaitForExit);
            await Task.Run(process.Close);
            await Task.Run(process.Dispose);
        }

        private async void Feedback_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"explorer",
                    Arguments = @"https://blueskyweb.zendesk.com/hc/en-us/requests/"
                }
            };
            _ = await Task.Run(process.Start);
            await Task.Run(process.WaitForExit);
            await Task.Run(process.Close);
            await Task.Run(process.Dispose);
        }
    }
}
