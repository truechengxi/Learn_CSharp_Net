using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

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

        class ClientState
        {
            public Socket socket;
            public byte[] readBuff = new byte[1024];
        }

        static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();

        static Socket listenfd;

        static void Main(string[] args)
        {
            //同步服务器
            // SyncServer syncServer = new SyncServer();
            // syncServer.StartServer();

            //异步服务器
            AsyncServer asyncServer = new AsyncServer();
            asyncServer.StartServer();
            
            // listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //
            // IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
            //
            // IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, 8888);
            //
            // listenfd.Bind(iPEndPoint);
            //
            // listenfd.Listen(0);
            //
            // Console.WriteLine("[服务器]启动成功");
            //
            // listenfd.BeginAccept(OnAcceptCallBack, listenfd);

            //while (true)
            //{
                //Console.WriteLine($"[服务器]接收到连接：{connfd.RemoteEndPoint}");
                //byte[] readBuff = new byte[1024];

                //int count = connfd.Receive(readBuff);

                //string readMessage = Encoding.Default.GetString(readBuff, 0, count);

                //Console.WriteLine($"[服务器]接收到消息：{readMessage}");

                //byte[] sendBytes = Encoding.Default.GetBytes(readMessage);

                //connfd.Send(sendBytes);
            //}

            Console.ReadKey();
        }

        private static void OnAcceptCallBack(IAsyncResult ar)
        {
            try
            {
                Socket listenfd = (Socket)ar.AsyncState;

                Socket clientfd = listenfd.EndAccept(ar);

                Console.WriteLine($"客户端连接：{clientfd.RemoteEndPoint}");

                ClientState state = new ClientState();

                state.socket = clientfd;
                
                clients.Add(clientfd, state);

                clientfd.BeginReceive(state.readBuff, 0, 1024, 0, OnReceiveCallBack, state);

                listenfd.BeginAccept(OnAcceptCallBack, listenfd);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"[服务器]接收连接失败：{ex.Message}");
            }
        }

        private static void OnReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                ClientState state = (ClientState)ar.AsyncState;

                var count = state.socket.EndReceive(ar);

                if (count == 0)
                {
                    Console.WriteLine($"[服务器]客户端断开连接：{state.socket.RemoteEndPoint}");
                    state.socket.Close();
                    clients.Remove(state.socket);
                    return;
                }

                string recvStr = Encoding.Default.GetString(state.readBuff, 0, count);

                Console.WriteLine($"收到来之：{state.socket.RemoteEndPoint}的消息：{recvStr}");

                byte[] sendBytes = Encoding.Default.GetBytes("Echo" + recvStr);

                state.socket.Send(sendBytes);

                state.socket.BeginReceive(state.readBuff, 0, 1024, 0, OnReceiveCallBack, state);

            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
                throw;
            }
        }
    }
}
