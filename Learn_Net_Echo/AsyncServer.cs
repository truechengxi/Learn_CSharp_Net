using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;

namespace Learn_Net_Echo
{
    public class AsyncServer
    {
        
        class ClientState
        {
            public Socket socket;
            public byte[] readBuff = new byte[1024];
        }
        
        static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();

        static Socket listenfd;
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