using HandyControl.Controls;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace WowLaunchApp
{
    class SocketMgr
    {
        public static Socket ClientWebSocket;        //socket
        private SocketClientAsync ClientLink;//客户端连接对象
        private string Client_IP = "127.0.0.1";//服务端IP地址
        private int Client_Port = 8888;//服务端监听的端口号
        private Thread Client_Td;//通讯内部使用线程
        private Thread listen;//通讯内部使用线程
        private bool ClientLinkRes = false;//服务器通讯状态标志

        private static SocketMgr instance; // 单例实例

        public static SocketMgr Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SocketMgr();
                }
                return instance;
            }
        }

        public SocketMgr()
        {
            Client_IP = Utils.Host;
            Client_Port = Utils.SocketPort;

        }

        /// <summary>
        /// 启动线程
        /// </summary>
        public void StartServer()
        {
            Client_Td = new Thread(LinkSocketSerFunc);
            Client_Td.Start();
            listen = new Thread(Listener);
        }

        private void Listener()
        {
            while (true)
            {
                if (!ClientWebSocket.Connected)
                {
                    StopApp();
                }
                Thread.Sleep(3000);
            }
        }
        /// <summary>
        /// 重连服务端线程
        /// </summary>
        private void LinkSocketSerFunc()
        {
            {
                try
                {
                    if (!ClientLinkRes)
                    {
                        ClientLink = new SocketClientAsync(Client_IP, Client_Port, 0);
                        ClientWebSocket = ClientLink.clientSocket;
                        ClientLink.OnMsgReceived += new SocketClientAsync.ReceiveMsgHandler(Client_OnMsgReceived);//绑定接受到服务端消息的事件
                        ClientLinkRes = ClientLink.ConnectServer();
                        listen.Start();
                    }
                    else
                    {
                        //此处写通讯成功的逻辑处理
                    }
                }
                catch (Exception ex)
                {
                    ClientLinkRes = false;
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }
        }
        public static void StopApp()
        {
            Growl.Error("服务端断开连接");
            HandyControl.Controls.MessageBox.Show("服务器断开连接", "连接错误", MessageBoxButton.OK, MessageBoxImage.Question);
            //Application.Current.Shutdown();
            Environment.Exit(0);
        }

        /// <summary>
        /// 接收消息处理
        /// </summary>
        /// <param name="info"></param>
        private void Client_OnMsgReceived(byte[] info, int i)
        {
            try
            {
                if (info.Length > 0 && info[0] != 0)//BCR连接错误NO
                {
                    //info为接受到服务器传过来的字节数组，需要进行什么样的逻辑处理在此书写便可   
                    if (Utils.TryParse(info, out int id, out string content))
                    {
                        //处理解包后的逻辑
                        var res = PacketHandle.ProcessReceivedPacket(id, content);
                        if (res != null)
                        {
                            ClientLink.Send(res);
                        }
                    }
                }
                else
                {
                    ClientLinkRes = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }


        /// <summary>
        /// 终止服务
        /// </summary>
        public void StopServer()
        {
            if (ClientLinkRes)
            {
                ClientLink.CloseLinkServer();
                ClientLink.Dispose();
            }
        }

        //注册
        public void RegisterAccount(string userName, string pwd, string code)
        {
            try
            {
                if (ClientWebSocket == null || !ClientWebSocket.Connected)
                {
                    //MessageBox.Show("服务器连接失败..");
                    Growl.Error("服务器连接失败..");
                    return;
                }
                string res = userName + "#" + pwd + "#" + code;
                // 构建包
                byte[] packet = Utils.BuildPacket(1, res);

                ClientWebSocket.Send(packet);
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.ToString());
            }

        }

        //修改/找回密码
        public void ChangePassword(string userName, string newPwd, string code)
        {
            try
            {
                if (ClientWebSocket == null || !ClientWebSocket.Connected)
                {
                    Growl.Error("服务器连接失败..");
                    return;
                }
                string res = userName + "#" + newPwd + "#" + code;
                // 构建包
                byte[] packet = Utils.BuildPacket(2, res);

                ClientWebSocket.Send(packet);
            }
            catch (Exception)
            {

            }

        }

        //角色解卡
        public void UnLockPlayer(string userName, string newPwd, string playerName)
        {
            try
            {
                if (ClientWebSocket == null || !ClientWebSocket.Connected)
                {
                    Growl.Error("服务器连接失败..");
                    return;
                }
                string res = userName + "#" + newPwd + "#" + playerName;
                // 构建包
                byte[] packet = Utils.BuildPacket(3, res);

                ClientWebSocket.Send(packet);
            }
            catch (Exception)
            {

            }
        }

        //获取网站/充值网站
        public void GetWebSiteUrl(string type)
        {
            try
            {
                if (ClientWebSocket == null || !ClientWebSocket.Connected)
                {
                    Growl.Error("服务器连接失败..");
                    return;
                }

                // 构建包
                byte[] packet = Utils.BuildPacket(4, type);

                ClientWebSocket.Send(packet);
            }
            catch (Exception)
            {

            }

        }
    }
}
