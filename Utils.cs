using HandyControl.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace WowLaunchApp
{
    class Utils
    {
        //全局页面引用
        public static Dialog dialog;
        //服务器ip地址
        public static string Host = "";
        //登录器端口
        public static int SocketPort = 8888;
        //下载服务器端口
        public static int HttpPort = 9999;

        public Utils()
        {
            InitUtils();
        }

        public void InitUtils()
        {
            Host = "127.0.0.1";
            SocketPort = 8888;
            HttpPort = 9999;
        }

        private static Utils instance; // 单例实例

        public static Utils Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Utils();
                }
                return instance;
            }
        }

        public static string GetMACAddress()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            NetworkInterface activeInterface = interfaces.FirstOrDefault(
                iface => iface.OperationalStatus == OperationalStatus.Up &&
                         (iface.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                          iface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211));

            if (activeInterface != null)
            {
                PhysicalAddress address = activeInterface.GetPhysicalAddress();
                byte[] bytes = address.GetAddressBytes();

                string macAddress = string.Join(":", bytes.Select(b => b.ToString("X2")));
                return macAddress;
            }

            return string.Empty;

        }

        //获得客户端系统信息
        public static string GetClientOs()
        {
            string Os = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Os = RuntimeInformation.OSDescription;

                // Windows 操作系统
                Console.WriteLine("操作系统：" + RuntimeInformation.OSDescription);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Os = RuntimeInformation.OSDescription;
                // Linux 操作系统
                Console.WriteLine("操作系统：" + RuntimeInformation.OSDescription);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Os = RuntimeInformation.OSDescription;
                // macOS 操作系统
                Console.WriteLine("操作系统：" + RuntimeInformation.OSDescription);
            }
            return Os;
        }


        //检测端口是否冲突
        public bool IsPortInUse(int port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = ipGlobalProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endPoint in tcpEndPoints)
            {
                if (endPoint.Port == port)
                {
                    return true;
                }
            }

            return false;
        }

        //解析数据包
        public static bool TryParse(byte[] data, out int id, out string content)
        {
            id = 0;
            content = null;

            try
            {
                // 检查数据是否足够解析
                if (data.Length < 8)
                {
                    Growl.Error("包长度获取错误:", data.Length.ToString());
                    return false; // 数据不足，无法解析
                }

                // 读取包长度和包 ID
                int length = BitConverter.ToInt32(data, 0);
                id = BitConverter.ToInt32(data, 4);

                // 检查数据是否足够包含内容
                if (data.Length < 8 + length)
                {
                    string res = "数据不足,无法解析:" + data.Length.ToString() + length.ToString();
                    Growl.Error(res);
                    return false; // 数据不足，无法解析
                }

                // 读取包内容并将其转换为字符串（使用 UTF-8 编码）
                content = Encoding.UTF8.GetString(data, 8, length);

                return true; // 解析成功
            }
            catch (Exception)
            {
                return false; // 解析失败
            }
        }


        //封包
        public static byte[] BuildPacket(int id, string content)
        {
            try
            {
                byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                int length = contentBytes.Length;

                byte[] idBytes = BitConverter.GetBytes(id);
                byte[] lengthBytes = BitConverter.GetBytes(length);

                byte[] package = new byte[8 + length];
                lengthBytes.CopyTo(package, 0);
                idBytes.CopyTo(package, 4);
                contentBytes.CopyTo(package, 8);

                return package;
            }
            catch (Exception)
            {
                // 处理封包失败的情况
                return null;
            }
        }

        //获取一个文件的md5值
        public static string GetMD5Value()
        {
            try
            {
                string md5FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "md5.txt");

                if (!File.Exists(md5FilePath))
                {
                    return ""; // 文件不存在，直接返回空字符串
                }

                string md5Value = File.ReadAllText(md5FilePath);
                return md5Value;
            }
            catch (Exception)
            {
                Growl.Error("和服务器断开连接,请重新打开登录器");
                return "";
            }

        }

        //计算文件的md5值
        public static string CalculateMD5(string filePath)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        byte[] hashBytes = md5.ComputeHash(stream);
                        StringBuilder sb = new StringBuilder();
                        foreach (byte b in hashBytes)
                        {
                            sb.Append(b.ToString("x2"));
                        }
                        return sb.ToString();
                    }
                }
            }
            catch (Exception)
            {
                Growl.Error("和服务器断开连接,请重新打开登录器");
                return "";
            }

        }

        //启动wow.exe
        public static void StartProgram()
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "wow.exe"; // 替换为你要启动的程序的名称或路径
                process.Start();
                return;
            }
            catch (Exception)
            {
                Growl.Error("请将登录器放到游戏目录下.");
                return;
            }
        }
    }
}
