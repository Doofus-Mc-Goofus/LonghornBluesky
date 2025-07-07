using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Models;
using Newtonsoft.Json.Linq;

namespace Client
{
    /// <summary>
    /// Interaction logic for PostPage.xaml
    /// </summary>
    public partial class PostPage : Page
    {
        private readonly ATProtocol aTProtocol;
        private readonly string uri;
        private readonly Dashboard dashboard;
        public PostPage(string uri, ATProtocol aTProtocol, Dashboard dashboard)
        {
            InitializeComponent();
            this.uri = uri;
            this.aTProtocol = aTProtocol;
            this.dashboard = dashboard;
            _ = Load();
        }

        private async Task Load()
        {
            Result<GetPostThreadOutput> thread = await aTProtocol.GetPostThreadAsync(ATUri.Create(uri), 6, 10);
            Post post = new Post(JObject.Parse(JObject.Parse(thread.Value.ToString())["thread"].ToString()), dashboard, aTProtocol, true, false, false);
            _ = PostGrid.Children.Add(post);
            if (JObject.Parse(thread.Value.ToString())["thread"]["replies"] != null)
            {
                JArray pingas = JArray.Parse(JObject.Parse(thread.Value.ToString())["thread"]["replies"].ToString());
                if (pingas.Count > 0)
                {
                    FeedGrid.Visibility = Visibility.Visible;
                    for (int i = 0; i < pingas.Count; i++)
                    {
                        Post reply = new Post(JObject.Parse(pingas[i].ToString()), dashboard, aTProtocol, false, false, false);
                        _ = ReplyStack.Children.Add(reply);
                    }
                }
            }
            GC.Collect();
        }
        private void Swaus(object sender, EventArgs e)
        {
            FeedGrid.Margin = new Thickness(0, PostStack.ActualHeight, 0, 0);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= Page_Unloaded;
            PostStack.LayoutUpdated -= Swaus;
            ReplyStack.Children.Clear();
            FeedTabControl.Items.Clear();
            FeedGrid.Children.Clear();
            PostGrid.Children.Clear();
            PostStack.Children.Clear();
            ((ScrollViewer)Content).Content = null;
        }
    }
}
