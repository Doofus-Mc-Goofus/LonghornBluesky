using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using FishyFlip;
using FishyFlip.Lexicon;
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
        private bool canUpload = true;
        private bool stillLoading = false;
        private readonly JArray feeds;
        private readonly ATProtocol aTProtocol;
        private readonly Dashboard dashboard;
        private readonly List<FishyFlip.Lexicon.App.Bsky.Embed.Image> attachedImages = new List<FishyFlip.Lexicon.App.Bsky.Embed.Image>();
        private FishyFlip.Lexicon.App.Bsky.Embed.EmbedVideo attachedVideo;
        private readonly List<string> feedCursor = new List<string>();
        private readonly List<string> feedUriString = new List<string>();
        private EmbedRecord quotePost = null;
        public Home(JArray feeds, ATProtocol aTProtocol, Dashboard dashboard)
        {
            InitializeComponent();
            this.feeds = feeds;
            this.aTProtocol = aTProtocol;
            this.dashboard = dashboard;
            WhatsUpImages.Children.Clear();
            feedCursor.Add(string.Empty);
            feedUriString.Add(string.Empty);
            _ = Load();
        }
        public async void Transfer(EmbedRecord quotePost)
        {
            this.quotePost = quotePost;
            Result<GetPostThreadOutput> thread = await aTProtocol.GetPostThreadAsync(quotePost.Record.Uri);
            JToken post = JObject.Parse(thread.Value.ToString())["thread"];
            Quote.Visibility = Visibility.Visible;
            QuoteUsername.Text = post["post"]["author"]["displayName"].ToString();
            QuoteUsername.Tag = post["post"]["author"]["did"].ToString();
            QuoteText.Text = post["post"]["record"]["text"].ToString();
            try
            {
                QuotePFP.Source = new BitmapImage(new Uri(post["post"]["author"]["avatar"].ToString()));
            }
            catch
            {

            }
            ClearPost.Visibility = Visibility.Visible;
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
                        scrollViewer.Unloaded += ScrollViewer_Unloaded;
                        feedgrid.Children.Add(scrollViewer);
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
                            scrollViewer.Unloaded += ScrollViewer_Unloaded;
                            feedgrid.Children.Add(scrollViewer);
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
                feedCursor.Add("");
                feedUriString.Add(feeds[i].SelectToken("value").ToString());
            }
            canEdit = true;
            // Old code (DO NOT UNCOMMENT!)
            // FeedTabControl.SelectionChanged += (s, ee) => LoadFeed((StackPanel)((TabItem)FeedTabControl.SelectedItem).Tag, false);
            FeedTabControl.SelectedIndex = 1;
        }

        private void ScrollViewer_Unloaded(object sender, RoutedEventArgs e)
        {
            ((ScrollViewer)sender).ScrollChanged -= ScrollViewer_ScrollChanged;
            ((ScrollViewer)sender).Unloaded -= ScrollViewer_Unloaded;
        }

        private async Task LoadFeed(StackPanel stackPanel, bool ignore)
        {
            if (!stillLoading)
            {
                stillLoading = true;
                if (stackPanel.Children.Count > 0 && !ignore)
                {
                    return;
                }
                ATUri AtUri;
                JArray feedlist;
                try
                {
                    AtUri = ATUri.Create(feedUriString[FeedTabControl.SelectedIndex]);
                    Result<GetFeedOutput> test = await aTProtocol.GetFeedAsync(AtUri, 10, feedCursor[FeedTabControl.SelectedIndex]);
                    feedlist = JArray.Parse(JObject.Parse(test.Value.ToString())["feed"].ToString());
                    for (int i = 0; i < feedlist.Count; i++)
                    {
                        JObject postdata = JObject.Parse(feedlist[i].ToString());
                        Post post = new Post(postdata, dashboard, aTProtocol, false, false, false);
                        stackPanel.Children.Add(post);
                    }
                    // MessageBox.Show(feedCursor);
                    // I think i'd be a lot happier if I didn't force myself to work on this constantly.
                    // It's not that I hate this project, it's just that I've spent 98% of the day working on it alone.
                    // My fault for teasing this project when all there was was the ability to log in and a half finished dashboard.
                    // MessageBox.Show(JObject.Parse(test.Value.ToString())["cursor"].ToString());
                    feedCursor[FeedTabControl.SelectedIndex] = JObject.Parse(test.Value.ToString())["cursor"].ToString();
                    stillLoading = false;
                }
                catch
                {
                    try
                    {
                        Result<GetTimelineOutput> test = await aTProtocol.GetTimelineAsync(feedUriString[FeedTabControl.SelectedIndex], 10, feedCursor[FeedTabControl.SelectedIndex]);
                        feedlist = JArray.Parse(JObject.Parse(test.Value.ToString())["feed"].ToString());
                        // System.Windows.Forms.Clipboard.SetText(feedlist.ToString());
                        for (int i = 0; i < feedlist.Count; i++)
                        {
                            JObject postdata = JObject.Parse(feedlist[i].ToString());
                            Post post = new Post(postdata, dashboard, aTProtocol, false, false, false);
                            _ = stackPanel.Children.Add(post);
                        }
                        if (feedlist.Count > 0)
                        {
                            DateTime dateTime;
                            if (feedlist[feedlist.Count - 1]["reason"] != null)
                            {
                                dateTime = (DateTime)feedlist[feedlist.Count - 1]["reason"]["indexedAt"];
                            }
                            else
                            {
                                dateTime = (DateTime)feedlist[feedlist.Count - 1]["post"]["record"]["createdAt"];
                            }
                            feedCursor[FeedTabControl.SelectedIndex] = dateTime.ToString("yyyy-MM-dd") + "T" + dateTime.ToString("HH:mm:ss") + "Z";
                        }
                        stillLoading = false;
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
                        stillLoading = false;
                    }
                }
                // lazy
                GC.Collect();
            }
        }

        private void WhatsUp_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // Fun fact, you cannot send a text post that just says "Whats up?", as it will be detected as empty.
            // I am really good at programming in case you couldn't tell.
            if (WhatsUp.Text == "Whats up?")
            {
                WhatsUp.Text = string.Empty;
            }
            WhatsUp.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void WhatsUp_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (WhatsUp.Text == string.Empty)
            {
                WhatsUp.Text = "Whats up?";
            }
            WhatsUp.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void Rectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _ = WhatsUpGrid.Focus();
        }
        private void WhatsUp_KeyUp(object sender, TextChangedEventArgs e)
        {
            if (canEdit)
            {
                CharCount.Content = WhatsUp.Text == "Whats up?" ? "300" : (object)(300 - WhatsUp.Text.Length);
                CharCount.Foreground = WhatsUp.Text.Length >= 300
                    ? new SolidColorBrush(Colors.IndianRed)
                    : WhatsUp.Text.Length >= 250 ? new SolidColorBrush(Colors.Goldenrod) : new SolidColorBrush(Colors.DimGray);
                if ((WhatsUp.Text != string.Empty && WhatsUp.Text != "Whats up?") || EmbedGet() != null)
                {
                    ClearPost.Visibility = Visibility.Visible;
                }
                else
                {
                    ClearPost.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void WhatsUpGrid_LayoutUpdated(object sender, System.EventArgs e)
        {
            if (canEdit)
            {
                FeedGrid.Margin = new Thickness(0, wrapper.ActualHeight, 0, 20);
            }
        }

        private void FeedTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canEdit)
            {

            }
        }

        private void Post_Click(object sender, RoutedEventArgs e)
        {
            if ((WhatsUp.Text != "" && WhatsUp.Text != "Whats up?") || attachedImages.Count > 0 || attachedVideo != null)
            {
                Post.Content = "Posting...";
                Post.IsEnabled = false;
                _ = PostMessage();
            }
            else
            {
                _ = MessageBox.Show("This post is empty", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private ATObject EmbedGet()
        {
            //  return attachedImages.Count > 0 ? new EmbedImages(attachedImages) : attachedVideo ?? (ATObject)null;
            if (quotePost != null)
            {
                if (EmbedGetOld() != null)
                {
                    return new RecordWithMedia(quotePost, EmbedGetOld());
                }
                else
                {
                    return quotePost;
                }
            }
            else if (attachedImages.Count > 0)
            {
                return new EmbedImages(attachedImages);
            }
            else if (attachedVideo != null)
            {
                return attachedVideo;
            }
            else
            {
                return null;
            }
        }
        private ATObject EmbedGetOld()
        {
            if (attachedImages.Count > 0)
            {
                return new EmbedImages(attachedImages);
            }
            else if (attachedVideo != null)
            {
                return attachedVideo;
            }
            else
            {
                return null;
            }
        }
        private async Task PostMessage()
        {
            Result<FishyFlip.Lexicon.Com.Atproto.Repo.CreateRecordOutput> postResult = await aTProtocol.Feed.CreatePostAsync(WhatsUp.Text, embed: EmbedGet());
            postResult.Switch(
                success =>
                {
                    WhatsUp.Text = "";
                    attachedImages.Clear();
                    attachedVideo = null;
                    WhatsUpImages.Children.Clear();
                    Quote.Visibility = Visibility.Collapsed;
                    quotePost = null;
                    ClearPost.Visibility = Visibility.Collapsed;
                    try
                    {
                        SoundPlayer soundPlayer = new SoundPlayer(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "POST"));
                        soundPlayer.Play();
                    }
                    catch
                    {

                    }
                },
                error =>
                {
                    _ = MessageBox.Show(error.Detail.Error + "\n\n" + error.Detail.Message + "\n\n" + error.Detail.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            );
            Post.Content = "Post";
            Post.IsEnabled = true;
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
                return rk == null ? "" : (string)rk.GetValue(key);
            }
            catch { return ""; }
        }
        private void MediaButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _ = UploadVideo();
        }
        private async Task UploadVideo()
        {
            if (attachedImages.Count == 0)
            {
                if (attachedVideo == null)
                {
                    if (canUpload)
                    {
                        canUpload = false;
                        using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
                        {
                            openFileDialog.Filter = "Video Files|*.mp4;*.webm;*.gif";
                            openFileDialog.RestoreDirectory = true;
                            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                string filePath = openFileDialog.FileName;
                                Stream fileStream = openFileDialog.OpenFile();
                                StreamContent content = new StreamContent(fileStream);
                                content.Headers.ContentLength = fileStream.Length;
                                if (Path.GetExtension(openFileDialog.FileName) == ".mp4")
                                {
                                    content.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
                                }
                                else if (Path.GetExtension(openFileDialog.FileName) == ".webm" || Path.GetExtension(openFileDialog.FileName) == ".jpg")
                                {
                                    content.Headers.ContentType = new MediaTypeHeaderValue("video/webm");
                                }
                                else if (Path.GetExtension(openFileDialog.FileName) == ".gif")
                                {
                                    content.Headers.ContentType = new MediaTypeHeaderValue("image/gif");
                                }
                                else
                                {
                                    _ = MessageBox.Show("This video format is not supported", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                Result<FishyFlip.Lexicon.Com.Atproto.Repo.UploadBlobOutput> blobResult = await aTProtocol.Repo.UploadBlobAsync(content);
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
                                await blobResult.SwitchAsync(
                                    async success =>
                                    {
                                        Grid grid = new Grid
                                        {
                                            HorizontalAlignment = HorizontalAlignment.Left,
                                            VerticalAlignment = VerticalAlignment.Top,
                                            Width = 96,
                                            Margin = new Thickness(0, 0, 10, 0)
                                        };
                                        System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle
                                        {
                                            Fill = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/res/ImageGradient.png")))
                                        };
                                        DropShadowEffect shadow = new DropShadowEffect
                                        {
                                            BlurRadius = 10,
                                            ShadowDepth = 5,
                                            Opacity = 0.33,
                                            Direction = 315
                                        };
                                        rect.Effect = shadow;
                                        _ = grid.Children.Add(rect);
                                        System.Windows.Shapes.Rectangle rect2 = new System.Windows.Shapes.Rectangle
                                        {
                                            Fill = new SolidColorBrush(Colors.White),
                                            Margin = new Thickness(5)
                                        };
                                        _ = grid.Children.Add(rect2);
                                        System.Windows.Controls.Image image1 = new System.Windows.Controls.Image
                                        {
                                            Source = new BitmapImage(new Uri("pack://application:,,,/res/previewnotavailable.png")),
                                            Margin = new Thickness(5)
                                        };
                                        _ = grid.Children.Add(image1);
                                        System.Windows.Controls.Image Close = new System.Windows.Controls.Image
                                        {
                                            Source = new BitmapImage(new Uri("pack://application:,,,/res/CloseSmall.png")),
                                            HorizontalAlignment = HorizontalAlignment.Right,
                                            VerticalAlignment = VerticalAlignment.Top,
                                            Stretch = Stretch.None,
                                            Margin = new Thickness(0, 10, 10, 0),
                                            Cursor = Cursors.Hand,
                                            ToolTip = "Detach video",
                                            Tag = grid
                                        };
                                        Close.MouseEnter += Close_MouseEnter;
                                        Close.MouseLeave += Close_MouseLeave;
                                        Close.MouseUp += (s, ee) => CloseVideo_MouseUp(grid);
                                        Close.Unloaded += CloseVideo_Unloaded;
                                        _ = grid.Children.Add(Close);
                                        _ = WhatsUpImages.Children.Add(grid);
                                        FishyFlip.Lexicon.App.Bsky.Embed.EmbedVideo video = new FishyFlip.Lexicon.App.Bsky.Embed.EmbedVideo(
                                        video: success.Blob,
                                        alt: "",
                                        aspectRatio: new AspectRatio(width: 16, height: 9));
                                        attachedVideo = video;
                                        ClearPost.Visibility = Visibility.Visible;
                                        WhatsUpImages.Visibility = Visibility.Visible;
                                    },
                                    async error =>
                                    {
                                        _ = MessageBox.Show($"Error: {error.StatusCode} {error.Detail}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                );
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                            }
                        }
                        canUpload = true;
                    }
                    else
                    {
                        _ = MessageBox.Show("Wait for your media to finish uploading", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    _ = MessageBox.Show("A video is already attached", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                _ = MessageBox.Show("Videos can't be attached onto a post if images are attached", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImagesButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _ = UploadImage();
        }
        private async Task UploadImage()
        {
            if (attachedImages.Count < 4)
            {
                if (attachedVideo == null)
                {
                    if (canUpload)
                    {
                        canUpload = false;
                        using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
                        {
                            openFileDialog.Filter = "Image Files|*.png;*.jpg;*.bmp;*.jpeg";
                            openFileDialog.RestoreDirectory = true;
                            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                string filePath = openFileDialog.FileName;
                                Stream fileStream = openFileDialog.OpenFile();
                                StreamContent content = new StreamContent(fileStream);
                                content.Headers.ContentLength = fileStream.Length;
                                if (Path.GetExtension(openFileDialog.FileName) == ".png")
                                {
                                    content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                                }
                                else if (Path.GetExtension(openFileDialog.FileName) == ".jpeg" || Path.GetExtension(openFileDialog.FileName) == ".jpg")
                                {
                                    content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                                }
                                else if (Path.GetExtension(openFileDialog.FileName) == ".bmp")
                                {
                                    content.Headers.ContentType = new MediaTypeHeaderValue("image/bmp");
                                }
                                else
                                {
                                    _ = MessageBox.Show("Image invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                Result<FishyFlip.Lexicon.Com.Atproto.Repo.UploadBlobOutput> blobResult = await aTProtocol.Repo.UploadBlobAsync(content);
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
                                await blobResult.SwitchAsync(
                                    async success =>
                                    {
                                        Grid grid = new Grid
                                        {
                                            HorizontalAlignment = HorizontalAlignment.Left,
                                            VerticalAlignment = VerticalAlignment.Top,
                                            Width = 96,
                                            Margin = new Thickness(0, 0, 10, 0)
                                        };
                                        System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle
                                        {
                                            Fill = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/res/ImageGradient.png")))
                                        };
                                        DropShadowEffect shadow = new DropShadowEffect
                                        {
                                            BlurRadius = 10,
                                            ShadowDepth = 5,
                                            Opacity = 0.33,
                                            Direction = 315
                                        };
                                        rect.Effect = shadow;
                                        _ = grid.Children.Add(rect);
                                        System.Windows.Shapes.Rectangle rect2 = new System.Windows.Shapes.Rectangle
                                        {
                                            Fill = new SolidColorBrush(Colors.White),
                                            Margin = new Thickness(5)
                                        };
                                        _ = grid.Children.Add(rect2);
                                        System.Windows.Controls.Image image1 = new System.Windows.Controls.Image
                                        {
                                            Source = new BitmapImage(new Uri(Path.GetFullPath(openFileDialog.FileName))),
                                            Margin = new Thickness(5)
                                        };
                                        _ = grid.Children.Add(image1);
                                        System.Windows.Controls.Image Close = new System.Windows.Controls.Image
                                        {
                                            Source = new BitmapImage(new Uri("pack://application:,,,/res/CloseSmall.png")),
                                            HorizontalAlignment = HorizontalAlignment.Right,
                                            VerticalAlignment = VerticalAlignment.Top,
                                            Stretch = Stretch.None,
                                            Margin = new Thickness(0, 10, 10, 0),
                                            Cursor = Cursors.Hand,
                                            ToolTip = "Detach image",
                                            Tag = grid
                                        };
                                        Close.MouseEnter += Close_MouseEnter;
                                        Close.MouseLeave += Close_MouseLeave;
                                        Close.Unloaded += Close_Unloaded;
                                        Close.MouseUp += (s, ee) => Close_MouseUp(grid);
                                        _ = grid.Children.Add(Close);
                                        _ = WhatsUpImages.Children.Add(grid);
                                        BitmapImage BitMapImage = new BitmapImage();
                                        BitMapImage.BeginInit();
                                        //BitmapEncoder must be set to GifBitmapEncoder to calculate the size of HD images (or higher), otherwise it will not return the correct result
                                        MemoryStream MemoryStream = ImageSourceToMemoryStream(new GifBitmapEncoder(), image1.Source);
                                        _ = MemoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                                        BitMapImage.StreamSource = MemoryStream;
                                        BitMapImage.EndInit();
                                        double aspectRatioed = BitMapImage.Width / BitMapImage.Height;
                                        FishyFlip.Lexicon.App.Bsky.Embed.Image image = new FishyFlip.Lexicon.App.Bsky.Embed.Image(
                                        image: success.Blob,
                                        alt: "",
                                        aspectRatio: new AspectRatio(width: (long)BitMapImage.Width, height: (long)BitMapImage.Height));
                                        attachedImages.Add(image);
                                        ClearPost.Visibility = Visibility.Visible;
                                        WhatsUpImages.Visibility = Visibility.Visible;
                                    },
                                    async error =>
                                    {
                                        _ = MessageBox.Show($"Error: {error.StatusCode} {error.Detail}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                );
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                            }
                        }
                        canUpload = true;
                    }
                    else
                    {
                        _ = MessageBox.Show("Wait for your media to finish uploading", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    _ = MessageBox.Show("Images can't be attached onto a post if a video is attached", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                _ = MessageBox.Show("Only 4 images can be attached onto a post", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
        private void Close_MouseEnter(object sender, MouseEventArgs e)
        {
            ((System.Windows.Controls.Image)sender).Source = new BitmapImage(new Uri("pack://application:,,,/res/CloseSmall2.png"));
        }

        private void Close_MouseLeave(object sender, MouseEventArgs e)
        {
            ((System.Windows.Controls.Image)sender).Source = new BitmapImage(new Uri("pack://application:,,,/res/CloseSmall.png"));
        }

        private void Close_Unloaded(object sender, RoutedEventArgs e)
        {
            ((System.Windows.Controls.Image)sender).MouseEnter -= Close_MouseEnter;
            ((System.Windows.Controls.Image)sender).MouseLeave -= Close_MouseLeave;
            ((System.Windows.Controls.Image)sender).MouseUp -= (s, ee) => Close_MouseUp((Grid)((System.Windows.Controls.Image)sender).Parent);
            ((System.Windows.Controls.Image)sender).Unloaded -= Close_Unloaded;
        }

        private void Close_MouseUp(Grid gridImage)
        {
            attachedImages.RemoveAt(((StackPanel)gridImage.Parent).Children.IndexOf(gridImage));
            gridImage.Children.Clear();
            ((StackPanel)gridImage.Parent).Children.RemoveAt(((StackPanel)gridImage.Parent).Children.IndexOf(gridImage));
            if (attachedImages.Count == 0)
            {
                WhatsUpImages.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseVideo_MouseUp(Grid gridImage)
        {
            attachedVideo = null;
            gridImage.Children.Clear();
            ((StackPanel)gridImage.Parent).Children.Clear();
            WhatsUpImages.Visibility = Visibility.Collapsed;
        }

        private void CloseVideo_Unloaded(object sender, RoutedEventArgs e)
        {
            ((System.Windows.Controls.Image)sender).MouseEnter -= Close_MouseEnter;
            ((System.Windows.Controls.Image)sender).MouseLeave -= Close_MouseLeave;
            ((System.Windows.Controls.Image)sender).MouseUp -= (s, ee) => CloseVideo_MouseUp((Grid)((System.Windows.Controls.Image)sender).Tag);
            ((System.Windows.Controls.Image)sender).Unloaded -= CloseVideo_Unloaded;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (((ScrollViewer)sender).ScrollableHeight - ((ScrollViewer)sender).VerticalOffset <= 320)
            {
                _ = LoadFeed((StackPanel)((ScrollViewer)sender).Content, true);
            }
        }

        private void Clear_MouseUp(object sender, MouseButtonEventArgs e)
        {
            WhatsUpGrid.Focus();
            WhatsUp.Text = "Whats up?";
            attachedImages.Clear();
            attachedVideo = null;
            WhatsUpImages.Children.Clear();
            Quote.Visibility = Visibility.Collapsed;
            quotePost = null;
            ClearPost.Visibility = Visibility.Collapsed;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= Page_Unloaded;
            WhatsUpGrid.LayoutUpdated -= WhatsUpGrid_LayoutUpdated;
            WhatsUp.LostKeyboardFocus -= WhatsUp_LostKeyboardFocus;
            WhatsUp.GotKeyboardFocus -= WhatsUp_GotKeyboardFocus;
            WhatsUp.TextChanged -= WhatsUp_KeyUp;
            Post.Click -= Post_Click;
            ImagesButton.MouseUp -= ImagesButton_MouseUp;
            MediaButton.MouseUp -= MediaButton_MouseUp;
            ClearPost.MouseEnter -= Close_MouseEnter;
            ClearPost.MouseLeave -= Close_MouseLeave;
            ClearPost.MouseUp -= Clear_MouseUp;
            FeedTabControl.SelectionChanged -= FeedTabControl_SelectionChanged;
            rect.MouseUp -= Rectangle_MouseUp;
            FeedTabControl.Items.Clear();
            FeedGrid.Children.Clear();
            QuotewrapPanel.Children.Clear();
            Quote.Children.Clear();
            aeh.Children.Clear();
            WhatsUpImages.Children.Clear();
            textwrapper.Children.Clear();
            wrapper.Children.Clear();
            grid.Children.Clear();
        }
    }
}
