using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Client
{
    /// <summary>
    /// Interaction logic for Viewer.xaml
    /// </summary>
    public partial class Viewer : Window
    {
        private readonly List<BitmapImage> images;
        private byte selected;
        private double scale = 1;
        private readonly bool isSingle = false;
        public Viewer(List<BitmapImage> images, byte selected)
        {
            InitializeComponent();
            try
            {
                this.images = images;
                this.selected = selected;
                if (images.Count == 1)
                {
                    Slideshow.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerSlideshowDisabled.png"));
                    Back.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerBackDisabled.png"));
                    Next.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerForwardDisabled.png"));
                    isSingle = true;
                }
                TheImage.Source = images[selected];
                SizeChanged += (s, ee) => Update();
                TheActualImage.Source = images[selected];
                Error.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Error.Content = ex.Message;
            }
        }
        private void Update()
        {

        }

        private void TheImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (TheImage.ActualWidth > TheImage.ActualHeight)
            {
                if (TheImage.ActualWidth >= 960)
                {
                    scale = Math.Floor(TheImage.ActualWidth / 480);
                    TheActualImage.Width = TheImage.ActualWidth / scale;
                }
                else
                {
                    TheActualImage.Width = TheImage.ActualWidth;
                    scale = 1;
                }
            }
            else
            {
                if (TheImage.ActualHeight >= 720)
                {
                    scale = Math.Floor(TheImage.ActualHeight / 360);
                    TheActualImage.Width = TheImage.ActualWidth / scale;
                }
                else
                {
                    TheActualImage.Width = TheImage.ActualWidth;
                    scale = 1;
                }
            }
            Width = TheActualImage.Width + 10 + SystemParameters.WindowNonClientFrameThickness.Left + SystemParameters.WindowNonClientFrameThickness.Right;
            Height = (TheActualImage.Width / (TheImage.ActualWidth / TheImage.ActualHeight)) + 10 + SystemParameters.WindowNonClientFrameThickness.Top + SystemParameters.WindowNonClientFrameThickness.Bottom;
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TheActualImage.Width = TheImage.ActualWidth * (0.125 * Math.Pow(2, ZoomSlider.Value)) / scale;
        }

        private void Zoom_MouseEnter(object sender, MouseEventArgs e)
        {
            Zoom.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerZoomHover.png"));
        }

        private void Zoom_MouseLeave(object sender, MouseEventArgs e)
        {
            Zoom.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerZoomNormal.png"));
        }

        private void Zoom_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Zoom.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerZoomPressed.png"));
        }

        private void Zoom_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Zoom.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerZoomHover.png"));
            switch (ZoomSlider.Visibility)
            {
                case Visibility.Visible:
                    ZoomSlider.Visibility = Visibility.Collapsed;
                    ZoomBox.Visibility = Visibility.Collapsed;
                    break;
                case Visibility.Hidden:
                    break;
                case Visibility.Collapsed:
                    ZoomSlider.Visibility = Visibility.Visible;
                    ZoomBox.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }
        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Grid)sender).Background = Brushes.Transparent;
        }
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Grid)sender).Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/res/ViewerSelectHover.png")));
        }
        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((Grid)sender).Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/res/ViewerSelectPressed.png")));
        }
        private async void Download_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // TESTING THE CODE, HAVIN A BUG ...WHASSSSSSSSSSSSSSSSUUUUUP !
            ((Grid)sender).Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/res/ViewerSelectHover.png")));
            HttpClient httpClient = new HttpClient();
            Stream streamGot = await httpClient.GetStreamAsync(images[selected].UriSource.OriginalString.Replace("@jpeg", "@png"));
            Stream myStream;
            Microsoft.Win32.SaveFileDialog saveFileDialog1 = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PNG|*.png",
                DefaultExt = ".png",
                RestoreDirectory = true
            };

            if (saveFileDialog1.ShowDialog() == true)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    FileStream fileStream = new FileStream(Path.Combine(Path.GetDirectoryName(saveFileDialog1.FileName), Path.GetFileNameWithoutExtension(saveFileDialog1.FileName) + ".tmp"), FileMode.Create, FileAccess.Write);
                    streamGot.CopyTo(fileStream);
                    fileStream.Close();
                    myStream.Close();
                    _ = UpdateFile(saveFileDialog1.FileName);
                }
            }

        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task UpdateFile(string filename)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // probably sloppy and terrible
            File.Copy(Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".tmp"), filename, true);
            File.Delete(Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".tmp"));
        }
        private async void Print_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((Grid)sender).Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/res/ViewerSelectHover.png")));
            HttpClient httpClient = new HttpClient();
            Stream streamGot = await httpClient.GetStreamAsync(images[selected].UriSource.OriginalString.Replace("@jpeg", "@png"));
            string temp = Path.GetTempFileName();
            FileStream fileStream = new FileStream(temp, FileMode.Create, FileAccess.Write);
            streamGot.CopyTo(fileStream);
            fileStream.Close();
            ShellHelper.PrintPhotosWizard(temp);
        }

        private void Next_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!isSingle)
            {
                Next.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerForwardHover.png"));
            }
        }

        private void Next_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!isSingle)
            {
                Next.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerForwardNormal.png"));
            }
        }

        private void Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isSingle)
            {
                Next.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerForwardPressed.png"));
            }
        }

        private void Next_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!isSingle)
            {
                Next.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerForwardHover.png"));
                selected++;
                if (selected >= images.Count)
                {
                    selected = 0;
                }
                TheImage.Source = images[selected];
                TheActualImage.Source = images[selected];
            }
        }
        private void Back_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isSingle)
            {
                Back.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerBackPressed.png"));
            }
        }

        private void Back_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!isSingle)
            {
                Back.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerBackHover.png"));
            }
        }

        private void Back_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!isSingle)
            {
                Back.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerBackNormal.png"));
            }
        }

        private void Back_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!isSingle)
            {
                Back.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerBackHover.png"));
                selected--;
                if (selected >= images.Count)
                {
                    selected = (byte)(images.Count - 1);
                }
                TheImage.Source = images[selected];
                TheActualImage.Source = images[selected];
            }
        }

        private void Slideshow_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!isSingle)
            {
                Slideshow.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerSlideshowHover.png"));
            }
        }

        private void Slideshow_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!isSingle)
            {
                Slideshow.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerSlideshowNormal.png"));
            }
        }

        private void Slideshow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isSingle)
            {
                Slideshow.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerSlideshowPressed.png"));
            }
        }

        private void Slideshow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!isSingle)
            {
                Slideshow.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerSlideshowHover.png"));
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= Window_Unloaded;
            TheImage.SizeChanged -= TheImage_SizeChanged;
            Slideshow.MouseDown -= Slideshow_MouseDown;
            Slideshow.MouseUp -= Slideshow_MouseUp;
            Slideshow.MouseLeave -= Slideshow_MouseLeave;
            Slideshow.MouseEnter -= Slideshow_MouseEnter;
            Back.MouseEnter -= Back_MouseEnter;
            Back.MouseLeave -= Back_MouseLeave;
            Back.MouseDown -= Back_MouseDown;
            Back.MouseUp -= Back_MouseUp;
            Next.MouseEnter -= Next_MouseEnter;
            Next.MouseUp -= Next_MouseUp;
            Next.MouseDown -= Next_MouseDown;
            Next.MouseLeave -= Next_MouseLeave;
            Zoom.MouseEnter -= Zoom_MouseEnter;
            Zoom.MouseLeave -= Zoom_MouseLeave;
            Zoom.MouseDown -= Zoom_MouseDown;
            Zoom.MouseUp -= Zoom_MouseUp;
            ZoomSlider.ValueChanged -= ZoomSlider_ValueChanged;
            Download.MouseDown -= Button_MouseDown;
            Download.MouseLeave -= Button_MouseLeave;
            Download.MouseEnter -= Button_MouseEnter;
            Download.MouseUp -= Download_MouseUp;
            Print.MouseDown -= Button_MouseDown;
            Print.MouseLeave -= Button_MouseLeave;
            Print.MouseEnter -= Button_MouseEnter;
            Print.MouseUp -= Print_MouseUp;
            Download.Children.Clear();
            Print.Children.Clear();
            ((Grid)Content).Children.Clear();
        }
    }
}
