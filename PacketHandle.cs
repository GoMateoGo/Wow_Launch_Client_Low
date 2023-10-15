using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowLaunchApp
{
    class PacketHandle
    {
        // 调用处理消息的方法
        public static byte[] ProcessReceivedPacket(int id, string content)
        {
            if (id == 1 || id == 2 || id == 3)//接收消息路由, 显示出来
                Growl.Info(content);

            if (id == 4)
            {
                string url = "https://www.baidu.com"; // 替换为您要打开的网页URL
                if (content != null)
                {
                    url = content;
                }
                try
                {
                    Process.Start(url);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            //取系统信息,mac和os
            if (id == 101)
            {
                try
                {
                    string mac = Utils.GetMACAddress();

                    if (mac == "")
                    {
                        mac = "怀疑试图隐藏";
                    }

                    //获得操作系统版本
                    string osVersion = Utils.GetClientOs();

                    //组装信息
                    string result = mac + "#" + osVersion;


                    // 构建包
                    byte[] packet = Utils.BuildPacket(101, result);

                    // 发送包给服务器
                    return packet;

                }
                catch (Exception ex)
                {
                    // 处理发送消息错误
                    Console.WriteLine("发送消息错误：" + ex.Message);
                    return null;
                }
            }

            return null;
        }
    }
}
