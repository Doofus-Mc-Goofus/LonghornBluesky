using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FishyFlip;
using FishyFlip.Lexicon;
using FishyFlip.Lexicon.Com.Atproto.Moderation;
using FishyFlip.Models;

namespace Client
{
    /// <summary>
    /// Interaction logic for ReportModeration.xaml
    /// </summary>
    public partial class ReportModeration : Window
    {
        private readonly ATObject aTObject;
        private readonly ATProtocol aTProtocol;
        public ReportModeration(ATObject aTObject, string data, ATProtocol aTProtocol)
        {
            InitializeComponent();
            this.aTProtocol = aTProtocol;
            this.aTObject = aTObject;
            switch (aTObject.Type)
            {
                case "app.bsky.feed.post":
                    Description.Text = "You have chosen to submit a report regarding the post \"" + data + "\". Please enter the details below and submit the report to us.";
                    break;
                case "app.bsky.actor.profile":
                    Description.Text = "You have chosen to submit a report regarding the user \"" + data + "\". Please enter the details below and submit the report to us.";
                    break;
                default:
                    Description.Text = "You have chosen to submit a report regarding the UNKNOWN \"" + data + "\". Please enter the details below and submit the report to us.";
                    break;
            }
        }

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (Reason.SelectedIndex != -1)
            {
                MainPage.Visibility = Visibility.Collapsed;
                SubmittingPage.Visibility = Visibility.Visible;
                Result<CreateReportOutput> result = await aTProtocol.CreateReportAsync((string)((ComboBoxItem)Reason.SelectedItem).Tag, aTObject, Why.Text);
                result.Switch(
                    success =>
                    {
                        Close();
                    },
                    error =>
                    {
                        _ = MessageBox.Show(error.Detail.Message + " (" + error.StatusCode + ")");
                        MainPage.Visibility = Visibility.Collapsed;
                        SubmittingPage.Visibility = Visibility.Visible;
                    });
            }
            else
            {
                Error.Content = "Select a reason to why this should be reviewed";
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HostProv_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Why.Text == "Why should this user be reviewed?")
            {
                Why.Text = string.Empty;
            }
            Why.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void HostProv_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Why.Text == string.Empty)
            {
                Why.Text = "Why should this user be reviewed?";
            }
            Why.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _ = BG.Focus();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Submit.Click -= Submit_Click;
            Cancel.Click -= Cancel_Click;
            Why.LostKeyboardFocus -= HostProv_LostKeyboardFocus;
            Why.GotKeyboardFocus -= HostProv_GotKeyboardFocus;
            Closing -= Window_Closing;
            BG.MouseDown -= Image_MouseDown;
        }
    }
}
