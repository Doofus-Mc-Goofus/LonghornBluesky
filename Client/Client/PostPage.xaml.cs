using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Lexicon.Com.Atproto.Repo;
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
        private readonly List<Post> posts = new List<Post>();
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
            posts.Add(post);
            WhatsUpControl whatsUpControl = new WhatsUpControl(aTProtocol, dashboard)
            {
                Margin = new Thickness(0, 20, 0, -6)
            };
            ReplyRefDef replyRefDef = new ReplyRefDef();
            switch (JObject.Parse(JObject.Parse(thread.Value.ToString())["thread"].ToString())["post"]["record"]["reply"])
            {
                case null:
                    replyRefDef.Parent = StrongRef.FromJson(JObject.Parse(JObject.Parse(thread.Value.ToString())["thread"].ToString())["post"].ToString());
                    replyRefDef.Root = StrongRef.FromJson(JObject.Parse(JObject.Parse(thread.Value.ToString())["thread"].ToString())["post"].ToString());
                    break;
                default:
                    replyRefDef.Parent = StrongRef.FromJson(JObject.Parse(JObject.Parse(thread.Value.ToString())["thread"].ToString())["post"].ToString());
                    replyRefDef.Root = StrongRef.FromJson(JObject.Parse(JObject.Parse(thread.Value.ToString())["thread"].ToString())["post"]["record"]["reply"]["root"].ToString());
                    break;
            }
            whatsUpControl.ReplyTrans(replyRefDef);
            _ = Stackie.Children.Add(whatsUpControl);
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
                        posts.Add(reply);
                    }
                }
            }
            GC.Collect();
        }
        private void Swaus(object sender, EventArgs e)
        {
            FeedGrid.Margin = new Thickness(0, Stackie.ActualHeight, 0, 0);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < posts.Count; i++)
            {
                posts[i].UnloadPost();
            }
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
