using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Embed;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Models;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace Client
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        private bool canEdit = false;
        private bool stillLoading = false;
        private readonly JArray feeds;
        private readonly ATProtocol aTProtocol;
        private readonly Dashboard dashboard;
        private readonly List<string> feedCursor = new List<string>();
        private readonly List<string> feedUriString = new List<string>();
        private readonly List<Post> posts = new List<Post>();
        private readonly List<ScrollViewer> scrollViewers = new List<ScrollViewer>();
        private readonly WhatsUpControl whatsUpControl;

        public Home(JArray feeds, ATProtocol aTProtocol, Dashboard dashboard)
        {
            InitializeComponent();
            this.feeds = feeds;
            this.aTProtocol = aTProtocol;
            this.dashboard = dashboard;
            feedCursor.Add(string.Empty);
            feedUriString.Add(string.Empty);
            whatsUpControl = new WhatsUpControl(aTProtocol, dashboard);
            _ = WhatsUpGrid.Children.Add(whatsUpControl);
            _ = Load();
        }
        public void Transfer(EmbedRecord quotePost)
        {
            whatsUpControl.Transfer(quotePost);
        }
        private async Task Load()
        {
            for (int i = 0; i < feeds.Count; i++)
            {
                if (bool.Parse(feeds[i].SelectToken("pinned").ToString()))
                {
                    if (feeds[i].SelectToken("type").ToString() == "timeline")
                    {
                        TabItem item = new TabItem
                        {
                            Header = "Following",
                            Padding = new Thickness(10, 2, 10, 4),
                            Margin = new Thickness(-2, -2, -2, 0),
                            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9FBDD2")),
                            Content = new StackPanel()
                        };
                        // feed me
                        Grid feedgrid = new Grid();
                        StackPanel stacker = new StackPanel
                        {
                            VerticalAlignment = VerticalAlignment.Top
                        };
                        ScrollViewer scrollViewer = new ScrollViewer
                        {
                            Content = stacker,
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                        };
                        scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                        scrollViewers.Add(scrollViewer);
                        _ = feedgrid.Children.Add(scrollViewer);
                        item.Tag = stacker;
                        item.Content = feedgrid;
                        _ = FeedTabControl.Items.Add(item);
                    }
                    else
                    {
                        Result<GetFeedGeneratorOutput> result = await aTProtocol.GetFeedGeneratorAsync(ATUri.Create(feeds[i].SelectToken("value").ToString()));
                        result.Switch(
                        success =>
                        {
                            TabItem item = new TabItem();
                            JObject obj = JObject.Parse(result.Value.ToString());
                            JObject obj2 = JObject.Parse(obj["view"].ToString());
                            item.Header = obj2["displayName"];
                            // Below causes everything inside it to have the tooltip, so removed.
                            // item.ToolTip = obj2["description"];
                            item.Padding = new Thickness(10, 2, 10, 4);
                            item.Margin = new Thickness(-2, -2, -2, 0);
                            item.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9FBDD2"));
                            // feed me
                            Grid feedgrid = new Grid();
                            StackPanel stacker = new StackPanel
                            {
                                VerticalAlignment = VerticalAlignment.Top
                            };
                            ScrollViewer scrollViewer = new ScrollViewer
                            {
                                Content = stacker,
                                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                            };
                            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                            scrollViewers.Add(scrollViewer);
                            _ = feedgrid.Children.Add(scrollViewer);
                            item.Tag = stacker;
                            item.Content = feedgrid;
                            _ = FeedTabControl.Items.Add(item);
                        },
                        error =>
                        {

                        }
                        );
                    }
                }
                feedCursor.Add(string.Empty);
                feedUriString.Add(feeds[i].SelectToken("value").ToString());
            }
            canEdit = true;
            // Old code (DO NOT UNCOMMENT!)
            // FeedTabControl.SelectionChanged += (s, ee) => LoadFeed((StackPanel)((TabItem)FeedTabControl.SelectedItem).Tag, false);
            if (HKCU_GetString(@"SOFTWARE\LonghornBluesky", "selectedIndex") != null)
            {
                FeedTabControl.SelectedIndex = int.Parse(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "selectedIndex"));
            }
            else
            {
                FeedTabControl.SelectedIndex = 1;
            } 
        }

        private async Task LoadFeed(StackPanel stackPanel)
        {
            if (!stillLoading)
            {
                stillLoading = true;
                ATUri AtUri;
                JArray feedlist;
                int numb = FeedTabControl.SelectedIndex;
                try
                {
                    if (feedUriString[numb].StartsWith("at"))
                    {
                        AtUri = ATUri.Create(feedUriString[numb]);
                        Result<GetFeedOutput> test = await aTProtocol.GetFeedAsync(AtUri, 10, feedCursor[numb]);
                        test.Switch(
                            success =>
                            {
                                feedlist = JArray.Parse(JObject.Parse(test.Value.ToString())["feed"].ToString());
                                for (int i = 0; i < feedlist.Count; i++)
                                {
                                    JObject postdata = JObject.Parse(feedlist[i].ToString());
                                    Post post = new Post(postdata, dashboard, aTProtocol, false, false, false);
                                    _ = stackPanel.Children.Add(post);
                                    posts.Add(post);
                                }
                                // MessageBox.Show(feedCursor);
                                // I think i'd be a lot happier if I didn't force myself to work on this constantly.
                                // It's not that I hate this project, it's just that I've spent 98% of the day working on it alone.
                                // My fault for teasing this project when all there was was the ability to log in and a half finished dashboard.
                                // MessageBox.Show(JObject.Parse(test.Value.ToString())["cursor"].ToString());
                                feedCursor[numb] = JObject.Parse(test.Value.ToString())["cursor"].ToString();
                            },
                            error =>
                            {
                                _ = MessageBox.Show($"Error: {error.StatusCode} {error.Detail}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        );
                    }
                    else
                    {
                        Result<GetTimelineOutput> test = await aTProtocol.GetTimelineAsync(feedUriString[numb], 10, feedCursor[numb]);
                        test.Switch(
                            success =>
                            {
                                feedlist = JArray.Parse(JObject.Parse(test.Value.ToString())["feed"].ToString());
                                // System.Windows.Forms.Clipboard.SetText(feedlist.ToString());
                                for (int i = 0; i < feedlist.Count; i++)
                                {
                                    JObject postdata = JObject.Parse(feedlist[i].ToString());
                                    Post post = new Post(postdata, dashboard, aTProtocol, false, false, false);
                                    _ = stackPanel.Children.Add(post);
                                    posts.Add(post);
                                }
                                if (feedlist.Count > 0)
                                {
                                    DateTime dateTime = feedlist[feedlist.Count - 1]["reason"] != null
                                        ? (DateTime)feedlist[feedlist.Count - 1]["reason"]["indexedAt"]
                                        : (DateTime)feedlist[feedlist.Count - 1]["post"]["record"]["createdAt"];
                                    feedCursor[numb] = dateTime.ToString("yyyy-MM-dd") + "T" + dateTime.ToString("HH:mm:ss") + "Z";
                                }
                            },
                            error =>
                            {
                                _ = MessageBox.Show($"Error: {error.StatusCode} {error.Detail}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            });
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
                stillLoading = false;
                // lazy
                GC.Collect();
            }
        }

        private void Rectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _ = WhatsUpGrid.Focus();
        }

        private void WhatsUpGrid_LayoutUpdated(object sender, EventArgs e)
        {
            if (canEdit)
            {
                FeedGrid.Margin = new Thickness(0, wrapper.ActualHeight, 0, 20);
            }
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            ((MediaPlayer)sender).Close();
        }

        private void MediaPlayer_MediaFailed(object sender, ExceptionEventArgs e)
        {
            ((MediaPlayer)sender).Close();
        }
        public string HKCU_GetString(string path, string key)
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(path);
                return rk == null ? string.Empty : (string)rk.GetValue(key);
            }
            catch { return string.Empty; }
        }

        public System.IO.MemoryStream ImageSourceToMemoryStream(BitmapEncoder BitEncoder, ImageSource ImgSource)
        {
            System.IO.MemoryStream Stream = null;
            switch ((ImgSource as BitmapSource) != null)
            {
                case true:
                    BitEncoder.Frames.Add(BitmapFrame.Create(ImgSource as BitmapSource));
                    Stream = new System.IO.MemoryStream();
                    BitEncoder.Save(Stream);
                    break;
            }
            return Stream;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (((ScrollViewer)sender).ScrollableHeight - ((ScrollViewer)sender).VerticalOffset <= 320)
            {
                _ = LoadFeed((StackPanel)((ScrollViewer)sender).Content);
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < posts.Count; i++)
            {
                posts[i].UnloadPost();
            }
            for (int i = 0; i < scrollViewers.Count; i++)
            {
                scrollViewers[i].ScrollChanged -= ScrollViewer_ScrollChanged;
            }
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "selectedIndex", FeedTabControl.SelectedIndex.ToString());
            Unloaded -= Page_Unloaded;
            WhatsUpGrid.Children.Clear();
            rect.MouseUp -= Rectangle_MouseUp;
            FeedTabControl.Items.Clear();
            FeedGrid.Children.Clear();
            wrapper.Children.Clear();
            grid.Children.Clear();
            GC.SuppressFinalize(this);
        }
        public void HKCU_AddKey(string path, string key, object value)
        {
            RegistryKey rk = Registry.CurrentUser.CreateSubKey(path);
            rk.SetValue(key, value);
        }
    }
}
