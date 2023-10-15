using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WowLaunchApp
{
    class SocketClientAsync
    {
        #region 声明变量
        public string IPAdress;
        public bool connected = false;
        public Socket clientSocket;
        private IPEndPoint hostEndPoint;
        private int Flag = 0;
        private AutoResetEvent autoConnectEvent = new AutoResetEvent(false);
        private SocketAsyncEventArgs lisnterSocketAsyncEventArgs;

        public delegate void StartListeHandler();
        public event StartListeHandler StartListen;

        public delegate void ReceiveMsgHandler(byte[] info, int i);
        public event ReceiveMsgHandler OnMsgReceived;

        private List<SocketAsyncEventArgs> s_lst = new List<SocketAsyncEventArgs>();
        #endregion
        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="port"></param>
        /// <param name="i"></param>
        public SocketClientAsync(string hostName, int port, int i)
        {
            Flag = i;
            IPAdress = hostName;
            IPAddress[] hostAddresses = Dns.GetHostAddresses(hostName);
            this.hostEndPoint = new IPEndPoint(hostAddresses[hostAddresses.Length - 1], port);
            this.clientSocket = new Socket(this.hostEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }
        #endregion
        #region 开始连接服务端
        /// <summary>
        /// 连接服务端
        /// </summary>
        /// <returns></returns>
        private bool Connect()
        {
            using (SocketAsyncEventArgs args = new SocketAsyncEventArgs())
            {
                args.UserToken = this.clientSocket;
                args.RemoteEndPoint = this.hostEndPoint;
                args.Completed += new EventHandler<SocketAsyncEventArgs>(this.OnConnect);
                this.clientSocket.ConnectAsync(args);
                bool flag = autoConnectEvent.WaitOne(5000);
                if (this.connected)
                {
                    this.lisnterSocketAsyncEventArgs = new SocketAsyncEventArgs();
                    byte[] buffer = new byte[1024];
                    this.lisnterSocketAsyncEventArgs.UserToken = this.clientSocket;
                    this.lisnterSocketAsyncEventArgs.SetBuffer(buffer, 0, buffer.Length);
                    this.lisnterSocketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(this.OnReceive);
                    this.StartListen();
                    return true;
                }
                return false;
            }
        }
        #endregion
        #region 判断有没有连接上服务端
        /// <summary>
        /// 判断有没有连接上
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            this.connected = (e.SocketError == SocketError.Success);
            autoConnectEvent.Set();
        }
        #endregion
        #region 发送数据到服务端
        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="mes"></param>
        public void Send(byte[] packetData)
        {
            if (this.connected)
            {
                // 计算数据包的总长度
                int totalLength = packetData.Length;

                // 创建数据包
                byte[] buffer = new byte[totalLength];

                // 填充包体
                Array.Copy(packetData, 0, buffer, 0, packetData.Length);

                // 发送数据包
                SocketAsyncEventArgs senderSocketAsyncEventArgs = null;
                lock (s_lst)
                {
                    if (s_lst.Count > 0)
                    {
                        senderSocketAsyncEventArgs = s_lst[s_lst.Count - 1];
                        s_lst.RemoveAt(s_lst.Count - 1);
                    }
                }

                if (senderSocketAsyncEventArgs == null)
                {
                    senderSocketAsyncEventArgs = new SocketAsyncEventArgs();
                    senderSocketAsyncEventArgs.UserToken = this.clientSocket;
                    senderSocketAsyncEventArgs.RemoteEndPoint = this.clientSocket.RemoteEndPoint;
                }

                senderSocketAsyncEventArgs.SetBuffer(buffer, 0, buffer.Length);
                this.clientSocket.SendAsync(senderSocketAsyncEventArgs);
            }
            else
            {
                this.connected = false;
            }
        }

        #endregion
        #region 监听服务端
        /// <summary>
        /// 监听服务端
        /// </summary>
        public void Listen()
        {
            if (this.connected && this.clientSocket != null)
            {
                try
                {
                    (lisnterSocketAsyncEventArgs.UserToken as Socket).ReceiveAsync(lisnterSocketAsyncEventArgs);
                }
                catch (Exception)
                {
                }
            }
        }
        #endregion
        #region 断开服务端的连接
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        private int Disconnect()
        {
            int res = 0;
            try
            {
                this.clientSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
            }
            try
            {
                this.clientSocket.Close();
            }
            catch (Exception)
            {
            }
            this.connected = false;
            return res;
        }
        #endregion
        #region 数据接收
        /// <summary>
        /// 数据接受
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred == 0)
            {
                if (clientSocket.Connected)
                {
                    try
                    {
                        this.clientSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        if (this.clientSocket.Connected)
                        {
                            this.clientSocket.Close();
                        }
                    }
                }
                byte[] info = new Byte[] { 0 };
                this.OnMsgReceived(info, Flag);
            }
            else
            {
                byte[] buffer = new byte[e.BytesTransferred];
                Buffer.BlockCopy(e.Buffer, 0, buffer, 0, e.BytesTransferred);
                this.OnMsgReceived(buffer, Flag);
                Listen();
            }
        }

        #endregion
        #region 建立连接服务端的方法
        /// <summary>
        /// 建立连接的方法
        /// </summary>
        /// <returns></returns>
        public bool ConnectServer()
        {
            // 等待 2 秒
            Thread.Sleep(2000);
            bool flag = false;
            this.StartListen += new StartListeHandler(SocketClient_StartListen);
            // this.OnMsgReceived += new ReceiveMsgHandler(SocketClient_OnMsgReceived);
            flag = this.Connect();
            if (!flag)
            {
                return flag;
            }
            return true;
        }
        #endregion
        #region 关闭与服务端之间的连接
        /// <summary>
        /// 关闭连接的方法
        /// </summary>
        /// <returns></returns>
        public int CloseLinkServer()
        {
            return this.Disconnect();
        }
        #endregion
        #region 监听方法
        /// <summary>
        /// 监听的方法
        /// </summary>
        private void SocketClient_StartListen()
        {
            this.Listen();
        }
        #endregion
        #region IDispose member
        public void Dispose()
        {
            if (this.clientSocket.Connected)
            {
                this.clientSocket.Close();
            }
        }
        #endregion
        #region 析构函数
        ~SocketClientAsync()
        {
            try
            {
                if (this.clientSocket.Connected)
                {
                    this.clientSocket.Close();
                }
            }
            catch
            {

            }
            finally
            {

            }
        }
        #endregion
    }
}
