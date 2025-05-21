using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Learn_Net_Echo
{
    /// <summary>
    /// 同步服务器
    /// </summary>
    public class SyncServer
    {
        string ip = "127.0.0.1";
        int port = 8888;
        private Socket _socket;

        public void StartServer()
        {
            BindIPAndPort();
        }

        //1.绑定IP和端口(Port)
        private void BindIPAndPort()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdr = IPAddress.Parse(ip);
            IPEndPoint endPoint = new IPEndPoint(ipAdr, port);
            _socket.Bind(endPoint);
            Listen();
        }

        //2.开启监听 Listen
        private void Listen()
        {
            _socket.Listen(10); //参数backlog指定队列中最多可容纳等待接受的连接数,0表示不限制.
            Console.WriteLine("服务器启动成功!");
            Accept();
        }

        //3.应答 Accept
        private void Accept()
        {
            while (true)
            {
                var connSocket = _socket.Accept();
                Console.WriteLine("[服务器]Accept");
                byte[] readBuffer = new byte[1024];
                var count = connSocket.Receive(readBuffer);
                string readStr = System.Text.Encoding.UTF8.GetString(readBuffer, 0, count);
                Console.WriteLine("[服务器接受]:"+readStr);
                Reply(connSocket, readStr);
            }
        }
        //4.回复
        private void Reply(Socket socket, string msg)
        {
            byte[] sendBuffer = Encoding.UTF8.GetBytes(msg);
            socket.Send(sendBuffer);
        }
    }
}