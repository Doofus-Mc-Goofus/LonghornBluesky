﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FishyFlip;
using FishyFlip.Lexicon;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Lexicon.App.Bsky.Graph;
using FishyFlip.Lexicon.Com.Atproto.Repo;
using FishyFlip.Lexicon.Community.Lexicon.Interaction;
using FishyFlip.Models;
using M3U8Parser;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace Client
{
    /// <summary>
    /// Interaction logic for Post.xaml
    /// </summary>
    public partial class Post : UserControl
    {
        private readonly Dashboard dashboard;
        private readonly JObject post;
        private readonly ATProtocol aTProtocol;
        private readonly bool isFocused;
        private readonly bool isPinned;
        private bool isLiked;
        private bool isReposted;
        private readonly bool isReply;
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private readonly List<string> bitmapImages = new List<string>();
        private readonly List<string> altTexts = new List<string>();
        private readonly List<BitmapImage> bigmacImages = new List<BitmapImage>();
        private readonly List<Grid> bigmacGrid = new List<Grid>();
        private readonly List<Post> posts = new List<Post>();
        public Post(JObject post, Dashboard dashboard, ATProtocol aTProtocol, bool isFocused, bool isPinned, bool isReply)
        {
            InitializeComponent();
            this.dashboard = dashboard;
            this.post = post;
            this.aTProtocol = aTProtocol;
            this.isFocused = isFocused;
            this.isPinned = post["reason"] != null && post["reason"]["$type"].ToString() == "app.bsky.feed.defs#reasonPin";
            this.isReply = isReply;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, ee) => UpdateTime();
            timer.Start();
            _ = Load();
        }

        private void UpdateTime()
        {
            try
            {
                DateTime dateTime = (DateTime)post["post"]["record"]["createdAt"];
                TimeSpan time = DateTime.UtcNow.ToUniversalTime() - dateTime.ToUniversalTime();
                // fuck you
                if (time.TotalSeconds < 60)
                {
                    Time.Text = Math.Floor(time.TotalSeconds).ToString() + "s";
                    timer.Interval = TimeSpan.FromSeconds(10);
                }
                else if (time.TotalMinutes < 60)
                {
                    Time.Text = Math.Floor(time.TotalMinutes).ToString() + "m";
                    timer.Interval = TimeSpan.FromMinutes(1);
                }
                else if (time.TotalHours < 24)
                {
                    Time.Text = Math.Floor(time.TotalHours).ToString() + "h";
                    timer.Interval = TimeSpan.FromHours(1);
                }
                else if (time.TotalDays < 30)
                {
                    Time.Text = Math.Floor(time.TotalDays).ToString() + "d";
                    timer.Interval = TimeSpan.FromDays(1);
                }
                else if (time.TotalDays < 365)
                {
                    Time.Text = Math.Floor(time.TotalDays / 30).ToString() + "mo";
                    timer.Interval = TimeSpan.FromDays(1);
                }
                else
                {
                    Time.Text = dateTime.ToShortDateString();
                    timer.Interval = TimeSpan.FromDays(1);
                }
            }
            catch
            {

            }
        }

        private async Task Load()
        {
            isLiked = post["post"]["viewer"]["like"] != null;
            isReposted = post["post"]["viewer"]["repost"] != null;
            if (isLiked)
            {
                LikeGlow.Opacity = 100;
            }
            if (isReposted)
            {
                RepostGlow.Opacity = 100;
            }
            if (aTProtocol.Session.Did.Handler == post["post"]["author"]["did"].ToString())
            {
                MuteAccount.Visibility = Visibility.Collapsed;
                BlockAccount.Visibility = Visibility.Collapsed;
                ReportPost.Visibility = Visibility.Collapsed;
                EditInteraction.Visibility = Visibility.Visible;
                DeletePost.Visibility = Visibility.Visible;
            }
            UpdateTime();
            if (isReply)
            {
                TheBackground.Visibility = Visibility.Collapsed;
                wrapPanel.Margin = new Thickness(40, 46, 0, 32);
                statsWrapPanel.Margin = new Thickness(40, 0, 0, 10);
                ParentPost.Children.Clear();
            }
            else
            {
                if (post["reason"] == null)
                {
                    if (post["reply"] != null)
                    {
                        // ms pain
                        try
                        {
                            JObject postdata = JObject.Parse(JObject.Parse("{\"post\":" + post["reply"]["parent"] + "}").ToString());
                            Post postControl = new Post(postdata, dashboard, aTProtocol, false, false, true);
                            ParentPost.Children.Insert(0, postControl);
                            ParentPost.Visibility = Visibility.Visible;
                            posts.Add(postControl);
                        }
                        catch
                        {

                        }
                        try
                        {
                            if (post["reply"]["parent"]["cid"].ToString() != post["reply"]["root"]["cid"].ToString())
                            {
                                JObject rootpostdata = JObject.Parse(JObject.Parse("{\"post\":" + post["reply"]["root"] + "}").ToString());
                                Post rootpostControl = new Post(rootpostdata, dashboard, aTProtocol, false, false, true);
                                RootPost.Children.Insert(0, rootpostControl);
                                RootPost.Visibility = Visibility.Visible;
                                posts.Add(rootpostControl);
                            }
                        }
                        catch
                        {

                        }
                    }
                    else
                    {
                        if (post["parent"] == null)
                        {
                            RootPost.Children.Clear();
                            ParentPost.Children.Clear();
                        }
                    }
                }
                else
                {
                    RootPost.Children.Clear();
                    ParentPost.Children.Clear();
                }
            }
            if (post["parent"] != null)
            {
                try
                {
                    JObject postdata = JObject.Parse(post["parent"].ToString());
                    Post postControl = new Post(postdata, dashboard, aTProtocol, false, false, true);
                    posts.Add(postControl);
                    ParentPost.Children.Insert(0, postControl);
                    ParentPost.Visibility = Visibility.Visible;
                }
                catch
                {

                }
            }
            try
            {
                Username.Text = post["post"]["author"]["displayName"].ToString();
            }
            catch
            {
                // If there's no username attached in the post, it must have been deleted. Hide any clickable elements.
                Username.Visibility = Visibility.Collapsed;
                statsWrapPanel.Visibility = Visibility.Collapsed;
                Text.Text = "This post was deleted.";
                wrapPanel.Margin = new Thickness(0, 10, 0, 0);
                Frame.Visibility = Visibility.Collapsed;
                PFP.Visibility = Visibility.Collapsed;
                Time.Visibility = Visibility.Collapsed;
                More.Visibility = Visibility.Collapsed;
                timer.Stop();
            }
            Username.Tag = post["post"]["author"]["did"].ToString();
            Fullname.Text = post["post"]["author"]["handle"].ToString();
            RepliesNumb.Text = post["post"]["replyCount"].ToString();
            RepostNumb.Text = (int.Parse(post["post"]["quoteCount"].ToString()) + int.Parse(post["post"]["repostCount"].ToString())).ToString();
            LikeNumb.Text = post["post"]["likeCount"].ToString();
            try
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
                bitmapImage.UriSource = new Uri(post["post"]["author"]["avatar"].ToString());
                bitmapImage.EndInit();
                PFP.Source = bitmapImage;
                bigmacImages.Add(bitmapImage);
                if (bitmapImage.CanFreeze)
                {
                    bitmapImage.Freeze();
                }
            }
            catch
            {

            }
            try
            {
                Text.Text = post["post"]["record"]["text"].ToString();
            }
            catch
            {

            }
            if (Text.Text == string.Empty)
            {
                Text.Visibility = Visibility.Collapsed;
            }
            if (post["reason"] != null)
            {
                if (post["reason"]["$type"].ToString() == "app.bsky.feed.defs#reasonRepost")
                {
                    RepostedGrid.Visibility = Visibility.Visible;
                    Reposted.Text = post["reason"]["by"]["displayName"].ToString();
                    Reposted.Tag = post["reason"]["by"]["did"].ToString();
                    SelectPost.Margin = new Thickness(-15, -15, -15, 1);
                }
            }
            if (isPinned)
            {
                RepostedGrid.Visibility = Visibility.Visible;
                Reposted.Visibility = Visibility.Collapsed;
                RepostedBy.Text = "Pinned";
                RepostIcon.Source = new BitmapImage(new Uri("pack://application:,,,/res/PinnedIcon.png"));
                SelectPost.Margin = new Thickness(-15, -15, -15, 1);
            }
            if (!isFocused)
            {
                RepostNumb.MouseEnter -= Label_MouseEnter;
                RepostNumb.MouseLeave -= Label_MouseLeave;
                RepostNumb.MouseEnter += SelectPost_MouseEnter;
                RepostNumb.MouseLeave += SelectPost_MouseLeave;
                RepostNumb.MouseUp += SelectPost_MouseUp;
                RepostNumb.Cursor = Cursors.Arrow;
                LikeNumb.MouseEnter -= Label_MouseEnter;
                LikeNumb.MouseLeave -= Label_MouseLeave;
                LikeNumb.MouseEnter += SelectPost_MouseEnter;
                LikeNumb.MouseLeave += SelectPost_MouseLeave;
                LikeNumb.MouseUp += SelectPost_MouseUp;
                LikeNumb.Cursor = Cursors.Arrow;
            }
            if (post["post"]["embed"] != null)
            {
                Images.Visibility = Visibility.Visible;
                if (post["post"]["embed"]["$type"].ToString() == "app.bsky.embed.images#view")
                {
                    JArray jArray = JArray.Parse(post["post"]["embed"]["images"].ToString());
                    for (int i = 0; i < jArray.Count; i++)
                    {
                        Grid grid = new Grid
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(0, 0, 10, 0),
                            Width = jArray.Count > 2 ? 96 : 192,
                            Cursor = Cursors.Hand
                        };
                        grid.MouseUp += SelectImage;
                        bigmacGrid.Add(grid);
                        grid.Tag = (byte)i;
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
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
                        bitmapImage.UriSource = new Uri(jArray[i]["thumb"].ToString());
                        bitmapImage.EndInit();
                        bigmacImages.Add(bitmapImage);
                        // BitmapImage bitmapImage = new BitmapImage(new Did(jArray[i]["thumb"].ToString()))
                        // {
                        // CacheOption = BitmapCacheOption.OnDemand
                        // });
                        bitmapImages.Add(jArray[i]["fullsize"].ToString());
                        altTexts.Add(jArray[i]["alt"].ToString());
                        Image image1 = new Image
                        {
                            Source = bitmapImage,
                            Margin = new Thickness(5)
                        };
                        _ = grid.Children.Add(image1);
                        _ = Images.Children.Add(grid);
                        if (bitmapImage.CanFreeze)
                        {
                            bitmapImage.Freeze();
                        }
                    }
                }
                else if (post["post"]["embed"]["$type"].ToString() == "app.bsky.embed.video#view")
                {
                    // That one Microsoft employee that creates a bug in the MediaElement UIElement which leaves me unable to play video files streamed from a https url so I have to create a clunky cache system where the fragments of a video file are downloaded onto the hard drive and then stitched back together on the fly.
                    // Oopsies
                    HttpClient httpClient = new HttpClient();
                    System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    HttpResponseMessage playlistraw = await httpClient.GetAsync(post["post"]["embed"]["playlist"].ToString());
                    string playlist = await playlistraw.Content.ReadAsStringAsync();
                    MasterPlaylist masterPlaylist = MasterPlaylist.LoadFromText(playlist);
                    // This code should typically give us the highest resolution video possible. IDC if it's dumb, what matters is that it works.
                    string videoPlaylistUri = masterPlaylist.Streams[masterPlaylist.Streams.Count - 1].Uri;
                    HttpResponseMessage videoplaylistraw = await httpClient.GetAsync(post["post"]["embed"]["playlist"].ToString() + @"/../" + videoPlaylistUri);
                    string videoplaylist = await videoplaylistraw.Content.ReadAsStringAsync();
                    _ = MasterPlaylist.LoadFromText(videoplaylist);
                    Grid grid = new Grid
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(0, 0, 10, 0),
                        Width = 256
                    };
                    System.Windows.Shapes.Rectangle rect2 = new System.Windows.Shapes.Rectangle
                    {
                        Fill = new SolidColorBrush(Colors.Black),
                        Margin = new Thickness(30, 5, 30, 5)
                    };
                    _ = grid.Children.Add(rect2);
                    // MediaElement media = new MediaElement();
                    // media.Source = new Did(post["post"]["embed"]["playlist"].ToString() + @"/../720p/video0.ts" + "?dur=6.000000
                    // media.Margin = new Thickness(30, 5, 30, 5);
                    // media.Volume = 0;
                    // media.UnloadedBehavior = MediaState.Manual;
                    // media.MediaEnded += MediaElement_MediaEnded;
                    // _ = grid.Children.Add(media);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
                    bitmapImage.UriSource = new Uri(post["post"]["embed"]["thumbnail"].ToString());
                    bitmapImage.EndInit();
                    System.Windows.Controls.Image image1 = new System.Windows.Controls.Image
                    {
                        Source = bitmapImage,
                        Margin = new Thickness(30, 5, 30, 5)
                    };
                    bigmacImages.Add(bitmapImage);
                    _ = grid.Children.Add(image1);
                    System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle
                    {
                        Fill = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/res/VideoFrameWide.png")))
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
                    _ = Images.Children.Add(grid);
                    if (bitmapImage.CanFreeze)
                    {
                        bitmapImage.Freeze();
                    }
                }
                else if (post["post"]["embed"]["$type"].ToString() == "app.bsky.embed.record#view")
                {
                    Quote.Visibility = Visibility.Visible;
                    Images.Visibility = Visibility.Collapsed;
                    try
                    {
                        QuoteUsername.Text = post["post"]["embed"]["record"]["author"]["displayName"].ToString();
                        QuoteUsername.Tag = post["post"]["embed"]["record"]["author"]["did"].ToString();
                        QuoteText.Text = post["post"]["embed"]["record"]["value"]["text"].ToString();
                    }
                    catch
                    {
                        Quote.Visibility = Visibility.Collapsed;
                        DeletedQuote.Visibility = Visibility.Visible;
                    }
                    try
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
                        bitmapImage.UriSource = new Uri(post["post"]["embed"]["record"]["author"]["avatar"].ToString());
                        bitmapImage.EndInit();
                        QuotePFP.Source = bitmapImage;
                        bigmacImages.Add(bitmapImage);
                        if (bitmapImage.CanFreeze)
                        {
                            bitmapImage.Freeze();
                        }
                    }
                    catch
                    {

                    }
                }
                else if (post["post"]["embed"]["$type"].ToString() == "app.bsky.embed.recordWithMedia#view")
                {
                    Quote.Visibility = Visibility.Visible;
                    QuoteUsername.Text = post["post"]["embed"]["record"]["record"]["author"]["displayName"].ToString();
                    QuoteUsername.Tag = post["post"]["embed"]["record"]["record"]["author"]["did"].ToString();
                    QuoteText.Text = post["post"]["embed"]["record"]["record"]["value"]["text"].ToString();
                    try
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
                        bitmapImage.UriSource = new Uri(post["post"]["embed"]["record"]["record"]["author"]["avatar"].ToString());
                        bitmapImage.EndInit();
                        QuotePFP.Source = bitmapImage;
                        bigmacImages.Add(bitmapImage);
                        if (bitmapImage.CanFreeze)
                        {
                            bitmapImage.Freeze();
                        }
                    }
                    catch
                    {

                    }
                    if (post["post"]["embed"]["media"]["$type"].ToString() == "app.bsky.embed.images#view")
                    {
                        JArray jArray = JArray.Parse(post["post"]["embed"]["media"]["images"].ToString());
                        for (int i = 0; i < jArray.Count; i++)
                        {
                            Grid grid = new Grid
                            {
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                Margin = new Thickness(0, 0, 10, 0),
                                Width = jArray.Count > 2 ? 96 : 192,
                                Cursor = Cursors.Hand
                            };
                            grid.MouseUp += SelectImage;
                            bigmacGrid.Add(grid);
                            grid.Tag = (byte)i;
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
                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
                            bitmapImage.UriSource = new Uri(jArray[i]["thumb"].ToString());
                            bitmapImage.EndInit();
                            bigmacImages.Add(bitmapImage);
                            // BitmapImage bitmapImage = new BitmapImage(new Did(jArray[i]["thumb"].ToString()))
                            // {
                            // CacheOption = BitmapCacheOption.OnDemand
                            // });
                            bitmapImages.Add(jArray[i]["fullsize"].ToString());
                            altTexts.Add(jArray[i]["alt"].ToString());
                            System.Windows.Controls.Image image1 = new System.Windows.Controls.Image
                            {
                                Source = bitmapImage,
                                Margin = new Thickness(5)
                            };
                            _ = grid.Children.Add(image1);
                            _ = Images.Children.Add(grid);
                            if (bitmapImage.CanFreeze)
                            {
                                bitmapImage.Freeze();
                            }
                        }
                    }
                    else if (post["post"]["embed"]["media"]["$type"].ToString() == "app.bsky.embed.video#view")
                    {
                        // That one Microsoft employee that creates a bug in the MediaElement UIElement which leaves me unable to play video files streamed from a https url so I have to create a clunky cache system where the fragments of a video file are downloaded onto the hard drive and then stitched back together on the fly.
                        // Oopsies
                        HttpClient httpClient = new HttpClient();
                        System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                        HttpResponseMessage playlistraw = await httpClient.GetAsync(post["post"]["embed"]["media"]["playlist"].ToString());
                        string playlist = await playlistraw.Content.ReadAsStringAsync();
                        MasterPlaylist masterPlaylist = MasterPlaylist.LoadFromText(playlist);
                        // This code should typically give us the highest resolution video possible. IDC if it's dumb, what matters is that it works.
                        string videoPlaylistUri = masterPlaylist.Streams[masterPlaylist.Streams.Count - 1].Uri;
                        HttpResponseMessage videoplaylistraw = await httpClient.GetAsync(post["post"]["embed"]["media"]["playlist"].ToString() + @"/../" + videoPlaylistUri);
                        string videoplaylist = await videoplaylistraw.Content.ReadAsStringAsync();
                        _ = MasterPlaylist.LoadFromText(videoplaylist);
                        Grid grid = new Grid
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(0, 0, 10, 0),
                            Width = 256
                        };
                        System.Windows.Shapes.Rectangle rect2 = new System.Windows.Shapes.Rectangle
                        {
                            Fill = new SolidColorBrush(Colors.Black),
                            Margin = new Thickness(30, 5, 30, 5)
                        };
                        _ = grid.Children.Add(rect2);
                        // MediaElement media = new MediaElement();
                        // media.Source = new Did(post["post"]["embed"]["playlist"].ToString() + @"/../720p/video0.ts" + "?dur=6.000000
                        // media.Margin = new Thickness(30, 5, 30, 5);
                        // media.Volume = 0;
                        // media.UnloadedBehavior = MediaState.Manual;
                        // media.MediaEnded += MediaElement_MediaEnded;
                        // _ = grid.Children.Add(media);
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
                        bitmapImage.UriSource = new Uri(post["post"]["embed"]["media"]["thumbnail"].ToString());
                        bitmapImage.EndInit();
                        Image image1 = new Image
                        {
                            Source = bitmapImage,
                            Margin = new Thickness(30, 5, 30, 5)
                        };
                        bigmacImages.Add(bitmapImage);
                        _ = grid.Children.Add(image1);
                        System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle
                        {
                            Fill = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/res/VideoFrameWide.png")))
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
                        _ = Images.Children.Add(grid);
                        if (bitmapImage.CanFreeze)
                        {
                            bitmapImage.Freeze();
                        }
                    }
                    else
                    {
                        Images.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    Images.Visibility = Visibility.Collapsed;
                }
            }
            if (JArray.Parse(post["post"]["labels"].ToString()).Count > 0)
            {
                ContentHidden.Visibility = Visibility.Visible;
                wrapPanel.Children.Remove(Images);
                ContentHidden.Content = Images;
                Images.Margin = new Thickness(0, 0, 0, 5);
                string strings = string.Empty;
                for (int i = 0; i < JArray.Parse(post["post"]["labels"].ToString()).Count - 1; i++)
                {
                    strings += JArray.Parse(post["post"]["labels"].ToString())[0]["val"].ToString() + ", ";
                }
                strings += JArray.Parse(post["post"]["labels"].ToString())[0]["val"].ToString();
                ContentHidden.Header = "Content Hidden (" + strings + ")";
            }
        }

        private void SelectImage(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                List<Uri> listthings = new List<Uri>();
                for (byte i = 0; i < bitmapImages.Count; i++)
                {
                    listthings.Add(new Uri(bitmapImages[i]));
                }
                Viewer viewer = new Viewer(listthings, (byte)((Grid)sender).Tag, altTexts);
                viewer.Show();
            }
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

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            ((MediaElement)sender).Position = new TimeSpan(0, 0, 0, 0, 1);
            ((MediaElement)sender).Play();
        }

        private void SelectPost_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!isFocused)
            {
                SelectPost.Opacity = 100;
            }
        }
        private void SelectPost_MouseLeave(object sender, MouseEventArgs e)
        {
            SelectPost.Opacity = 0;
        }
        private void SelectPost_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!isFocused && e.ChangedButton == MouseButton.Left)
            {
                dashboard.NavigateToPost(post["post"]["uri"].ToString());
            }
        }
        private void SelectQuote_MouseEnter(object sender, MouseEventArgs e)
        {
            SelectQuotePost.Opacity = 100;
        }

        private void SelectQuote_MouseLeave(object sender, MouseEventArgs e)
        {
            SelectQuotePost.Opacity = 0;
        }
        private void SelectQuote_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (post["post"]["embed"]["record"]["record"] != null)
                {
                    dashboard.NavigateToPost(post["post"]["embed"]["record"]["record"]["uri"].ToString());
                }
                else
                {
                    dashboard.NavigateToPost(post["post"]["embed"]["record"]["uri"].ToString());
                }
            }
        }

        private async void Like_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Result<GetPostThreadOutput> thread = await aTProtocol.GetPostThreadAsync(ATUri.Create(post["post"]["uri"].ToString()));
            JToken thread2 = JObject.Parse(thread.Value.ToString())["thread"];
            if (isLiked)
            {
                Result<DeleteRecordOutput> result = await aTProtocol.Feed.DeleteLikeAsync(ATUri.Create(thread2["post"]["viewer"]["like"].ToString()).Rkey);
                result.Switch(
                    success =>
                    {
                        LikeNumb.Text = (int.Parse(LikeNumb.Text) - 1).ToString();
                        LikeGlow.Opacity = 0;
                        isLiked = false;
                    },
                    error =>
                    {
                        _ = MessageBox.Show($"Error: {error.StatusCode} {error.Detail}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                );
            }
            else
            {
                FishyFlip.Lexicon.App.Bsky.Feed.Like like = new FishyFlip.Lexicon.App.Bsky.Feed.Like
                {
                    CreatedAt = DateTime.Now,
                    Subject = StrongRef.FromJson(post["post"].ToString())
                };
                Result<CreateRecordOutput> result = await aTProtocol.Feed.CreateLikeAsync(like);
                result.Switch(
                    success =>
                    {
                        LikeNumb.Text = (int.Parse(LikeNumb.Text) + 1).ToString();
                        LikeGlow.Opacity = 100;
                        isLiked = true;
                    },
                    error =>
                    {
                        _ = MessageBox.Show($"Error: {error.StatusCode} {error.Detail}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                );
            }
        }

        private void Repost_Click(object sender, MouseButtonEventArgs e)
        {
            ((Image)sender).ContextMenu.IsOpen = true;
        }
        private void RepostContext_Click(object sender, MouseButtonEventArgs e)
        {
            if (isFocused)
            {
                ((TextBlock)sender).ContextMenu.IsOpen = true;
            }
        }
        private async void RepostContext(object sender, RoutedEventArgs e)
        {
            Result<GetPostThreadOutput> thread = await aTProtocol.GetPostThreadAsync(ATUri.Create(post["post"]["uri"].ToString()));
            JToken thread2 = JObject.Parse(thread.Value.ToString())["thread"];
            if (isReposted)
            {
                Result<DeleteRecordOutput> result = await aTProtocol.Feed.DeleteRepostAsync(ATUri.Create(thread2["post"]["viewer"]["repost"].ToString()).Rkey);
                result.Switch(
                    success =>
                    {
                        RepostNumb.Text = (int.Parse(RepostNumb.Text) - 1).ToString();
                        RepostGlow.Opacity = 0;
                        isReposted = false;
                    },
                    error =>
                    {
                        _ = MessageBox.Show($"Error: {error.StatusCode} {error.Detail}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                );
            }
            else
            {
                Repost repost = new Repost
                {
                    CreatedAt = DateTime.Now,
                    Subject = StrongRef.FromJson(post["post"].ToString())
                };
                Result<CreateRecordOutput> result = await aTProtocol.Feed.CreateRepostAsync(repost);
                result.Switch(
                    success =>
                    {
                        RepostNumb.Text = (int.Parse(RepostNumb.Text) + 1).ToString();
                        RepostGlow.Opacity = 100;
                        isReposted = true;
                    },
                    error =>
                    {
                        _ = MessageBox.Show($"Error: {error.StatusCode} {error.Detail}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                );
            }
        }

        private void QuoteContext(object sender, RoutedEventArgs e)
        {
            StrongRef quote = StrongRef.FromJson(post["post"].ToString());
            dashboard.QuotePost(new FishyFlip.Lexicon.App.Bsky.Embed.EmbedRecord(quote));
        }
        public void UnloadPost()
        {
            // The sun is leaking
            for (int i = 0; i < posts.Count; i++)
            {
                posts[i].UnloadPost();
            }
            for (int i = 0; i < bigmacGrid.Count; i++)
            {
                bigmacGrid[i].MouseUp -= SelectImage;
                bigmacGrid[i].Children.Clear();
            }
            RepostIcon.MouseEnter -= SelectPost_MouseEnter;
            RepostIcon.MouseLeave -= SelectPost_MouseLeave;
            RepostIcon.MouseUp -= SelectPost_MouseUp;
            RepostedBy.MouseEnter -= SelectPost_MouseEnter;
            RepostedBy.MouseLeave -= SelectPost_MouseLeave;
            RepostedBy.MouseUp -= SelectPost_MouseUp;
            RepostedBy.MouseEnter -= Label_MouseEnter;
            RepostedBy.MouseLeave -= Label_MouseLeave;
            RepostedBy.MouseUp -= Username_MouseUp;
            SelectPost.MouseEnter -= SelectPost_MouseEnter;
            SelectPost.MouseLeave -= SelectPost_MouseLeave;
            SelectPost.MouseUp -= SelectPost_MouseUp;
            Username.MouseEnter -= Label_MouseEnter;
            Username.MouseLeave -= Label_MouseLeave;
            Username.MouseUp -= Username_MouseUp;
            Time.MouseEnter -= SelectPost_MouseEnter;
            Time.MouseLeave -= SelectPost_MouseLeave;
            Time.MouseUp -= SelectPost_MouseUp;
            Fullname.MouseEnter -= Label_MouseEnter;
            Fullname.MouseLeave -= Label_MouseLeave;
            Fullname.MouseUp -= Username_MouseUp;
            Text.MouseEnter -= SelectPost_MouseEnter;
            Text.MouseLeave -= SelectPost_MouseLeave;
            Text.MouseUp -= SelectPost_MouseUp;
            SelectQuotePost.MouseEnter -= SelectQuote_MouseEnter;
            SelectQuotePost.MouseLeave -= SelectQuote_MouseLeave;
            SelectQuotePost.MouseUp -= SelectQuote_MouseUp;
            QuoteUsername.MouseEnter -= Label_MouseEnter;
            QuoteUsername.MouseLeave -= Label_MouseLeave;
            QuoteUsername.MouseUp -= Username_MouseUp;
            originallyposted.MouseEnter -= SelectQuote_MouseEnter;
            originallyposted.MouseLeave -= SelectQuote_MouseLeave;
            originallyposted.MouseUp -= SelectQuote_MouseUp;
            QuoteText.MouseEnter -= SelectQuote_MouseEnter;
            QuoteText.MouseLeave -= SelectQuote_MouseLeave;
            QuoteText.MouseUp -= SelectQuote_MouseUp;
            DeletedQuote.MouseEnter -= SelectPost_MouseEnter;
            DeletedQuote.MouseLeave -= SelectPost_MouseLeave;
            DeletedQuote.MouseUp -= SelectPost_MouseUp;
            RepostIconThingy.MouseUp -= Repost_Click;
            Repost.Click -= RepostContext;
            Quote1.Click -= QuoteContext;
            RepostNumb.MouseEnter -= Label_MouseEnter;
            RepostNumb.MouseLeave -= Label_MouseLeave;
            RepostNumb.MouseEnter -= SelectPost_MouseEnter;
            RepostNumb.MouseLeave -= SelectPost_MouseLeave;
            RepostNumb.MouseUp -= SelectPost_MouseUp;
            LikeIcon.MouseUp -= Like_MouseUp;
            LikeNumb.MouseEnter -= Label_MouseEnter;
            LikeNumb.MouseLeave -= Label_MouseLeave;
            LikeNumb.MouseEnter -= SelectPost_MouseEnter;
            LikeNumb.MouseLeave -= SelectPost_MouseLeave;
            LikeNumb.MouseUp -= SelectPost_MouseUp;
            More.MouseUp -= Repost_Click;
            timer.Tick -= (s, ee) => UpdateTime();
            BlockAccount.Click -= BlockAccount_Click;
            CopyPostTextItem.Click -= CopyPostText;
            CopyPostLinkItem.Click -= CopyPostLink;
            UnregisterName("More");
            statsWrapPanel.Children.Clear();
            DeletedQuote.Children.Clear();
            QuotewrapPanel.Children.Clear();
            aeh.Children.Clear();
            Quote.Children.Clear();
            Images.Children.Clear();
            wrapPanel.Children.Clear();
            brirb.Children.Clear();
            ParentPost.Children.Clear();
            RootPost.Children.Clear();
            RepostedGrid.Children.Clear();
            stackie.Children.Clear();
            ((Grid)Content).Children.Clear();
            for (int i = 0; i < bigmacImages.Count; i++)
            {
                DisposeImage(bigmacImages[i]);
                bigmacImages[i] = null;
            }
            GC.SuppressFinalize(this);
        }
        private void DisposeImage(BitmapImage image)
        {
            if (image != null)
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream(new byte[] { 0x0 }))
                    {
                        image.StreamSource = ms;
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        private async void ReportContext_Click(object sender, RoutedEventArgs e)
        {
            // fix
            Result<GetPostThreadOutput> result = await aTProtocol.GetPostThreadAsync(ATUri.Create(post["post"]["uri"].ToString()));
            result.Switch(
                success =>
                {
                    switch (success.Thread)
                    {
                        case ThreadViewPost tvp:
                            string fish = Text.Text.Substring(0, 50);
                            if (fish.Length == 50)
                            {
                                fish += "...";
                            }
                            ReportModeration reportModeration = new ReportModeration(tvp.Post.Record, fish, aTProtocol);
                            reportModeration.Show();
                            break;
                    }
                },
                error =>
                {

                });
        }

        private async void DeletePost_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this post? If you delete this post, you will not be able to recover it.", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                Result<DeleteRecordOutput> result2 = await aTProtocol.DeletePostAsync(aTProtocol.Session.Did, ATUri.Create(post["post"]["uri"].ToString()).Rkey);
                result2.Switch(
                    success =>
                    {
                        SoundPlayer soundPlayer = new SoundPlayer(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "DELETE"));
                        soundPlayer.Play();
                        if (post["parent"] != null)
                        {
                            dashboard.NavigateToPost(post["parent"]["post"]["uri"].ToString());
                        }
                        else if (post["reply"] != null)
                        {
                            if (post["reply"]["parent"] != null)
                            {
                                dashboard.NavigateToPost(post["reply"]["parent"]["uri"].ToString());
                            }
                        }
                        else
                        {
                            dashboard.NavigateToProfile(post["post"]["author"]["did"].ToString());
                        }
                    },
                    error =>
                    {
                        _ = MessageBox.Show(error.Detail.Error + "\n\n" + error.Detail.Message + "\n\n" + error.Detail.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
            }
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

        private void CopyPostText(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(Text.Text);
            _ = MessageBox.Show("Copied post text", "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CopyPostLink(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(post["post"]["uri"].ToString());
            _ = MessageBox.Show("Copied post link", "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BlockAccount_Click(object sender, RoutedEventArgs e)
        {
            Block block = new Block
            {
                CreatedAt = DateTime.Now,
                Subject = ATDid.Create(post["post"]["author"]["did"].ToString())
            };
            Result<CreateRecordOutput> result = await aTProtocol.CreateBlockAsync(block);
            result.Switch(
                success =>
                {
                    _ = MessageBox.Show("Account blocked", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    BlockAccount.Visibility = Visibility.Collapsed;
                },
                error =>
                {
                    _ = MessageBox.Show($"Error: {error.StatusCode} {error.Detail}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
        }

        private void ViewQuote_Click(object sender, RoutedEventArgs e)
        {
            dashboard.NavigateToQuotes(post["post"]["uri"].ToString(), int.Parse(post["post"]["quoteCount"].ToString()));
        }

        private void ViewRepost_Click(object sender, RoutedEventArgs e)
        {
            dashboard.NavigateToEngage(post["post"]["uri"].ToString(), 2, int.Parse(post["post"]["quoteCount"].ToString()));
        }

        private void Like_Click(object sender, MouseButtonEventArgs e)
        {
            dashboard.NavigateToEngage(post["post"]["uri"].ToString(), 3, int.Parse(post["post"]["likeCount"].ToString()));
        }
    }
}
