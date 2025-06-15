using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Learn_Net_Echo.ChatRoom;
using Learn_Net_Echo.DLD;

namespace Learn_Net_Echo
{
    internal class Program
    {
        /*
         * 服务器端：
         * 1.创建Socket对象
         * 2.绑定IP和端口
         * 3.监听 Listen
         * 4.接收连接 Accept
         * 5.接收数据 Receive
         * 6.发送数据 Send
         * 7.关闭连接 Close
         */

        static void Main(string[] args)
        {
            //同步Echo服务器
            // SyncServer syncServer = new SyncServer();
            // syncServer.StartServer();
            
            //异步聊天室服务器
            ChatServer chatServer = new ChatServer();
            chatServer.StartServer();
            
            //大乱斗案例服务器
            // DaLuanDouServer daLuanDouServer = new DaLuanDouServer();
            // daLuanDouServer.StartServer();

            Console.ReadKey();
        }
    }
}
