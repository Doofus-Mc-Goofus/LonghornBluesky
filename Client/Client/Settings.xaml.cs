using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using FishyFlip;
using FishyFlip.Models;
using INI;

namespace Client
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        private readonly ATProtocol aTProtocol;
        private readonly Session session;
        private readonly Dashboard dashboard;
        private byte selectobjectsidebar;
        private bool isRoot = true;
        public Settings(ATProtocol aTProtocol, Session session, Dashboard dashboard)
        {
            InitializeComponent();
            this.session = session;
            this.aTProtocol = aTProtocol;
            this.dashboard = dashboard;
            IniFile myIni = new IniFile("config.ini");
            if (File.Exists("config.ini") && myIni.Read("ICanHasSecretBeytahFeatures", "LHbsky") == "2")
            {
                Secret.Visibility = Visibility.Visible;
            }
        }
        private void Back_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Back.Source = isRoot
                ? new BitmapImage(new Uri("pack://application:,,,/res/BackDisabled.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/BackPressed.png"));
        }

        private void Back_MouseEnter(object sender, MouseEventArgs e)
        {
            Back.Source = isRoot
                ? new BitmapImage(new Uri("pack://application:,,,/res/BackDisabled.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/BackHover.png"));
        }

        private void Back_MouseLeave(object sender, MouseEventArgs e)
        {
            Back.Source = isRoot
                ? new BitmapImage(new Uri("pack://application:,,,/res/BackDisabled.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/BackNormal.png"));
        }

        private void Back_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GoBack();
        }
        public void GoBack()
        {
            Back.Source = new BitmapImage(new Uri("pack://application:,,,/res/BackDisabled.png"));
            if (!isRoot)
            {
                isRoot = true;
                Title.Text = "Settings";
                PageFrame.Visibility = Visibility.Collapsed;
                homie.Visibility = Visibility.Visible;
            }
        }
        private void Account_MouseEnter(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 1)
            {
                Account_BG.Visibility = Visibility.Visible;
                Account_BG.Opacity = 0.5;
            }
        }

        private void Account_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 1)
            {
                Account_BG.Visibility = Visibility.Collapsed;
            }
        }

        private void Account_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Account_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
            Account_BG.Visibility = Visibility.Visible;
            Account_BG.Opacity = 1;
            selectobjectsidebar = 0;
            HideOthersSidebar();
            HandleableError usororer = new HandleableError(new ATError(418, new ErrorDetail("", "I'm a teapot")));
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(usororer);
        }

        private void Privacy_MouseEnter(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 2)
            {
                Privacy_BG.Visibility = Visibility.Visible;
                Privacy_BG.Opacity = 0.5;
            }
        }

        private void Privacy_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 2)
            {
                Privacy_BG.Visibility = Visibility.Collapsed;
            }
        }

        private void Privacy_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Privacy_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
            Privacy_BG.Visibility = Visibility.Visible;
            Privacy_BG.Opacity = 1;
            selectobjectsidebar = 0;
            HideOthersSidebar();
            HandleableError usororer = new HandleableError(new ATError(418, new ErrorDetail("", "I'm a teapot")));
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(usororer);
        }
        private void Moderation_MouseEnter(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 3)
            {
                Moderation_BG.Visibility = Visibility.Visible;
                Moderation_BG.Opacity = 0.5;
            }
        }

        private void Moderation_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 3)
            {
                Moderation_BG.Visibility = Visibility.Collapsed;
            }
        }

        private void Moderation_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Moderation_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
            Moderation_BG.Visibility = Visibility.Visible;
            Moderation_BG.Opacity = 1;
            selectobjectsidebar = 0;
            HideOthersSidebar();
            HandleableError usororer = new HandleableError(new ATError(418, new ErrorDetail("", "I'm a teapot")));
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(usororer);
        }
        private void HideOthersSidebar()
        {
            if (selectobjectsidebar != 1)
            {
                Account_BG.Visibility = Visibility.Collapsed;
                Account_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            }
            if (selectobjectsidebar != 2)
            {
                Privacy_BG.Visibility = Visibility.Collapsed;
                Privacy_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            }
            if (selectobjectsidebar != 3)
            {
                Moderation_BG.Visibility = Visibility.Collapsed;
                Moderation_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            }
            if (selectobjectsidebar != 4)
            {
                Content_BG.Visibility = Visibility.Collapsed;
                Content_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            }
            if (selectobjectsidebar != 5)
            {
                Personalization_BG.Visibility = Visibility.Collapsed;
                Personalization_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            }
            if (selectobjectsidebar != 6)
            {
                Accessibility_BG.Visibility = Visibility.Collapsed;
                Accessibility_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            }
            if (selectobjectsidebar != 7)
            {
                Language_BG.Visibility = Visibility.Collapsed;
                Language_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            }
            if (selectobjectsidebar != 8)
            {
                Help_BG.Visibility = Visibility.Collapsed;
                Help_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            }
            if (selectobjectsidebar != 9)
            {
                About_BG.Visibility = Visibility.Collapsed;
                About_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            }
        }

        private void CAM_MouseEnter(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 4)
            {
                Content_BG.Visibility = Visibility.Visible;
                Content_BG.Opacity = 0.5;
            }
        }

        private void CAM_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 4)
            {
                Content_BG.Visibility = Visibility.Collapsed;
            }
        }

        private void CAM_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Content_BG.Visibility = Visibility.Visible;
            Content_BG.Opacity = 1;
            selectobjectsidebar = 0;
            Content_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
            HideOthersSidebar();
            HandleableError usororer = new HandleableError(new ATError(418, new ErrorDetail("", "I'm a teapot")));
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(usororer);
        }

        private void Personalization_MouseEnter(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 5)
            {
                Personalization_BG.Visibility = Visibility.Visible;
                Personalization_BG.Opacity = 0.5;
            }
        }

        private void Personalization_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 5)
            {
                Personalization_BG.Visibility = Visibility.Collapsed;
            }
        }

        private void Personalization_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Personalization_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
            Personalization_BG.Visibility = Visibility.Visible;
            Personalization_BG.Opacity = 1;
            selectobjectsidebar = 0;
            HideOthersSidebar();
            isRoot = false;
            Back.Source = new BitmapImage(new Uri("pack://application:,,,/res/BackNormal.png"));
            Title.Text = "Personalization";
            PageFrame.Visibility = Visibility.Visible;
            homie.Visibility = Visibility.Collapsed;
            Personalization usororer = new Personalization(this);
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(usororer);
        }

        private void Accessibility_MouseEnter(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 6)
            {
                Accessibility_BG.Visibility = Visibility.Visible;
                Accessibility_BG.Opacity = 0.5;
            }
        }

        private void Accessibility_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 6)
            {
                Accessibility_BG.Visibility = Visibility.Collapsed;
            }
        }

        private void Accessibility_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Accessibility_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
            Accessibility_BG.Visibility = Visibility.Visible;
            Accessibility_BG.Opacity = 1;
            selectobjectsidebar = 0;
            HideOthersSidebar();
            HandleableError usororer = new HandleableError(new ATError(418, new ErrorDetail("", "I'm a teapot")));
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(usororer);
        }

        private void Language_MouseEnter(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 7)
            {
                Language_BG.Visibility = Visibility.Visible;
                Language_BG.Opacity = 0.5;
            }
        }

        private void Language_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 7)
            {
                Language_BG.Visibility = Visibility.Collapsed;
            }
        }

        private void Language_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Language_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
            Language_BG.Visibility = Visibility.Visible;
            Language_BG.Opacity = 1;
            selectobjectsidebar = 0;
            HideOthersSidebar();
            HandleableError usororer = new HandleableError(new ATError(418, new ErrorDetail("", "I'm a teapot")));
            PageFrame.NavigationService.Navigated += NavServiceOnNavigated;
            _ = PageFrame.NavigationService.Navigate(usororer);
        }

        private void Help_MouseEnter(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 8)
            {
                Help_BG.Visibility = Visibility.Visible;
                Help_BG.Opacity = 0.5;
            }
        }

        private void Help_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 8)
            {
                Help_BG.Visibility = Visibility.Collapsed;
            }
        }

        private void Help_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Help_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
            Help_BG.Visibility = Visibility.Visible;
            Help_BG.Opacity = 1;
            selectobjectsidebar = 0;
            HideOthersSidebar();
            Support sup = new Support();
            sup.Show();
        }

        private void About_MouseEnter(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 9)
            {
                About_BG.Visibility = Visibility.Visible;
                About_BG.Opacity = 0.5;
            }
        }

        private void About_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selectobjectsidebar != 9)
            {
                About_BG.Visibility = Visibility.Collapsed;
            }
        }

        private void About_MouseUp(object sender, MouseButtonEventArgs e)
        {
            About_Text.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
            About_BG.Visibility = Visibility.Visible;
            About_BG.Opacity = 1;
            selectobjectsidebar = 0;
            HideOthersSidebar();
            AboutBox aboutBox = new AboutBox();
            _ = aboutBox.ShowDialog();
        }
        private void NavServiceOnNavigated(object sender, NavigationEventArgs args)
        {
            _ = PageFrame.NavigationService.RemoveBackEntry();
            GC.Collect();
            PageFrame.NavigationService.Navigated -= NavServiceOnNavigated;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            homie.Margin = new Thickness(20, test.ActualHeight + 20, 20, 20);
            PageFrame.Margin = new Thickness(0, test.ActualHeight, 0, 0);
        }

        private void Rectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            selectobjectsidebar = 0;
            HideOthersSidebar();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // fix
            SizeChanged -= Page_SizeChanged;
            Unloaded -= Page_Unloaded;
            Back.MouseEnter -= Back_MouseEnter;
            Back.MouseLeave -= Back_MouseLeave;
            Back.MouseDown -= Back_MouseDown;
            Back.MouseUp -= Back_MouseUp;
            rect.MouseUp -= Rectangle_MouseUp;
            Account.MouseEnter -= Account_MouseEnter;
            Account.MouseLeave -= Account_MouseLeave;
            Account.MouseUp -= Account_MouseUp;
            Account.Children.Clear();
            Privacy.MouseEnter -= Privacy_MouseEnter;
            Privacy.MouseLeave -= Privacy_MouseLeave;
            Privacy.MouseUp -= Privacy_MouseUp;
            Privacy.Children.Clear();
            Moderation.MouseEnter -= Moderation_MouseEnter;
            Moderation.MouseLeave -= Moderation_MouseLeave;
            Moderation.MouseUp -= Moderation_MouseUp;
            Moderation.Children.Clear();
            Content.MouseEnter -= CAM_MouseEnter;
            Content.MouseLeave -= CAM_MouseLeave;
            Content.MouseUp -= CAM_MouseUp;
            Content.Children.Clear();
            Personalization.MouseEnter -= Personalization_MouseEnter;
            Personalization.MouseLeave -= Personalization_MouseLeave;
            Personalization.MouseUp -= Personalization_MouseUp;
            Personalization.Children.Clear();
            Accessibility.MouseEnter -= Accessibility_MouseEnter;
            Accessibility.MouseLeave -= Accessibility_MouseLeave;
            Accessibility.MouseUp -= Accessibility_MouseUp;
            Accessibility.Children.Clear();
            Language.MouseEnter -= Language_MouseEnter;
            Language.MouseLeave -= Language_MouseLeave;
            Language.MouseUp -= Language_MouseUp;
            Language.Children.Clear();
            Help.MouseEnter -= Help_MouseEnter;
            Help.MouseLeave -= Help_MouseLeave;
            Help.MouseUp -= Help_MouseUp;
            Help.Children.Clear();
            About.MouseEnter -= About_MouseEnter;
            About.MouseLeave -= About_MouseLeave;
            About.MouseUp -= About_MouseUp;
            About.Children.Clear();
            Secret.Children.Clear();
            homie.Children.Clear();
            test.Children.Clear();
            ((Grid)aegaegaegag.Content).Children.Clear();
            aegaegaegag.Content = null;
            GC.SuppressFinalize(this);
        }
    }
}
