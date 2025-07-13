using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Client
{
    public partial class NotificationOverlay : Window
    {
        public double toolbarHeight = SystemParameters.PrimaryScreenHeight - SystemParameters.FullPrimaryScreenHeight - SystemParameters.WindowCaptionHeight;
        private readonly bool issignedIn = true;
        public NotificationOverlay()
        {
            InitializeComponent();
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
            UpdateMargins();
        }
        private void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            toolbarHeight = SystemParameters.PrimaryScreenHeight - SystemParameters.FullPrimaryScreenHeight - SystemParameters.WindowCaptionHeight;
            UpdateMargins();
        }
        public void CreateNotification(string Title, string Text, byte task = 255)
        {
            Toast toast = new Toast(Title, Text, this, task);
            _ = stack.Children.Add(toast);
        }
        public void RemoveToast(Toast toast, byte beebus)
        {
            stack.Children.Remove(toast);
            if (beebus != 255)
            {
                _ = FocusWindow(beebus);
            }
        }
        private async Task FocusWindow(byte notifType)
        {
            _ = Activate();
            _ = Focus();
            if (issignedIn)
            {
                if (notifType == 0)
                {
                    Process process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = @"explorer",
                            Arguments = @"https://github.com/Doofus-Mc-Goofus/LonghornBluesky/releases/latest"
                        }
                    };
                    _ = await Task.Run(process.Start);
                    await Task.Run(process.WaitForExit);
                    await Task.Run(process.Close);
                }
            }
        }
        private void UpdateMargins()
        {
            stack.Margin = new Thickness(SystemParameters.WorkArea.Left + 10, SystemParameters.WorkArea.Top + 10, SystemParameters.PrimaryScreenWidth - SystemParameters.WorkArea.Right + 10, SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Bottom + 10);
            stack.VerticalAlignment = SystemParameters.WorkArea.Top > SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Bottom
                ? VerticalAlignment.Top
                : VerticalAlignment.Bottom;
            stack.HorizontalAlignment = SystemParameters.WorkArea.Left > SystemParameters.PrimaryScreenWidth - SystemParameters.WorkArea.Right
                ? HorizontalAlignment.Left
                : HorizontalAlignment.Right;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            _ = SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }
        #region Window styles
        [Flags]
        public enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        public enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = -20,
            // ...
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            int error;
            IntPtr result;
            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                int tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            return (result == IntPtr.Zero) && (error != 0) ? throw new System.ComponentModel.Win32Exception(error) : result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int IntSetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);
        #endregion
    }
}
