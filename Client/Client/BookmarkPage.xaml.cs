using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Bookmark;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Models;
using Newtonsoft.Json.Linq;

namespace Client
{
    /// <summary>
    /// Interaction logic for BookmarkPage.xaml
    /// </summary>
    public partial class BookmarkPage : Page
    {
        private readonly ATProtocol aTProtocol;
        private readonly Dashboard dashboard;
        private string cursor = "";
        private bool isLoading = false;
        private readonly List<Post> posts = new List<Post>();
        public BookmarkPage(ATProtocol aTProtocol, Dashboard dashboard)
        {
            InitializeComponent();
            this.aTProtocol = aTProtocol;
            this.dashboard = dashboard;
            // Number.Text = number.ToString() + " bookmarks";
        }
        private async void Load()
        {
            if (!isLoading)
            {
                isLoading = true;
                try
                {
                    Result<GetBookmarksOutput> test = await aTProtocol.GetBookmarksAsync(10, cursor);
                    JArray feedlist = JArray.Parse(JObject.Parse(test.Value.ToString())["bookmarks"].ToString());
                    for (int i = 0; i < feedlist.Count; i++)
                    {
                        JObject postdata = JObject.Parse("{\"post\":" + feedlist[i]["item"].ToString() + "}");
                        Post post = new Post(postdata, dashboard, aTProtocol, false, false, false);
                        _ = ReplyStack.Children.Add(post);
                        posts.Add(post);
                    }
                    if (JObject.Parse(test.Value.ToString())["cursor"] != null)
                    {
                        cursor = JObject.Parse(test.Value.ToString())["cursor"].ToString();
                    }
                    else
                    {
                        ((ScrollViewer)Content).ScrollChanged -= ScrollViewer_ScrollChanged;
                    }
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show(ex.Message);
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
            // dashboard.NavigateToPost(Uri.ToString());
        }
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (((ScrollViewer)sender).ScrollableHeight - ((ScrollViewer)sender).VerticalOffset <= 320)
            {
                Load();
            }
        }

        private void Page_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // fix
            for (int i = 0; i < posts.Count; i++)
            {
                posts[i].UnloadPost();
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
