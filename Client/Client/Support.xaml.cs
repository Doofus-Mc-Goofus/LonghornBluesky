using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Client
{
    /// <summary>
    /// Interaction logic for Support.xaml
    /// </summary>
    public partial class Support : Window
    {
        public Support()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            browse.Dispose();
        }
        private void HostProv_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HostProv.Text == string.Empty)
            {
                HostProv.Text = "Search Help";
            }
            HostProv.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void HostProv_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HostProv.Text == "Search Help")
            {
                HostProv.Text = string.Empty;
            }
            HostProv.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void Back_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Back.Source = !browse.CanGoBack
                ? new BitmapImage(new Uri("pack://application:,,,/res/BackDisabled.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/BackPressed.png"));
        }

        private void Back_MouseEnter(object sender, MouseEventArgs e)
        {
            Back.Source = !browse.CanGoBack
                ? new BitmapImage(new Uri("pack://application:,,,/res/BackDisabled.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/BackHover.png"));
        }

        private void Back_MouseLeave(object sender, MouseEventArgs e)
        {
            Back.Source = !browse.CanGoBack
                ? new BitmapImage(new Uri("pack://application:,,,/res/BackDisabled.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/BackNormal.png"));
        }

        private void Back_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (browse.CanGoBack)
            {
                browse.GoBack();
            }
            Back.Source = !browse.CanGoBack
                ? new BitmapImage(new Uri("pack://application:,,,/res/BackDisabled.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/BackHover.png"));
        }

        private void Forward_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Forward.Source = !browse.CanGoForward
? new BitmapImage(new Uri("pack://application:,,,/res/ForwardDisabled.png"))
: new BitmapImage(new Uri("pack://application:,,,/res/ForwardPressed.png"));
        }

        private void Forward_MouseEnter(object sender, MouseEventArgs e)
        {
            Forward.Source = !browse.CanGoForward
? new BitmapImage(new Uri("pack://application:,,,/res/ForwardDisabled.png"))
: new BitmapImage(new Uri("pack://application:,,,/res/ForwardHover.png"));
        }

        private void Forward_MouseLeave(object sender, MouseEventArgs e)
        {
            Forward.Source = !browse.CanGoForward
    ? new BitmapImage(new Uri("pack://application:,,,/res/ForwardDisabled.png"))
    : new BitmapImage(new Uri("pack://application:,,,/res/ForwardNormal.png"));
        }

        private void Forward_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (browse.CanGoForward)
            {
                browse.GoForward();
            }
            Forward.Source = !browse.CanGoForward
? new BitmapImage(new Uri("pack://application:,,,/res/ForwardDisabled.png"))
: new BitmapImage(new Uri("pack://application:,,,/res/ForwardHover.png"));
        }

        private void Browse_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Back.Source = !browse.CanGoBack
    ? new BitmapImage(new Uri("pack://application:,,,/res/BackDisabled.png"))
    : new BitmapImage(new Uri("pack://application:,,,/res/BackNormal.png"));
            Forward.Source = !browse.CanGoForward
? new BitmapImage(new Uri("pack://application:,,,/res/ForwardDisabled.png"))
: new BitmapImage(new Uri("pack://application:,,,/res/ForwardNormal.png"));
        }
    }
}
