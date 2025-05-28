using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Learn_Net_Echo
{
    public class ChatServer
    {
        public class ClientStage
        {
            public Socket ClientSocket;
            public byte[] Buffer = new byte[1024];
        }

        private Socket socket;
        private Dictionary<Socket, ClientStage> clients = new Dictionary<Socket, ClientStage>();

        public void StartServer()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            BindIpAndPort("127.0.0.1", 8888);
        }

        public void BindIpAndPort(string ip, int port)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            socket.Bind(ipEndPoint);
            Listen();
        }

        public void Listen()
        {
            socket.Listen(10);
            Console.WriteLine("服务器启动成功");
            Accept();
        }

        public void Accept()
        {
            socket.BeginAccept(AcceptCallBack, socket);
        }

        private void AcceptCallBack(IAsyncResult ar)
        {
            var server = (Socket)ar.AsyncState;
            var client = server.EndAccept(ar);
            var clientStage = new ClientStage();
            clientStage.ClientSocket = client;
            clients.Add(client, clientStage);
            OnLineCallBack(clientStage.ClientSocket);
            client.BeginReceive(clientStage.Buffer, 0, clientStage.Buffer.Length, SocketFlags.None, ReceiveCallBack,
                clientStage);
            //再次启动Accept
            server.BeginAccept(AcceptCallBack, server);
        }

        private void OnLineCallBack(Socket client)
        {
            BroadCast(EncodeMessage(false, clients.Count.ToString()));
        }

        private void OffLineCallBack(Socket client)
        {
            BroadCast(EncodeMessage(false, clients.Count.ToString()));
        }

        private void ReceiveCallBack(IAsyncResult ar)
        {
            var clientStage = (ClientStage)ar.AsyncState;
            var client = clientStage.ClientSocket;
            var count = client.EndReceive(ar);
            if (count > 0)
            {
                var str = Encoding.UTF8.GetString(clientStage.Buffer, 0, count);
                str = str.Substring(0, str.Length - 1);
                Console.WriteLine($"{client.RemoteEndPoint} : {str}");
                var returnStr = $"{client.RemoteEndPoint} : {str}";
                BroadCast(EncodeMessage(true, returnStr));
                BroadCast(EncodeMessage(false, clients.Count.ToString()));
                // SendMessage(client,str);
                client.BeginReceive(clientStage.Buffer, 0, clientStage.Buffer.Length, SocketFlags.None, ReceiveCallBack,
                    clientStage);
            }
            else if (count == 0)
            {
                clients.Remove(client);
                OffLineCallBack(client);
                client.Close();
                Console.WriteLine("Client socket closed");
                return;
            }
        }

        private byte[] EncodeMessage(bool isUser, string message)
        {
            string sendMsg;
            if (isUser)
            {
                sendMsg = "[USER]" + message;
            }
            else
            {
                sendMsg = "[SYSTEM]" + message;
            }

            byte[] msgByte = Encoding.UTF8.GetBytes(sendMsg);
            short len = (short)msgByte.Length;
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                // 写入消息长度（2字节，大端序）
                bw.Write(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(len)));
                // 写入消息内容
                bw.Write(msgByte);
                return ms.ToArray();
            }
        }


        /// <summary>
        /// 广播给所有在线的用户
        /// </summary>
        /// <param name="data"></param>
        private void BroadCast(byte[] data)
        {
            foreach (var client in clients)
            {
                try
                {
                    client.Value.ClientSocket.Send(data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }
}