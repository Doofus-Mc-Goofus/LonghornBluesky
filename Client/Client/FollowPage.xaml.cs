using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FishyFlip;
using FishyFlip.Lexicon;
using FishyFlip.Lexicon.App.Bsky.Feed;
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
        private readonly ATDid Did;
        private readonly ATUri Uri;
        private readonly ATProtocol aTProtocol;
        private readonly byte isBy;
        private readonly Dashboard dashboard;
        private string cursor = "";
        private bool isLoading = false;
        private readonly List<User> users = new List<User>();
        public FollowPage(ATProtocol aTProtocol, byte isBy, Dashboard dashboard, int number, ATDid Did = null, ATUri Uri = null)
        {
            InitializeComponent();
            this.Did = Did;
            this.Uri = Uri;
            this.aTProtocol = aTProtocol;
            this.isBy = isBy;
            this.dashboard = dashboard;
            if (isBy == 0)
            {
                Number.Text = number.ToString() + " followers";
            }
            else if (isBy == 1)
            {
                Number.Text = number.ToString() + " following";
            }
            else if (isBy == 2)
            {
                Number.Text = number.ToString() + " reposts";
            }
            else if (isBy == 3)
            {
                Number.Text = number.ToString() + " likes";
            }
            else
            {
                Number.Text = number.ToString();
            }
        }
        private async Task Load()
        {
            if (!isLoading)
            {
                isLoading = true;
                if (isBy == 0)
                {
                    Result<GetFollowersOutput> test = await aTProtocol.GetFollowersAsync(Did, 10, cursor);
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
                else if (isBy == 1)
                {
                    Result<GetFollowsOutput> test = await aTProtocol.GetFollowsAsync(Did, 10, cursor);
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
                else if (isBy == 2)
                {
                    Result<GetRepostedByOutput> test = await aTProtocol.GetRepostedByAsync(Uri, null, 10, cursor);
                    JObject bingus = JObject.Parse(test.Value.ToString());
                    System.Windows.Forms.Clipboard.SetText(bingus.ToString());
                    Username.Text = "Reposts";
                    for (int i = 0; i < JArray.Parse(bingus["repostedBy"].ToString()).Count; i++)
                    {
                        // fix this
                        User user = new User(JObject.Parse(bingus["repostedBy"][i].ToString()), dashboard, aTProtocol);
                        _ = ReplyStack.Children.Add(user);
                        users.Add(user);
                    }
                    if (bingus["cursor"] != null)
                    {
                        cursor = bingus["cursor"].ToString();
                    }
                    else
                    {
                        ((ScrollViewer)Content).ScrollChanged -= ScrollViewer_ScrollChanged;
                    }
                }
                else if (isBy == 3)
                {
                    Result<GetLikesOutput> test = await aTProtocol.GetLikesAsync(Uri, null, 10, cursor);
                    JObject bingus = JObject.Parse(test.Value.ToString());
                    Username.Text = "Likes";
                    for (int i = 0; i < JArray.Parse(bingus["likes"].ToString()).Count; i++)
                    {
                        // fix this
                        User user = new User(JObject.Parse(bingus["likes"][i]["actor"].ToString()), dashboard, aTProtocol);
                        _ = ReplyStack.Children.Add(user);
                        users.Add(user);
                    }
                    if (JObject.Parse(test.Value.ToString())["cursor"] != null)
                    {
                        DateTime dateTime = (DateTime)JObject.Parse(test.Value.ToString())["cursor"];
                        cursor = dateTime.ToString("yyyy-MM-dd") + "T" + dateTime.ToString("HH:mm:ss") + "Z";
                    }
                    else
                    {
                        ((ScrollViewer)Content).ScrollChanged -= ScrollViewer_ScrollChanged;
                    }
                }
                else
                {

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
            if (Did != null)
            {
                dashboard.NavigateToProfile(Did.ToString());
            }
            else
            {
                dashboard.NavigateToPost(Uri.ToString());
            }
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
