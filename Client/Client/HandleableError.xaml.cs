using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using FishyFlip.Models;
using INI;

namespace Client
{
    /// <summary>
    /// Interaction logic for HandleableError.xaml
    /// </summary>
    public partial class HandleableError : Page
    {
        public HandleableError(ATError error)
        {
            InitializeComponent();
            ErrorText.Content = "Error " + error.StatusCode;
            ErrorDetail.Content = error.Detail.Message;
            if (File.Exists("config.ini"))
            {
                IniFile myIni = new IniFile("config.ini");
                if (myIni.Read("MSN", "LHbsky") == "1")
                {
                    logo.Source = new BitmapImage(new Uri("pack://application:,,,/res/logoshad.png"));
                }
            }
        }

        private void Page_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ((Grid)Content).Children.Clear();
            Unloaded -= Page_Unloaded;
        }
    }
}
