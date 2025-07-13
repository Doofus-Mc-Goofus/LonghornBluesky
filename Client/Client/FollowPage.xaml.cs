using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Graph;
using FishyFlip.Models;
using Newtonsoft.Json.Linq;

namespace Client
{
    /// <summary>
    /// Interaction logic for FollowPage.xaml
    /// </summary>
    public partial class FollowPage : Page
    {
        private readonly ATDid Uri;
        private readonly ATProtocol aTProtocol;
        private readonly bool isBy;
        private readonly Dashboard dashboard;
        private string cursor = "";
        private bool isLoading = false;
        private readonly List<User> users = new List<User>();
        public FollowPage(ATDid Uri, ATProtocol aTProtocol, bool isBy, Dashboard dashboard, int number)
        {
            InitializeComponent();
            this.Uri = Uri;
            this.aTProtocol = aTProtocol;
            this.isBy = isBy;
            this.dashboard = dashboard;
            Number.Text = isBy ? number.ToString() + " following" : number.ToString() + " followers";
        }
        private async Task Load()
        {
            if (!isLoading)
            {
                isLoading = true;
                if (isBy)
                {
                    Result<GetFollowsOutput> test = await aTProtocol.GetFollowsAsync(Uri, 10, cursor);
                    JObject bingus = JObject.Parse(test.Value.ToString());
                    Username.Text = bingus["subject"]["displayName"].ToString();
                    for (int i = 0; i < JArray.Parse(bingus["follows"].ToString()).Count; i++)
                    {
                        // fix this
                        User user = new User(JObject.Parse(bingus["follows"][i].ToString()), dashboard, aTProtocol);
                        _ = ReplyStack.Children.Add(user);
                        users.Add(user);
                    }
                    cursor = bingus["cursor"].ToString();
                }
                else
                {
                    Result<GetFollowersOutput> test = await aTProtocol.GetFollowersAsync(Uri, 10, cursor);
                    JObject bingus = JObject.Parse(test.Value.ToString());
                    Username.Text = bingus["subject"]["displayName"].ToString();
                    for (int i = 0; i < JArray.Parse(bingus["followers"].ToString()).Count; i++)
                    {
                        User user = new User(JObject.Parse(bingus["followers"][i].ToString()), dashboard, aTProtocol);
                        _ = ReplyStack.Children.Add(user);
                        users.Add(user);
                    }
                    cursor = bingus["cursor"].ToString();
                }
                isLoading = false;
            }
        }
        private void Back_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Back.Source = new BitmapImage(new Uri("pack://application:,,,/res/BackPressed.png"));
        }

        private void Back_MouseEnter(object sender, MouseEventArgs e)
        {
            Back.Source = new BitmapImage(new Uri("pack://application:,,,/res/BackHover.png"));
        }

        private void Back_MouseLeave(object sender, MouseEventArgs e)
        {
            Back.Source = new BitmapImage(new Uri("pack://application:,,,/res/BackNormal.png"));
        }

        private void Back_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Back.Source = new BitmapImage(new Uri("pack://application:,,,/res/BackHover.png"));
            dashboard.NavigateToProfile(Uri.ToString());
        }
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (((ScrollViewer)sender).ScrollableHeight - ((ScrollViewer)sender).VerticalOffset <= 320)
            {
                _ = Load();
            }
        }

        private void Page_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // fix
            for (int i = 0; i < users.Count; i++)
            {
                users[i].UnloadUser();
            }
            Unloaded -= Page_Unloaded;
            ReplyStack.Children.Clear();
            FeedTabControl.Items.Clear();
            FeedGrid.Children.Clear();
            PostGrid.Children.Clear();
            ((ScrollViewer)Content).Content = null;
            GC.SuppressFinalize(this);
        }
    }
}
