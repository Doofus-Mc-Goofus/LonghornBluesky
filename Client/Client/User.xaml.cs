using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Graph;
using FishyFlip.Lexicon.Com.Atproto.Repo;
using FishyFlip.Models;
using Newtonsoft.Json.Linq;

namespace Client
{
    /// <summary>
    /// Interaction logic for User.xaml
    /// </summary>
    public partial class User : UserControl
    {
        private readonly Dashboard dashboard;
        private readonly JObject profile;
        private readonly ATProtocol aTProtocol;
        private readonly ATDid ATDid;
        private ATUri followUri;
        private bool isFollowing = false;
#pragma warning disable IDE0044 // Add readonly modifier
        private bool isbeingFollowed = false;
#pragma warning restore IDE0044 // Add readonly modifier
        public User(JObject profile, Dashboard dashboard, ATProtocol aTProtocol)
        {
            InitializeComponent();
            this.dashboard = dashboard;
            this.profile = profile;
            this.aTProtocol = aTProtocol;
            _ = Load();
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task Load()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            try
            {
                Username.Text = profile["displayName"].ToString();
                Username.ToolTip = profile["displayName"].ToString();
            }
            catch
            {

            }
            Fullname.Text = "@" + profile["handle"].ToString();
            Fullname.ToolTip = "@" + profile["handle"].ToString();
            try
            {
                Bio.Text = profile["description"].ToString();
            }
            catch
            {
            }
            try
            {
                PFP.Source = new BitmapImage(new Uri(profile["avatar"].ToString()));
            }
            catch
            {

            }
        }
        private void SelectPost_MouseEnter(object sender, MouseEventArgs e)
        {
            SelectPost.Opacity = 100;
        }
        private void SelectPost_MouseLeave(object sender, MouseEventArgs e)
        {
            SelectPost.Opacity = 0;
        }
        private void SelectPost_MouseUp(object sender, MouseButtonEventArgs e)
        {
            dashboard.NavigateToProfile(profile["did"].ToString());
        }
        private void Label_MouseEnter(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).TextDecorations.Add(TextDecorations.Underline);
        }
        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).TextDecorations.Clear();
        }
        private void Username_MouseUp(object sender, MouseButtonEventArgs e)
        {
            dashboard.NavigateToProfile(((TextBlock)sender).Tag.ToString());
        }
        private async void Follow_Click(object sender, RoutedEventArgs e)
        {
            if (isFollowing)
            {
                Result<DeleteRecordOutput> result = await aTProtocol.DeleteFollowAsync(followUri.Did, followUri.Rkey);
                result.Switch(
                    success =>
                    {
                        Follow.Content = isbeingFollowed ? "Follow Back" : "Follow";
                        isFollowing = false;
                    },
                    error =>
                    {
                        _ = MessageBox.Show($"Error: {error.StatusCode} {error.Detail}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
            }
            else
            {
                Follow follow = new Follow
                {
                    CreatedAt = DateTime.Now,
                    Subject = ATDid
                };
                Result<CreateRecordOutput> result = await aTProtocol.CreateFollowAsync(follow);
                result.Switch(
                    success =>
                    {
                        Follow.Content = "Following";
                        isFollowing = true;
                        followUri = success.Uri;
                    },
                    error =>
                    {
                        _ = MessageBox.Show($"Error: {error.StatusCode} {error.Detail}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
            }
        }

        public void UnloadUser()
        {
            // fix
            SelectPost.MouseEnter -= SelectPost_MouseEnter;
            SelectPost.MouseLeave -= SelectPost_MouseLeave;
            SelectPost.MouseUp -= SelectPost_MouseUp;
            Username.MouseEnter -= SelectPost_MouseEnter;
            Username.MouseLeave -= SelectPost_MouseLeave;
            Username.MouseUp -= SelectPost_MouseUp;
            Fullname.MouseEnter -= SelectPost_MouseEnter;
            Fullname.MouseLeave -= SelectPost_MouseLeave;
            Fullname.MouseUp -= SelectPost_MouseUp;
            Follow.Click -= Follow_Click;
            Bio.MouseEnter -= SelectPost_MouseEnter;
            Bio.MouseLeave -= SelectPost_MouseLeave;
            Bio.MouseUp -= SelectPost_MouseUp;
            ((Grid)Content).Children.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
