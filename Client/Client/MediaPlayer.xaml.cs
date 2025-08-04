using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Unosquare.FFME.Common;

namespace Client
{
    // fix ALL of this code
    /// <summary>
    /// Interaction logic for MediaPlayer.xaml
    /// </summary>
    public partial class MediaPlayer : Window
    {
        private bool isPaused = false;
        private string Vid360p = string.Empty;
        private string Vid720p = string.Empty;
        private int videoLength = 0;
        private readonly bool useHD = true;
        private readonly Uri url;
        private bool move = true;
        private bool repeat = false;
        private byte playState = 0;
        private bool isAero;
        private int bottommargin;
        public MediaPlayer(string altText, Uri url)
        {
            InitializeComponent();
            Unosquare.FFME.Library.FFmpegDirectory = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "bin");
            if (altText != string.Empty)
            {
                ImageTooltip.Text = altText;
            }
            else
            {
                Video.ToolTip = null;
            }
            this.url = url;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }
        private async Task LoadVideoPlaylist()
        {
            // saves the main playlist to the disk for later (this will be important later)
            HttpClient httpClient = new HttpClient();
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "playlist.m3u");
            Stream streamGot = await httpClient.GetStreamAsync(url);
            FileStream fileStream = new FileStream(Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".tmp"), FileMode.Create, FileAccess.Write);
            await streamGot.CopyToAsync(fileStream);
            fileStream.Close();
            UpdateFile(filePath);
            // splits the playlist into an array so that we can find the subplaylists and their data
            string[] listofthings = File.ReadAllText(filePath).Split(char.Parse("\n"));
            for (uint i = 2; i < listofthings.Length; i++)
            {
                if (listofthings[i].Contains("#EXT-X-STREAM-INF"))
                {
                    // gets the metadata and grabs the sub-playlist urls
                    string[] metadata = listofthings[i].Substring(listofthings[i].IndexOf(":")).Split(char.Parse(","));
                    if (metadata[metadata.Length - 1].Contains("720"))
                    {
                        Vid720p = listofthings[i + 1];
                    }
                    else if (metadata[metadata.Length - 1].Contains("360"))
                    {
                        Vid360p = listofthings[i + 1];
                    }
                }
            }
            await LoadVideo();
        }
        private async Task LoadVideo()
        {
            string newUrl = url + "/../" + (useHD ? Vid720p : Vid360p);
            HttpClient httpClient = new HttpClient();
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "video.m3u");
            Stream streamGot = await httpClient.GetStreamAsync(new Uri(newUrl));
            FileStream fileStream = new FileStream(Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".tmp"), FileMode.Create, FileAccess.Write);
            await streamGot.CopyToAsync(fileStream);
            fileStream.Close();
            UpdateFile(filePath);
            // split the sub-playlist into an array so that we can get the videos
            string[] listofthings = File.ReadAllText(filePath).Split(char.Parse("\n"));
            double videoLengthTemp = 0;
            for (uint i = 4; i < listofthings.Length; i++)
            {
                if (listofthings[i].Contains("#EXTINF"))
                {
                    // gets the metadata and grabs the sub-playlist urls
                    string metadata = listofthings[i].Substring(listofthings[i].IndexOf(":") + 1, 4);
                    videoLengthTemp += double.Parse(metadata);
                }
            }
            // we gotta create a new playlist because the regular one is incompatible
            string newPlaylist = "#EXTM3U\n#EXT-X-VERSION:3\n#EXT-X-PLAYLIST-TYPE:VOD\n#EXT-X-MEDIA-SEQUENCE:0\n#EXT-X-TARGETDURATION:6";
            videoLength = (int)TimeSpan.FromSeconds(videoLengthTemp).TotalMilliseconds;
            for (uint i = 4; i < listofthings.Length; i++)
            {
                if (listofthings[i].Contains("#EXTINF"))
                {
                    // listofthings[i + 1]
                    // add to the playlist
                    newPlaylist += "\n" + listofthings[i];
                    newPlaylist += "\n" + listofthings[i + 1].Substring(0, listofthings[i + 1].IndexOf("?"));
                    string filePath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), listofthings[i + 1].Substring(0, listofthings[i + 1].IndexOf("?")));
                    Stream streamGot2 = await httpClient.GetStreamAsync(new Uri(url + "/../" + (useHD ? "720p/" : "360p/") + listofthings[i + 1]));
                    FileStream fileStream2 = new FileStream(Path.Combine(Path.GetDirectoryName(filePath2), Path.GetFileNameWithoutExtension(filePath2) + ".tmp"), FileMode.Create, FileAccess.Write);
                    await streamGot2.CopyToAsync(fileStream2);
                    fileStream2.Close();
                    UpdateFile(filePath2);
                }
            }
            newPlaylist += "\n#EXT-X-ENDLIST";
            using (FileStream fs = File.Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vid.m3u")))
            {
                byte[] info = new UTF8Encoding().GetBytes(newPlaylist);
                fs.Write(info, 0, info.Length);
            }
            _ = await Video.Open(new Uri(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vid.m3u")));
        }
        private void UpdateFile(string filename)
        {
            // probably sloppy and terrible
            File.Copy(Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".tmp"), filename, true);
            File.Delete(Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".tmp"));
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            move = false;
            if (Video.Source != null && Video.NaturalDuration.HasValue)
            {
                if (physicalSlider.Value != (double)(Video.Position.TotalMilliseconds / Video.NaturalDuration.Value.TotalMilliseconds) && (Video.Position.TotalMilliseconds / Video.NaturalDuration.Value.TotalMilliseconds).ToString() != double.NaN.ToString())
                {
                    physicalSlider.Value = (double)(Video.Position.TotalMilliseconds / Video.NaturalDuration.Value.TotalMilliseconds);
                }
                Time.Text = string.Format("{0:mm\\:ss}", Video.Position);
                Time.Visibility = Width >= 480 ? Visibility.Visible : Visibility.Collapsed;
                TimeShadow.Visibility = Width >= 480 ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                physicalSlider.Value = 0;
                Time.Visibility = Visibility.Collapsed;
                TimeShadow.Visibility = Visibility.Collapsed;
                Time.Text = string.Empty;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;      // width of left border that retains its size
            public int cxRightWidth;     // width of right border that retains its size
            public int cyTopHeight;      // height of top border that retains its size
            public int cyBottomHeight;   // height of bottom border that retains its size
        };

        [DllImport("DwmApi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(
            IntPtr hwnd,
            ref MARGINS pMarInset);
        [DllImport("dwmapi.dll")]
        public static extern IntPtr DwmIsCompositionEnabled(out bool pfEnabled);
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtain the window handle for WPF application
                IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
                HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
                _ = DwmIsCompositionEnabled(out bool istheAero);
                isAero = istheAero;
                UpdateExtendedFrames();
                // Get System Dpi
                System.Drawing.Graphics desktop = System.Drawing.Graphics.FromHwnd(mainWindowPtr);
                float DesktopDpiX = desktop.DpiX;
                float DesktopDpiY = desktop.DpiY;
                // Set Margins
                bottommargin = Convert.ToInt32((70 + SystemParameters.WindowNonClientFrameThickness.Bottom) * (DesktopDpiX / 96));
                MARGINS margins = new MARGINS
                {
                    // Extend glass frame into client area
                    // Note that the default desktop Dpi is 96dpi. The  margins are
                    // adjusted for the system Dpi.
                    cxLeftWidth = 0,
                    cxRightWidth = 0,
                    cyTopHeight = 0,
                    cyBottomHeight = bottommargin
                };
                Video.Margin = new Thickness(0, 0, 0, bottommargin);
                Error.Margin = new Thickness(0, 0, 0, bottommargin);
                int hr = DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
                //
                if (hr < 0)
                {
                    //DwmExtendFrameIntoClientArea Failed
                }
            }
            // If not Vista, paint background white.
            catch (DllNotFoundException)
            {
                Application.Current.MainWindow.Background = Brushes.White;
            }
            if (HKCU_GetString(@"SOFTWARE\LonghornBluesky", "mediaVolume") != null)
            {
                VolumeSlider.Value = double.Parse(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "mediaVolume")) * 100;
                // Ugly hack becuase fuck MediaElement. It is the worst piece of code I have ever attempted to use (besides my own of course)
                Video.Volume = double.Parse(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "mediaVolume")) / 1.01;
            }
            if (HKCU_GetString(@"SOFTWARE\LonghornBluesky", "mediaRepeat") != null)
            {
                repeat = bool.Parse(HKCU_GetString(@"SOFTWARE\LonghornBluesky", "mediaRepeat"));
                RepeatIcon.Source = repeat
                    ? new BitmapImage(new Uri("pack://application:,,,/res/Repeat1.png"))
                    : new BitmapImage(new Uri("pack://application:,,,/res/Repeat0.png"));
            }
            if (Directory.Exists("bin"))
            {
                VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
                StopIcon.Source = new BitmapImage(new Uri("pack://application:,,,/res/Stop1.png"));
                // _ = await Video.Open(url);
                await LoadVideoPlaylist();
                _ = await Video.Play();
                Play.Source = new BitmapImage(new Uri("pack://application:,,,/res/MediaPauseNormal.png"));
            }
            else
            {
                Error.Text = "FFmpeg for Longhorn Bluesky is required to play this video\nYou'll have to reinstall Longhorn Bluesky";
            }
        }

        private void UpdateExtendedFrames()
        {
            IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
            HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
            mainWindowSrc.CompositionTarget.BackgroundColor = isAero ? Color.FromArgb(0, 185, 209, 234) : IsActive ? Color.FromArgb(255, 185, 209, 234) : Color.FromArgb(255, 215, 228, 242);
        }

        private void Window_Focus(object sender, EventArgs e)
        {
            UpdateExtendedFrames();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= Window_Unloaded;
            _ = Video.Close();
            ((Grid)Content).Children.Clear();
            GC.SuppressFinalize(this);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Right:
                    GoNext();
                    break;
                case Key.Left:
                    GoBack();
                    break;
                default:
                    break;
            }
        }

        private void Next_MouseEnter(object sender, MouseEventArgs e)
        {
            Next.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerForwardHover.png"));

        }

        private void Next_MouseLeave(object sender, MouseEventArgs e)
        {
            Next.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerForwardNormal.png"));

        }

        private void Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Next.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerForwardPressed.png"));

        }

        private void Next_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Next.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerForwardHover.png"));
            GoNext();
        }
        private void GoNext()
        {
            HideVolumeSlider();
            Video.Position += TimeSpan.FromSeconds(5);
        }
        private void Back_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Back.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerBackPressed.png"));

        }

        private void Back_MouseEnter(object sender, MouseEventArgs e)
        {
            Back.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerBackHover.png"));

        }

        private void Back_MouseLeave(object sender, MouseEventArgs e)
        {
            Back.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerBackNormal.png"));

        }

        private void Back_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Back.Source = new BitmapImage(new Uri("pack://application:,,,/res/ViewerBackHover.png"));
            GoBack();
        }
        private void GoBack()
        {
            HideVolumeSlider();
            if (Video.Visibility == Visibility.Collapsed)
            {
                isPaused = false;
                StopIcon.Source = new BitmapImage(new Uri("pack://application:,,,/res/Stop1.png"));
                Error.Text = "Loading...";
                Video.Visibility = Visibility.Visible;
                switch (playState)
                {
                    case 1:
                        Play.Source = new BitmapImage(new Uri("pack://application:,,,/res/MediaPauseHover.png"));
                        break;
                    case 2:
                        Play.Source = new BitmapImage(new Uri("pack://application:,,,/res/MediaPausePressed.png"));
                        break;
                    default:
                        Play.Source = new BitmapImage(new Uri("pack://application:,,,/res/MediaPauseNormal.png"));
                        break;
                }
            }
            Video.Position -= TimeSpan.FromSeconds(5);
        }
        private void Play_MouseEnter(object sender, MouseEventArgs e)
        {
            Play.Source = isPaused
                ? new BitmapImage(new Uri("pack://application:,,,/res/MediaPlayHover.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/MediaPauseHover.png"));
            playState = 1;
            Play.ToolTip = isPaused ? "Play" : "Pause";

        }

        private void Play_MouseLeave(object sender, MouseEventArgs e)
        {
            Play.Source = isPaused
                ? new BitmapImage(new Uri("pack://application:,,,/res/MediaPlayNormal.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/MediaPauseNormal.png"));
            playState = 0;
        }

        private void Play_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Play.Source = isPaused
                ? new BitmapImage(new Uri("pack://application:,,,/res/MediaPlayPressed.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/MediaPausePressed.png"));
            playState = 2;
        }

        private void Play_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                _ = Video.Pause();
            }
            else
            {
                if (Video.Visibility == Visibility.Collapsed)
                {
                    Error.Text = "Loading...";
                    Video.Position = new TimeSpan(0, 0, 0);
                }
                StopIcon.Source = new BitmapImage(new Uri("pack://application:,,,/res/Stop1.png"));
                _ = Video.Play();
                Video.Visibility = Visibility.Visible;
            }
            Play.Source = isPaused
                ? new BitmapImage(new Uri("pack://application:,,,/res/MediaPlayHover.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/MediaPauseHover.png"));
            HideVolumeSlider();
            playState = 1;
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

        private void Stop_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((Grid)sender).Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/res/ViewerSelectHover.png")));

            if (Directory.Exists("bin"))
            {
                HideVolumeSlider();
                _ = Video.Stop();
                StopIcon.Source = new BitmapImage(new Uri("pack://application:,,,/res/Stop0.png"));
                Error.Text = "This video has ended";
                isPaused = true;
                switch (playState)
                {
                    case 1:
                        Play.Source = new BitmapImage(new Uri("pack://application:,,,/res/MediaPlayHover.png"));
                        break;
                    case 2:
                        Play.Source = new BitmapImage(new Uri("pack://application:,,,/res/MediaPlayPressed.png"));
                        break;
                    default:
                        Play.Source = new BitmapImage(new Uri("pack://application:,,,/res/MediaPlayNormal.png"));
                        break;
                }
                Video.Visibility = Visibility.Collapsed;
            }
        }
        private void Repeat_MouseUp(object sender, MouseButtonEventArgs e)
        {
            HideVolumeSlider();
            repeat = !repeat;
            ((Grid)sender).Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/res/ViewerSelectHover.png")));
            RepeatIcon.Source = repeat
                ? new BitmapImage(new Uri("pack://application:,,,/res/Repeat1.png"))
                : new BitmapImage(new Uri("pack://application:,,,/res/Repeat0.png"));
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "mediaRepeat", repeat.ToString());
        }
        public void HKCU_AddKey(string path, string key, object value)
        {
            RegistryKey rk = Registry.CurrentUser.CreateSubKey(path);
            rk.SetValue(key, value);
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
        private async void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double prevVolumeSlider = VolumeSlider.Value;
            VolumeTooltip.Visibility = Visibility.Visible;
            VolumeTooltipText.Content = VolumeSlider.Value.ToString();
            Video.Volume = VolumeSlider.Value / 100;
            HKCU_AddKey(@"SOFTWARE\LonghornBluesky", "mediaVolume", (VolumeSlider.Value / 100).ToString());
            await Task.Delay(3000);
            if (VolumeSlider.Value == prevVolumeSlider)
            {
                VolumeTooltip.Visibility = Visibility.Collapsed;
            }
        }

        private void Volume_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Volume.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/res/VolumeSelectPressed.png")));
        }

        private void Volume_MouseEnter(object sender, MouseEventArgs e)
        {
            Volume.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/res/VolumeSelectHover.png")));
        }

        private void Volume_MouseLeave(object sender, MouseEventArgs e)
        {
            Volume.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/res/VolumeSelectNormal.png")));
        }

        private void Volume_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Volume.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/res/VolumeSelectHover.png")));
            switch (VolumeSlider.Visibility)
            {
                case Visibility.Visible:
                    HideVolumeSlider();
                    break;
                case Visibility.Hidden:
                    break;
                case Visibility.Collapsed:
                    VolumeSlider.Visibility = Visibility.Visible;
                    VolumeBox.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private void HideVolumeSlider()
        {
            VolumeSlider.Visibility = Visibility.Collapsed;
            VolumeBox.Visibility = Visibility.Collapsed;
            VolumeTooltip.Visibility = Visibility.Collapsed;
        }
        private void Video_MediaFailed(object sender, MediaFailedEventArgs e)
        {
            _ = MessageBox.Show(e.ErrorException.Message);
        }

        private void Video_MediaEnded(object sender, EventArgs e)
        {
            if (repeat)
            {
                Video.Position = new TimeSpan(0, 0, 0);
                StopIcon.Source = new BitmapImage(new Uri("pack://application:,,,/res/Stop1.png"));
                _ = Video.Play();
            }
            else
            {
                Error.Text = "This video has ended";
                StopIcon.Source = new BitmapImage(new Uri("pack://application:,,,/res/Stop0.png"));
                isPaused = true;
                switch (playState)
                {
                    case 1:
                        Play.Source = new BitmapImage(new Uri("pack://application:,,,/res/MediaPlayHover.png"));
                        break;
                    case 2:
                        Play.Source = new BitmapImage(new Uri("pack://application:,,,/res/MediaPlayPressed.png"));
                        break;
                    default:
                        Play.Source = new BitmapImage(new Uri("pack://application:,,,/res/MediaPlayNormal.png"));
                        break;
                }
                Video.Visibility = Visibility.Collapsed;
            }
        }

        private async void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (move)
            {
                ((Slider)sender).ToolTip = null;
                _ = await Video.Stop();
                Play.Source = new BitmapImage(new Uri("pack://application:,,,/res/MediaPlayNormal.png"));
                double timeSpan = ((Slider)sender).Value;
                await Task.Delay(250);
                if (((Slider)sender).Value == timeSpan)
                {
                    Video.Position = new TimeSpan(0, 0, 0, 0, (int)(Video.NaturalDuration.Value.TotalMilliseconds * ((Slider)sender).Value));
                    _ = await Video.Play();
                    StopIcon.Source = new BitmapImage(new Uri("pack://application:,,,/res/Stop1.png"));
                    Play.Source = new BitmapImage(new Uri("pack://application:,,,/res/MediaPauseNormal.png"));
                    ((Slider)sender).ToolTip = "Seek";
                }
            }
            move = true;
        }
    }
}
