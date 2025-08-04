using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FishyFlip;
using FishyFlip.Models;
using Newtonsoft.Json.Linq;

namespace Client
{
    /// <summary>
    /// Interaction logic for Feed.xaml
    /// </summary>
    public partial class Feed : UserControl
    {
        private readonly Dashboard dashboard;
        private readonly JObject feed;
        private readonly ATProtocol aTProtocol;
        private readonly ATDid ATDid;
        public Feed(JObject feed, Dashboard dashboard, ATProtocol aTProtocol)
        {
            InitializeComponent();
            this.dashboard = dashboard;
            this.feed = feed;
            this.aTProtocol = aTProtocol;
            _ = Load();
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task Load()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            try
            {
                Name.Text = feed["displayName"].ToString();
                Name.ToolTip = feed["displayName"].ToString();
            }
            catch
            {

            }
            Author.Text = "Feed by @" + feed["creator"]["handle"].ToString();
            Author.ToolTip = "@" + feed["creator"]["handle"].ToString();
            try
            {
                Desc.Text = feed["description"].ToString();
            }
            catch
            {
            }
            try
            {
                PFP.Source = new BitmapImage(new Uri(feed["avatar"].ToString()));
            }
            catch
            {

            }
        }
        private void SelectPost_MouseEnter(object sender, MouseEventArgs e)
        {
            SelectPost.Opacity = 100;
        }
        private void SelectPost_MouseLeave(object sender, MouseEventArgs e)
        {
            SelectPost.Opacity = 0;
        }
        private void SelectPost_MouseUp(object sender, MouseButtonEventArgs e)
        {
            dashboard.NavigateToFeed(feed["uri"].ToString(), feed);
        }
        private void Label_MouseEnter(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).TextDecorations.Add(TextDecorations.Underline);
        }
        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).TextDecorations.Clear();
        }
        public void UnloadUser()
        {
            // fix
            SelectPost.MouseEnter -= SelectPost_MouseEnter;
            SelectPost.MouseLeave -= SelectPost_MouseLeave;
            SelectPost.MouseUp -= SelectPost_MouseUp;
            Name.MouseEnter -= SelectPost_MouseEnter;
            Name.MouseLeave -= SelectPost_MouseLeave;
            Name.MouseUp -= SelectPost_MouseUp;
            Author.MouseEnter -= SelectPost_MouseEnter;
            Author.MouseLeave -= SelectPost_MouseLeave;
            Author.MouseUp -= SelectPost_MouseUp;
            Desc.MouseEnter -= SelectPost_MouseEnter;
            Desc.MouseLeave -= SelectPost_MouseLeave;
            Desc.MouseUp -= SelectPost_MouseUp;
            ((Grid)Content).Children.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
