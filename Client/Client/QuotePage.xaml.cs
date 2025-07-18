﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Lexicon.App.Bsky.Graph;
using FishyFlip.Models;
using Newtonsoft.Json.Linq;

namespace Client
{
    /// <summary>
    /// Interaction logic for QuotePage.xaml
    /// </summary>
    public partial class QuotePage : Page
    {
        private readonly ATUri Uri;
        private readonly ATProtocol aTProtocol;
        private readonly Dashboard dashboard;
        private string cursor = "";
        private bool isLoading = false;
        private readonly List<Post> posts = new List<Post>();
        public QuotePage(ATUri Uri, ATProtocol aTProtocol, Dashboard dashboard, int number)
        {
            InitializeComponent();
            this.Uri = Uri;
            this.aTProtocol = aTProtocol;
            this.dashboard = dashboard;
            Number.Text = number.ToString() + " quotes";
        }
        private async void Load()
        {
            if (!isLoading)
            {
                isLoading = true;
                try
                {
                    Result<GetQuotesOutput> test = await aTProtocol.GetQuotesAsync(Uri, null, 10, cursor);
                    JArray feedlist = JArray.Parse(JObject.Parse(test.Value.ToString())["posts"].ToString());
                    for (int i = 0; i < feedlist.Count; i++)
                    {
                        JObject postdata = JObject.Parse("{\"post\":" + feedlist[i].ToString() + "}");
                        Post post = new Post(postdata, dashboard, aTProtocol, false, false, false);
                        _ = ReplyStack.Children.Add(post);
                        posts.Add(post);
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
            dashboard.NavigateToPost(Uri.ToString());
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
