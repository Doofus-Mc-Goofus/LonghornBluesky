using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Client
{
    /// <summary>
    /// Interaction logic for Toast.xaml
    /// </summary>
    public partial class Toast : UserControl
    {
        private readonly NotificationOverlay notificationOverlay;
        private readonly Stopwatch j = new Stopwatch();
        private readonly DispatcherTimer k = new DispatcherTimer();
        private float timeRemaining = 100;
        private Storyboard myStoryboard;
        private bool isClicked = false;
        private readonly byte task;
        public Toast(string Title, string Text, NotificationOverlay notificationOverlay, byte task)
        {
            InitializeComponent();
            this.task = task;
            Opacity = 0;
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(0.5))
            };
            CubicEase easingFunction = new CubicEase
            {
                EasingMode = EasingMode.EaseIn
            };
            myDoubleAnimation.EasingFunction = easingFunction;
            myStoryboard = new Storyboard();
            myStoryboard.Children.Add(myDoubleAnimation);
            Storyboard.SetTargetName(myDoubleAnimation, Name);
            Storyboard.SetTargetProperty(myDoubleAnimation,
                new PropertyPath(OpacityProperty));
            myStoryboard.Begin(this);
            this.notificationOverlay = notificationOverlay;
            this.Title.Text = Title;
            this.Text.Text = Text;
            j.Start();
            k.Interval = TimeSpan.FromMilliseconds(0);
            k.Tick += (s, ee) => Update();
            k.Start();
        }
        private void Update()
        {
            float deltaTime = (float)j.Elapsed.TotalMilliseconds;
            timeRemaining -= (float)0.01 * deltaTime;
            Progress.Value = timeRemaining;
            j.Restart();
            if (timeRemaining <= 0)
            {
                j.Stop();
                k.Stop();
                DoubleAnimation myDoubleAnimation = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5))
                };
                CubicEase easingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseIn
                };
                myDoubleAnimation.EasingFunction = easingFunction;
                myStoryboard = new Storyboard();
                // fix
                myStoryboard.Children.Add(myDoubleAnimation);
                Storyboard.SetTargetName(myDoubleAnimation, Name);
                Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(OpacityProperty));
                myStoryboard.Completed += (o, s) => Delete();
                myStoryboard.Begin(this);
            }
        }

        private void Delete()
        {
            if (isClicked)
            {
                notificationOverlay.RemoveToast(this, task);
            }
            else
            {
                notificationOverlay.RemoveToast(this, 255);
            }
            UnregisterName("control");
            Close.MouseUp -= MouseUp;
            UnregisterName("Close");
            Title.MouseUp -= MouseUp;
            UnregisterName("Title");
            Progress.MouseUp -= MouseUp;
            UnregisterName("Progress");
            Text.MouseUp -= MouseUp;
            UnregisterName("Text");
            ((Grid)Content).Children.Clear();
        }

        private void Close_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close.Source = new BitmapImage(new Uri("pack://application:,,,/res/ClosePressed.png"));
        }

        private void Close_MouseEnter(object sender, MouseEventArgs e)
        {
            Close.Source = new BitmapImage(new Uri("pack://application:,,,/res/CloseHover.png"));
        }

        private void Close_MouseLeave(object sender, MouseEventArgs e)
        {
            Close.Source = new BitmapImage(new Uri("pack://application:,,,/res/CloseNormal.png"));
        }

        private void Close_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Close.Source = new BitmapImage(new Uri("pack://application:,,,/res/CloseHover.png"));
            timeRemaining = 0;
        }

        private new void MouseUp(object sender, MouseButtonEventArgs e)
        {
            isClicked = true;
            timeRemaining = 0;
        }
    }
}
