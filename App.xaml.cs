using HandyControl.Controls;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WowLaunchApp
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private Mutex mutex;
        Utils utils = new Utils();
        protected override void OnStartup(StartupEventArgs e)
        {
            const string mutexName = "MyUniqueMutexName";

            // 创建一个互斥体，判断是否已经存在同名的互斥体
            mutex = new Mutex(true, mutexName, out bool createdNew);

            if (!createdNew)
            {
                Current.Shutdown();
                return;
            }

            // 在应用程序启动时执行初始化代码
            var socket = SocketMgr.Instance;
            socket.StartServer();

            base.OnStartup(e);
        }


        protected override void OnExit(ExitEventArgs e)
        {
            // 当应用程序退出时，释放互斥锁
            mutex?.ReleaseMutex();

            base.OnExit(e);
        }
    }
}
