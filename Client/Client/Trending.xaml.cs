using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FishyFlip;
using FishyFlip.Models;
using Newtonsoft.Json.Linq;
namespace Client
{
    /// <summary>
    /// Interaction logic for Trending.xaml
    /// </summary>
    public partial class Trending : UserControl
    {
        private readonly Dashboard dashboard;
        private readonly JObject trending;
        private readonly ATProtocol aTProtocol;
        private readonly byte i;
        public Trending(JObject trending, Dashboard dashboard, ATProtocol aTProtocol, byte i)
        {
            InitializeComponent();
            this.dashboard = dashboard;
            this.trending = trending;
            this.aTProtocol = aTProtocol;
            this.i = i;
            _ = Load();
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task Load()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            TrendName.Text = i + ". " + trending["displayName"].ToString();
            TrendName.ToolTip = trending["displayName"].ToString();
            Details.Text = trending["postCount"].ToString() + " posts - " + trending["category"].ToString();
            Details.ToolTip = trending["postCount"].ToString() + " posts - " + trending["category"].ToString();
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
            // dashboard.NavigateToFeed(trending["link"].ToString(), null);
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
            TrendName.MouseEnter -= SelectPost_MouseEnter;
            TrendName.MouseLeave -= SelectPost_MouseLeave;
            TrendName.MouseUp -= SelectPost_MouseUp;
            Details.MouseEnter -= SelectPost_MouseEnter;
            ((Grid)Content).Children.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
