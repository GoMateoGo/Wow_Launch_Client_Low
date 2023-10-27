using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Interactivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
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
using WowLaunchApp.View;
using WowLaunchApp.ViewModule;

namespace WowLaunchApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private bool isDragging;
        private Point startPoint;
        public MainWindow()
        {
            InitializeComponent();


            // 将 PushMainWindow2Top 命令与对应的处理方法关联
            CommandBindings.Add(new CommandBinding(ControlCommands.PushMainWindow2Top, PushMainWindow2Top_Executed));
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized)
            {
                this.Hide();
                // 最小化窗口时显示托盘气球提示
                NotifyIcon.ShowBalloonTip("提示", "窗口已最小化", NotifyIconInfoType.Info, "消息");
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void PushMainWindow2Top_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // 当点击托盘图标时，显示窗口并将其置于顶层
            Show();
            Topmost = true;
        }

        //最小化窗体
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        //关闭窗体
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        
        //按下鼠标时,激活窗体可移动
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            startPoint = e.GetPosition(this);
        }

        //移动过程
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentPoint = e.GetPosition(this);
                double deltaX = currentPoint.X - startPoint.X;
                double deltaY = currentPoint.Y - startPoint.Y;

                this.Left += deltaX;
                this.Top += deltaY;
            }
        }

        //当释放鼠标时,释放移动窗体
        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                this.ReleaseMouseCapture();
            }
        }

        private void AccountMgrHandle(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            if (clickedButton.Content.ToString() == "注册账号")
            {
                AccountMgr.mgrType = 0;

                // 处理注册按钮点击事件
            }
            else if (clickedButton.Content.ToString() == "修改密码")
            {
                AccountMgr.mgrType = 1;
            }
            else if (clickedButton.Content.ToString() == "角色解卡")
            {
                AccountMgr.mgrType = 2;
            }

            //显示账号管理页面
            Utils.dialog = HandyControl.Controls.Dialog.Show(new AccountDialog());
        }

        private async void StartGame(object sender, RoutedEventArgs e)
        {
            string httpHost = Utils.Host;
            int httpPort = Utils.HttpPort;
            string downloadUrl = $"http://{httpHost}:{httpPort.ToString()}/down_load/";
            string checkUrl = $"http://{httpHost}:{httpPort}/check";
            string md5 = Utils.GetMD5Value();
            Growl.Success("检查更新中...");
            var res = await Http.PostRequest(checkUrl, md5);
            if (res)
            {
                Growl.Success("游戏启动中...");
                Utils.StartProgram();
                return;
            }
            ProBar.Visibility = Visibility.Visible;
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string savePaths = currentDirectory + "\\data\\zh-cn";
            await Http.DownloadAndExtractResource(downloadUrl, UpdateProgressBar, ModifyStartBtnStatus);
            Utils.StartProgram();
        }

        // 进度回调函数，用于更新进度条的值
        private void UpdateProgressBar(double progress)
        {
            // 在 UI 线程上更新进度条的值
            Dispatcher.Invoke(() => ProBarValue.Value = progress);
        }

        private void ModifyStartBtnStatus(bool status = true)
        {
            // 在 UI 线程上更新进度条的值
            Dispatcher.Invoke(() => StartWow.IsEnabled = status);
            if (status)
                StartWow.IsEnabled = true;
            else
                StartWow.IsEnabled = false;
        }


        //打开充值网站
        private void OpenPayUrl(object sender, RoutedEventArgs e)
        {
            SocketMgr.Instance.GetWebSiteUrl("1");
        }

        //打开网站首页
        private void OpenIndex(object sender, RoutedEventArgs e)
        {
            SocketMgr.Instance.GetWebSiteUrl("0");
        }

        private void ImageClick(object sender, MouseButtonEventArgs e)
        {
            string url = "https://www.getgamesf.com"; // 替换为您要打开的网页URL
            try
            {
                Process.Start(url);
            }
            catch (Exception)
            {
                return;
            }
        }

        // 在窗口关闭时执行的代码
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SocketMgr.Instance.CloseClient();
            
        }
    }
}
