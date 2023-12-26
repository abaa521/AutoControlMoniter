using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Threading;
namespace AutoControlMoniter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon;
        private bool IsTestVersion = false;

        public MainWindow()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
            CreateTrayIcon();


            NotShowWindow();
            DispatcherTimer timer = new DispatcherTimer();
            if (IsTestVersion)
            {
                timer.Interval = TimeSpan.FromSeconds(5);
            }
            else
            {
                timer.Interval = TimeSpan.FromSeconds(60);
            }
            
            timer.Tick += (sender, e) =>
            {
                if (IsTestVersion)
                {
                    ShowWindow();
                    return;
                }

                if (IsWorkingTime && IsShow)
                {
                    NotShowWindow();
                }
                else if (!IsWorkingTime)
                {
                    ShowWindow();
                }

                
            };
            timer.Start();
        }

        private void CreateTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon("monitor.ico"),
                Visible = true
            };
            _notifyIcon.DoubleClick += (sender, args) =>
            {
                if (IsShow)
                {
                    NotShowWindow();
                }
                else
                {
                    ShowWindow();
                }
            };

            _notifyIcon.Text = "AutoControlMoniter";

            // 创建上下文菜单
            System.Windows.Forms.ContextMenuStrip menu = new System.Windows.Forms.ContextMenuStrip();

            // 创建菜单项
            System.Windows.Forms.ToolStripMenuItem exitItem = new System.Windows.Forms.ToolStripMenuItem("Exit");

            // 添加点击事件处理器
            exitItem.Click += (sender, args) =>
            {
                System.Windows.Application.Current.Shutdown();
            };

            // 将菜单项添加到上下文菜单
            menu.Items.Add(exitItem);

            // 将上下文菜单关联到通知图标
            _notifyIcon.ContextMenuStrip = menu;

        }
        private bool IsShow = true;

        public bool IsWorkingTime
        {
            get
            {
                var now = DateTime.Now;
                var startOfWorkDay = new TimeSpan(8, 30, 0); // 早上九点
                var endOfWorkDay = new TimeSpan(18, 30, 0); // 晚上六点

                // 检查是否为周一至周五（平日）并且当前时间在工作时间范围内
                return now.DayOfWeek != DayOfWeek.Saturday &&
                       now.DayOfWeek != DayOfWeek.Sunday &&
                       now.TimeOfDay >= startOfWorkDay &&
                       now.TimeOfDay <= endOfWorkDay;
            }
        }

        private void ShowWindow()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {

                this.Visibility = Visibility.Visible; // 使窗口可见
                this.Activate(); // 激活窗口，使其成为前台窗口
                IsShow = true;
                SetTopmost();
                NativeMethods.SetCursorPos(2000, 2000);
            }));
        }

        private void NotShowWindow()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.Visibility = Visibility.Hidden; // 隐藏窗口
                IsShow = false;

                //NativeMethods.SetCursorPos(0, 0);
            }));
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _notifyIcon.Dispose();
        }
        private void SetTopmost()
        {
            var hWnd = new WindowInteropHelper(this).Handle;
            NativeMethods.SetWindowPos(hWnd, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, NativeMethods.TOPMOST_FLAGS);
        }


    }
    public class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        public const UInt32 SWP_NOSIZE = 0x0001;
        public const UInt32 SWP_NOMOVE = 0x0002;
        public const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;


    }
}