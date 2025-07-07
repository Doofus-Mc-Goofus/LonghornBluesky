using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Actor;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Lexicon.App.Bsky.Graph;
using FishyFlip.Lexicon.Com.Atproto.Repo;
using FishyFlip.Models;
using Newtonsoft.Json.Linq;

namespace Client
{
    /// <summary>
    /// Interaction logic for Profile.xaml
    /// </summary>
    public partial class Profile : Page
    {
        private readonly ATProtocol aTProtocol;
        private readonly ATDid ATDid;
        private readonly Session session;
        private readonly Dashboard dashboard;
        private ATUri raellycoolwig;
        private bool isbeingFollowed = false;
        private bool stillLoading = false;
        private readonly string[] cursors = new string[7];
        private bool isFollowing = false;
        public Profile(ATDid Uri, ATProtocol aTProtocol, Session session, Dashboard dashboard)
        {
            InitializeComponent();
            FeedGrid.Visibility = Visibility.Collapsed;
            ATDid = Uri;
            this.session = session;
            this.aTProtocol = aTProtocol;
            this.dashboard = dashboard;
            for (int i = 0; i < cursors.Length; i++)
            {
                cursors[i] = string.Empty;
            }
            _ = Load();
            // this.SizeChanged += Page_SizeChanged;
        }
        private async Task Load()
        {
            Result<ProfileViewDetailed> result = await aTProtocol.GetProfileAsync(ATDid);
            result.Switch(
            async success =>
            {
                Username.Text = success.DisplayName;
                Username.ToolTip = success.DisplayName;
                Fullname.Text = "@" + success.Handle.Handle;
                Fullname.ToolTip = "@" + success.Handle.Handle;
                Bio.Text = success.Description;
                try
                {
                    PFP.Source = new BitmapImage(new Uri(success.Avatar));
                }
                catch
                {

                }
                try
                {
                    Banner.Source = new BitmapImage(new Uri(success.Banner));
                }
                catch
                {

                }
                PostNum.Text = success.PostsCount.ToString();
                FolliwingNum.Text = success.FollowsCount.ToString();
                FollowersNum.Text = success.FollowersCount.ToString();
                if (ATDid.ToString() == session.Did.ToString())
                {
                    EditProf.Visibility = Visibility.Visible;
                    Follow.Visibility = Visibility.Collapsed;
                    MuteContext.Visibility = Visibility.Collapsed;
                    BlockContext.Visibility = Visibility.Collapsed;
                    ReportContext.Visibility = Visibility.Collapsed;
                    Likes.Visibility = Visibility.Visible;
                }
                else
                {
                    if (success.Viewer.Following != null)
                    {
                        Follow.Content = "Following";
                        raellycoolwig = success.Viewer.Following;
                        isFollowing = true;
                    }
                    if (success.Viewer.FollowedBy != null)
                    {
                        Follow.Content = "Follow Back";
                        isbeingFollowed = true;
                    }
                }
                FeedGrid.Visibility = Visibility.Visible;
                await GetPinnedPost(success);
                await LoadPosts(PostsStack);
            },
            error =>
            {
                _ = MessageBox.Show($"Error: {error.StatusCode} {error.Detail}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            );
        }
        private async Task GetPinnedPost(ProfileViewDetailed success)
        {
            try
            {
                Result<GetPostThreadOutput> pinnedpost = await aTProtocol.GetPostThreadAsync(success.PinnedPost.Uri, 0, 0);
                Post post = new Post(JObject.Parse(JObject.Parse(pinnedpost.Value.ToString())["thread"].ToString()), dashboard, aTProtocol, false, true, false);
                _ = PostsStack.Children.Add(post);
            }
            catch
            {

            }
            if (Bio.ActualHeight > 32)
            {
                ShowMore.Visibility = Visibility.Visible;
                Bio.MaxHeight = 32;
            }
        }
        private async Task LoadPosts(StackPanel stackPanel)
        {
            if (!stillLoading)
            {
                stillLoading = true;
                JArray feedlist = new JArray();
                JObject postresultparse = new JObject();
                if (FeedTabControl.SelectedIndex <= 3)
                {
                    Result<GetAuthorFeedOutput> postsresult = await aTProtocol.GetAuthorFeedAsync(ATDid, 10, cursors[FeedTabControl.SelectedIndex], GetFilterType(FeedTabControl.SelectedIndex), false);
                    feedlist = JArray.Parse(JObject.Parse(postsresult.Value.ToString())["feed"].ToString());
                    postresultparse = JObject.Parse(postsresult.Value.ToString());
                }
                else if (FeedTabControl.SelectedIndex == 4)
                {
                    Result<GetActorLikesOutput> postsresult = await aTProtocol.GetActorLikesAsync(ATDid, 10, cursors[FeedTabControl.SelectedIndex]);
                    feedlist = JArray.Parse(JObject.Parse(postsresult.Value.ToString())["feed"].ToString());
                    postresultparse = JObject.Parse(postsresult.Value.ToString());
                }
                else
                {

                }
                for (int i = 0; i < feedlist.Count; i++)
                {
                    JObject postdata = JObject.Parse(feedlist[i].ToString());
                    Post post = new Post(postdata, dashboard, aTProtocol, false, false, false);
                    _ = stackPanel.Children.Add(post);
                }
                DateTime dateTime = feedlist[feedlist.Count - 1]["reason"] != null
                    ? (DateTime)feedlist[feedlist.Count - 1]["reason"]["indexedAt"]
                    : (DateTime)feedlist[feedlist.Count - 1]["post"]["record"]["createdAt"];
                if (FeedTabControl.SelectedIndex <= 3)
                {
                    cursors[FeedTabControl.SelectedIndex] = dateTime.ToString("yyyy-MM-dd") + "T" + dateTime.ToString("HH:mm:ss") + "Z";
                }
                else if (FeedTabControl.SelectedIndex == 4)
                {
                    cursors[FeedTabControl.SelectedIndex] = postresultparse["cursor"].ToString();
                }
                stillLoading = false;
            }
        }
        private string GetFilterType(int id)
        {
            switch (id)
            {
                case 1:
                    return "posts_no_replies";
                case 2:
                    return "posts_with_replies";
                case 3:
                    return "posts_with_media";
                default:
                    return null;
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
        private void Rectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _ = Rectangular.Focus();
        }

        private void Overflow_Click(object sender, RoutedEventArgs e)
        {
            Overflow.ContextMenu.IsOpen = true;
        }
        private void Wrapper_LayoutUpdated(object sender, EventArgs e)
        {
            FeedGrid.Margin = new Thickness(20, wrapper.ActualHeight + 40, 20, 20);
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (((ScrollViewer)sender).ScrollableHeight - ((ScrollViewer)sender).VerticalOffset <= 320)
            {
                _ = LoadPosts((StackPanel)((ScrollViewer)sender).Content);
            }
        }

        private void ShowMore_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Bio.MaxHeight == double.PositiveInfinity)
            {
                ShowMore.Text = "Show more...";
                Bio.MaxHeight = 32;
            }
            else
            {
                ShowMore.Text = "Show less";
                Bio.MaxHeight = double.PositiveInfinity;
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Big banner
            Banner.Height = 136 * (wrapper.ActualWidth / 484);
            test.Margin = new Thickness(0, Banner.Height - 26, 0, 0);
        }

        private async void Follow_Click(object sender, RoutedEventArgs e)
        {
            if (isFollowing)
            {
                // I know this may come across as harsh to some, but I believe everyone in this story should die.
                Result<DeleteRecordOutput> result = await aTProtocol.DeleteFollowAsync(raellycoolwig.Did, raellycoolwig.Rkey);
                result.Switch(
                    success =>
                    {
                        Follow.Content = isbeingFollowed ? "Follow Back" : "Follow";
                        isFollowing = false;
                    },
                    error =>
                    {
                        _ = MessageBox.Show($"Error: {error.StatusCode} {error.Detail}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
            }
            else
            {
                Follow follow = new Follow
                {
                    CreatedAt = DateTime.Now,
                    Subject = ATDid
                };
                Result<CreateRecordOutput> result = await aTProtocol.CreateFollowAsync(follow);
                result.Switch(
                    success =>
                    {
                        Follow.Content = "Following";
                        isFollowing = true;
                        raellycoolwig = success.Uri;
                    },
                    error =>
                    {
                        _ = MessageBox.Show($"Error: {error.StatusCode} {error.Detail}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
            }
        }

        private void EditProf_Click(object sender, RoutedEventArgs e)
        {
            EditProf editProf = new EditProf(ATDid, aTProtocol, session, dashboard);
            dashboard.NavigateToProfileEdit(editProf);
        }

        private void BlockContext_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FollowersNum_MouseUp(object sender, MouseButtonEventArgs e)
        {
            dashboard.NavigateToFollow(ATDid.ToString(), false, int.Parse(FollowersNum.Text));
        }

        private void FolliwingNum_MouseUp(object sender, MouseButtonEventArgs e)
        {
            dashboard.NavigateToFollow(ATDid.ToString(), true, int.Parse(FolliwingNum.Text));
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // fix
            Unloaded -= Page_Unloaded;
            test.LayoutUpdated -= Wrapper_LayoutUpdated;
            Rectangular.MouseUp -= Rectangle_MouseUp;
            ShowMore.MouseEnter -= Label_MouseEnter;
            ShowMore.MouseLeave -= Label_MouseLeave;
            ShowMore.MouseUp -= ShowMore_MouseUp;
            EditProf.Click -= EditProf_Click;
            Follow.Click -= Follow_Click;
            Unblock.Click -= Follow_Click;
            Overflow.Click -= Overflow_Click;
            BlockContext.Click -= BlockContext_Click;
            PostsScroll.ScrollChanged -= ScrollViewer_ScrollChanged;
            RepliesScroll.ScrollChanged -= ScrollViewer_ScrollChanged;
            MediaScroll.ScrollChanged -= ScrollViewer_ScrollChanged;
            LikesScroll.ScrollChanged -= ScrollViewer_ScrollChanged;
            FeedsScroll.ScrollChanged -= ScrollViewer_ScrollChanged;
            StarterPacksScroll.ScrollChanged -= ScrollViewer_ScrollChanged;
            ListsScroll.ScrollChanged -= ScrollViewer_ScrollChanged;
            rect.MouseUp -= Rectangle_MouseUp;
            PostsStack.Children.Clear();
            RepliesStack.Children.Clear();
            MediaStack.Children.Clear();
            LikesStack.Children.Clear();
            FeedsStack.Children.Clear();
            StarterPacksStack.Children.Clear();
            ListsStack.Children.Clear();
            FeedTabControl.Items.Clear();
            test.Children.Clear();
            wrapper.Children.Clear();
            ((Grid)Content).Children.Clear();
        }
    }
}
