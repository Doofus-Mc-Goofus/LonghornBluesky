using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Actor;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Lexicon.App.Bsky.Unspecced;
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
            Search.TextChanged += Search_TextChanged;
            _ = Load();
        }
        private async Task Load()
        {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            FishyFlip.Models.Result<GetTrendingTopicsOutput> result = await aTProtocol.GetTrendingTopicsAsync();
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                               // System.Windows.Forms.Clipboard.SetText(result.Value.ToString());
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
                if (Search.Text[0].ToString() == "a" && Search.Text[1].ToString() == "t" && Search.Text[2].ToString() == ":" && Search.Text[3].ToString() == "/" && Search.Text[4].ToString() == "/")
                {
                    if (Search.Text.Contains("/app.bsky.feed.post/"))
                    {
                        dashboard.NavigateToPost(Search.Text);
                    }
                    else
                    {

                    }
                }
                else
                {
                    canLoad = true;
                    PostStack.Children.Clear();
                    PeopleStack.Children.Clear();
                    // FeedsStack.Children.Clear();
                    TrendingGrid.Visibility = Visibility.Collapsed;
                    await LoadSearch((byte)FeedTabControl.SelectedIndex);
                    FeedGrid.Visibility = Visibility.Visible;
                }
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
                        FishyFlip.Models.Result<SearchActorsOutput> result = await aTProtocol.SearchActorsAsync(Search.Text, cursor: feedCursor[index], limit: 10);
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

        private void PassBoxButton_MouseEnter(object sender, MouseEventArgs e)
        {
            PassBoxButton.Source = new BitmapImage(new Uri("pack://application:,,,/res/signinbutton-2.png"));
        }

        private void PassBoxButton_MouseLeave(object sender, MouseEventArgs e)
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
            Search.TextChanged -= Search_TextChanged;
            PassBoxButton.MouseDown -= PassBoxButton_MouseDown;
            PassBoxButton.MouseEnter -= PassBoxButton_MouseEnter;
            PassBoxButton.MouseLeave -= PassBoxButton_MouseLeave;
            PassBoxButton.MouseUp -= PassBoxButton_MouseUp;
            ClearButton.MouseDown -= ClearButton_MouseDown;
            ClearButton.MouseEnter -= ClearButton_MouseEnter;
            ClearButton.MouseLeave -= ClearButton_MouseLeave;
            ClearButton.MouseUp -= ClearButton_MouseUp;
            PostStack.Children.Clear();
            PeopleStack.Children.Clear();
            FeedsStack.Children.Clear();
            FeedTabControl.Items.Clear();
            FeedGrid.Children.Clear();
            SearchGrid.Children.Clear();
            ((ScrollViewer)Content).Content = null;
            Content = null;
            GC.SuppressFinalize(this);
        }

        private void ClearButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ClearButton.Source = new BitmapImage(new Uri("pack://application:,,,/res/ClosePressed.png"));
        }

        private void ClearButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ClearButton.Source = new BitmapImage(new Uri("pack://application:,,,/res/CloseHover.png"));
        }

        private void ClearButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ClearButton.Source = new BitmapImage(new Uri("pack://application:,,,/res/CloseNormal.png"));
        }

        private void ClearButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ClearButton.Source = new BitmapImage(new Uri("pack://application:,,,/res/CloseHover.png"));
            canLoad = false;
            PostStack.Children.Clear();
            PeopleStack.Children.Clear();
            FeedsStack.Children.Clear();
            FeedGrid.Visibility = Visibility.Collapsed;
            TrendingGrid.Visibility = Visibility.Visible;
            Search.Text = "Search for posts, users, or feeds";
            Search.Foreground = new SolidColorBrush(Colors.Gray);
            _ = rect.Focus();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearButton.Visibility = Search.Text == string.Empty || Search.Text == "Search for posts, users, or feeds" ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
