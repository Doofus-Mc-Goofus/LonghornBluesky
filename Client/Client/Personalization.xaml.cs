using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
using FishyFlip.Lexicon.Blue.Moji.Packs;
using FishyFlip.Lexicon.Com.Atproto.Sync;
using FishyFlip.Lexicon.Tools.Ozone.Moderation;

namespace Client
{
    /// <summary>
    /// Interaction logic for Personalization.xaml
    /// </summary>
    public partial class Personalization : Page
    {
        private readonly Settings settings;
        public Personalization(Settings settings)
        {
            InitializeComponent();
            this.settings = settings;
        }
        private void Rectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _ = rect.Focus();
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // fix
            Unloaded -= Page_Unloaded;
            rect.MouseUp -= Rectangle_MouseUp;
            homie.Children.Clear();
            ((Grid)Content).Children.Clear();
            Content = null;
            GC.SuppressFinalize(this);
        }
        private void ApplyStuff()
        {

        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            ApplyStuff();
            settings.GoBack();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            settings.GoBack();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            ApplyStuff();
        }

        private void LogCheck(object sender, RoutedEventArgs e)
        {
            LogSounds.Visibility = Visibility.Visible;
        }

        private void LogUncheck(object sender, RoutedEventArgs e)
        {
            LogSounds.Visibility = Visibility.Collapsed;
        }
    }
}
