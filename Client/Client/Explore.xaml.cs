using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Actor;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Lexicon.Fyi.Unravel.Frontpage;
using INI;
using Newtonsoft.Json.Linq;

namespace Client
{
    /// <summary>
    /// Interaction logic for Explore.xaml
    /// </summary>
    public partial class Explore : Page
    {
        private bool canLoad = false;
        private readonly ATProtocol aTProtocol;
        private readonly List<string> feedCursor = new List<string>();
        private readonly List<Post> posts = new List<Post>();
        private readonly List<User> users = new List<User>();
        private readonly Dashboard dashboard;
        public Explore(ATProtocol aTProtocol, Dashboard dashboard)
        {
            InitializeComponent();
            this.aTProtocol = aTProtocol;
            this.dashboard = dashboard;
            for (byte i = 0; i < 4; i++)
            {
                feedCursor.Add(null);
            }
        }

        private void Search_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Search.Text == string.Empty)
            {
                Search.Text = "Search for posts, users, or feeds";
            }
            Search.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void Search_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Search.Text == "Search for posts, users, or feeds")
            {
                Search.Text = string.Empty;
            }
            Search.Foreground = new SolidColorBrush(Colors.Black);
        }
        private async Task SearchVoid()
        {
            if (Search.Text != string.Empty && Search.Text != "Search for posts, users, or feeds")
            {
                canLoad = true;
                PostStack.Children.Clear();
                PeopleStack.Children.Clear();
                // FeedsStack.Children.Clear();
                await LoadSearch((byte)FeedTabControl.SelectedIndex);
                FeedGrid.Visibility = Visibility.Visible;
            }
        }
        private async Task LoadSearch(byte index)
        {
            if (canLoad)
            {
                canLoad = false;
                try
                {
                    if (index == 1)
                    {
                        FishyFlip.Models.Result<SearchPostsOutput> result = await aTProtocol.SearchPostsAsync(Search.Text, cursor: feedCursor[index], limit: 10);
                        JArray feedlist = JArray.Parse(JObject.Parse(result.Value.ToString())["posts"].ToString());
                        for (int i = 0; i < feedlist.Count; i++)
                        {
                            JObject postdata = JObject.Parse("{\"post\":" + feedlist[i].ToString() + "}");
                            Post post = new Post(postdata, dashboard, aTProtocol, false, false, false);
                            _ = ((StackPanel)((TabItem)FeedTabControl.Items[index]).Content).Children.Add(post);
                            posts.Add(post);
                        }
                        if (JObject.Parse(result.Value.ToString())["cursor"] != null)
                        {
                            feedCursor[index] = JObject.Parse(result.Value.ToString())["cursor"].ToString();
                        }
                    }
                    else if (index == 2)
                    {
                        var result = await aTProtocol.SearchActorsAsync(Search.Text, cursor: feedCursor[index], limit: 10);
                        JArray feedlist = JArray.Parse(JObject.Parse(result.Value.ToString())["actors"].ToString());
                        for (int i = 0; i < feedlist.Count; i++)
                        {
                            JObject postdata = JObject.Parse(feedlist[i].ToString());
                            User post = new User(postdata, dashboard, aTProtocol);
                            _ = ((StackPanel)((TabItem)FeedTabControl.Items[index]).Content).Children.Add(post);
                            users.Add(post);
                        }
                        if (JObject.Parse(result.Value.ToString())["cursor"] != null)
                        {
                            feedCursor[index] = JObject.Parse(result.Value.ToString())["cursor"].ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        _ = MessageBox.Show(ex.Message.ToString() + "\n" + ex.InnerException.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch
                    {
                        _ = MessageBox.Show(ex.Message.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                canLoad = true;
            }
        }
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (((ScrollViewer)sender).ScrollableHeight - ((ScrollViewer)sender).VerticalOffset <= 320)
            {
                _ = LoadSearch((byte)FeedTabControl.SelectedIndex);
            }
        }
        private void PassBoxButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PassBoxButton.Source = new BitmapImage(new Uri("pack://application:,,,/res/signinbutton-3.png"));
        }

        private void PassBoxButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            PassBoxButton.Source = new BitmapImage(new Uri("pack://application:,,,/res/signinbutton-2.png"));
        }

        private void PassBoxButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            PassBoxButton.Source = new BitmapImage(new Uri("pack://application:,,,/res/signinbutton-1.png"));
        }

        private void PassBoxButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PassBoxButton.Source = new BitmapImage(new Uri("pack://application:,,,/res/signinbutton-1.png"));
            _ = SearchVoid();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < posts.Count; i++)
            {
                posts[i].UnloadPost();
            }
            for (int i = 0; i < users.Count; i++)
            {
                users[i].UnloadUser();
            }
            Unloaded -= Page_Unloaded;
            Search.LostKeyboardFocus -= Search_LostKeyboardFocus;
            Search.GotKeyboardFocus -= Search_GotKeyboardFocus;
            PassBoxButton.MouseDown -= PassBoxButton_MouseDown;
            PassBoxButton.MouseEnter -= PassBoxButton_MouseEnter;
            PassBoxButton.MouseLeave -= PassBoxButton_MouseLeave;
            PassBoxButton.MouseUp -= PassBoxButton_MouseUp;
            PostStack.Children.Clear();
            PeopleStack.Children.Clear();
            FeedsStack.Children.Clear();
            FeedTabControl.Items.Clear();
            FeedGrid.Children.Clear();
            SearchGrid.Children.Clear();
            ((ScrollViewer)Content).Content = null;
            Content = null;
        }
    }
}
